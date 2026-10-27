using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;

namespace TarDistillation
{
    /// <summary>
    ///     Tar from pine wood, in the three steps the old tar burners used: split the
    ///     pitch-soaked stump wood into spik, dry it, then burn it slowly in a covered mile.
    ///     The plains tar pits are left untouched - this is a second way to get tar, not a
    ///     replacement, and it is gated behind the blast furnace so it cannot be done sooner.
    /// </summary>
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class TarDistillationPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "hakon.valheim.tardistillation";
        public const string PluginName = "TarDistillation";
        public const string PluginVersion = "0.0.2";

        /// <summary>Raw split tyri. Crafted, then dried.</summary>
        public const string SpikPrefab = "Tyrispik";

        /// <summary>Dried tyri. Comes out of the drying station, goes into the fermenter.</summary>
        public const string DriedSpikPrefab = "TyrispikDried";

        // Vanilla prefabs: cloned for the look, consumed, or produced.
        private const string CoreWoodPrefab = "RoundLog";
        private const string ResinPrefab = "Resin";
        private const string ElderBarkPrefab = "ElderBark";
        private const string TarPrefab = "Tar";

        internal static ModConfig Cfg { get; private set; }

        private static bool contentCreated;

        private Harmony harmony;

        private void Awake()
        {
            Cfg = new ModConfig(Config);
            ModLocalization.Register();

            PrefabManager.OnVanillaPrefabsAvailable += AddContent;

            // Amounts can be retuned without a restart. On a server the admin values arrive
            // through Jotunn config sync, before the client loads into the world.
            Cfg.ValuesChanged += ApplyAmounts;
            SynchronizationManager.OnConfigurationSynchronized += (sender, args) => ApplyAmounts();

            harmony = new Harmony(PluginGuid);
            harmony.PatchAll();
        }

        private void OnDestroy()
        {
            harmony?.UnpatchSelf();
        }

        private void AddContent()
        {
            PrefabManager.OnVanillaPrefabsAvailable -= AddContent;
            EnsureContent();
        }

        /// <summary>
        ///     Builds the items and conversions, once. Called from Jotunn's vanilla-prefabs
        ///     event, and again from an ObjectDB.Awake prefix as a fallback: that event hangs
        ///     off ObjectDB.CopyOtherDB, which only the main menu calls, and a dedicated server
        ///     has no main menu. Calling it twice is harmless, calling it too early is not, so
        ///     it backs out while the vanilla prefabs it clones are still missing.
        /// </summary>
        internal static void EnsureContent()
        {
            if (contentCreated || Cfg == null)
            {
                return;
            }

            if (PrefabManager.Instance.GetPrefab(CoreWoodPrefab) == null)
            {
                return;
            }

            contentCreated = true;

            try
            {
                AddSpik();
                AddDriedSpik();
                AddConversions();

                Jotunn.Logger.LogInfo(
                    $"Tar chain: {Cfg.CoreWoodPerBatch.Value} core wood + {Cfg.ResinPerBatch.Value} resin " +
                    $"to one spik, dried in {Smelters.GetInternalName(Cfg.DryingStation.Value)}, " +
                    $"fermented into {Cfg.TarPerBatch.Value} tar and {Cfg.CoalPerBatch.Value} coal");
            }
            catch (Exception ex)
            {
                Jotunn.Logger.LogError($"Could not register the tar chain: {ex}");
            }
        }

        /// <summary>Cleaning and splitting stump wood into spik. The only crafted step.</summary>
        private static void AddSpik()
        {
            var requirements = new List<RequirementConfig>
            {
                new RequirementConfig { Item = CoreWoodPrefab, Amount = Cfg.CoreWoodPerBatch.Value },
            };

            if (Cfg.ResinPerBatch.Value > 0)
            {
                requirements.Add(new RequirementConfig { Item = ResinPrefab, Amount = Cfg.ResinPerBatch.Value });
            }

            var spik = new CustomItem(SpikPrefab, CoreWoodPrefab, new ItemConfig
            {
                Name = ModLocalization.SpikName,
                Description = ModLocalization.SpikDescription,
                Amount = 1,
                Weight = 4f,
                StackSize = 50,
                CraftingStation = CraftingStations.GetInternalName(Cfg.SplittingStation.Value),
                Requirements = requirements.ToArray(),
            });

            ItemManager.Instance.AddItem(spik);
        }

        /// <summary>
        ///     Dried spik. Built without an ItemConfig on purpose: it must never be craftable,
        ///     the drying station is the only source.
        /// </summary>
        private static void AddDriedSpik()
        {
            var dried = new CustomItem(DriedSpikPrefab, ElderBarkPrefab);

            var shared = dried.ItemDrop.m_itemData.m_shared;
            shared.m_name = ModLocalization.DriedName;
            shared.m_description = ModLocalization.DriedDescription;
            shared.m_weight = 3f;
            shared.m_maxStackSize = 50;

            ItemManager.Instance.AddItem(dried);
        }

        private static void AddConversions()
        {
            // Drying. One for one by design: drying drives off water, it does not add wood.
            ItemManager.Instance.AddItemConversion(new CustomItemConversion(new SmelterConversionConfig
            {
                Station = Smelters.GetInternalName(Cfg.DryingStation.Value),
                FromItem = SpikPrefab,
                ToItem = DriedSpikPrefab,
            }));

            // The burn. The fermenter stands in for the mile: covered, slow, tapped at the bottom.
            ItemManager.Instance.AddItemConversion(new CustomItemConversion(new FermenterConversionConfig
            {
                Station = Fermenters.Fermenter,
                FromItem = DriedSpikPrefab,
                ToItem = TarPrefab,
                ProducedItems = Cfg.TarPerBatch.Value,
            }));
        }

        /// <summary>
        ///     Pushes changed amounts onto the live recipe and fermenter prefab. Station choices
        ///     are not re-applied here, those are read once at startup.
        /// </summary>
        private static void ApplyAmounts()
        {
            UpdateRecipe();
            UpdateFermenterYield();
        }

        private static void UpdateRecipe()
        {
            var objectDb = ObjectDB.instance;
            if (objectDb == null || objectDb.m_recipes == null)
            {
                return;
            }

            foreach (var recipe in objectDb.m_recipes)
            {
                if (recipe == null || recipe.m_item == null || recipe.m_item.name != SpikPrefab)
                {
                    continue;
                }

                foreach (var resource in recipe.m_resources)
                {
                    if (resource?.m_resItem == null)
                    {
                        continue;
                    }

                    if (resource.m_resItem.name == CoreWoodPrefab)
                    {
                        resource.m_amount = Cfg.CoreWoodPerBatch.Value;
                    }
                    else if (resource.m_resItem.name == ResinPrefab)
                    {
                        resource.m_amount = Cfg.ResinPerBatch.Value;
                    }
                }
            }
        }

        private static void UpdateFermenterYield()
        {
            var scene = ZNetScene.instance;
            if (scene == null)
            {
                return;
            }

            var prefab = scene.GetPrefab(Fermenters.Fermenter);
            var fermenter = prefab == null ? null : prefab.GetComponent<Fermenter>();
            if (fermenter?.m_conversion == null)
            {
                return;
            }

            foreach (var conversion in fermenter.m_conversion)
            {
                if (conversion?.m_from == null || conversion.m_from.name != DriedSpikPrefab)
                {
                    continue;
                }

                conversion.m_producedItems = Cfg.TarPerBatch.Value;
            }
        }
    }
}
