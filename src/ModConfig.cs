using System;
using BepInEx.Configuration;
using Jotunn.Configs;

namespace TarDistillation
{
    /// <summary>
    ///     Every tunable of the mod. Everything that touches the chain itself is admin only,
    ///     so Jotunn locks it on clients and pushes the server's values down on connect; see
    ///     README, "Multiplayer". The appearance section is the exception - it changes nothing
    ///     but what one machine draws, so it stays local and each player keeps their own.
    /// </summary>
    internal class ModConfig
    {
        /// <summary>Station needed to split a batch of tyrispik.</summary>
        public readonly ConfigEntry<string> SplittingStation;

        /// <summary>Core wood per batch of tyrispik.</summary>
        public readonly ConfigEntry<int> CoreWoodPerBatch;

        /// <summary>Resin per batch of tyrispik.</summary>
        public readonly ConfigEntry<int> ResinPerBatch;

        /// <summary>Station that dries the spik. Blast furnace by default: it is post-Moder,
        /// so distilling can never undercut a trip to the plains.</summary>
        public readonly ConfigEntry<string> DryingStation;

        /// <summary>Seconds to dry one spik. 0 keeps the station's own speed.</summary>
        public readonly ConfigEntry<int> DryingSeconds;

        /// <summary>Tar out of one tapped mile (fermenter).</summary>
        public readonly ConfigEntry<int> TarPerBatch;

        /// <summary>Charcoal left over by the burn. 0 turns the by-product off.</summary>
        public readonly ConfigEntry<int> CoalPerBatch;

        /// <summary>Seconds for one burn. 0 keeps the fermenter's own duration.</summary>
        public readonly ConfigEntry<int> BurningSeconds;

        /// <summary>Use the models and icons shipped in assets/, rather than the vanilla
        /// wood the items are cloned from. Local, and read once at startup.</summary>
        public readonly ConfigEntry<bool> CustomModels;

        /// <summary>Size of the shipped model where it lies on the ground. Local.</summary>
        public readonly ConfigEntry<float> ModelScale;

        /// <summary>Raised when a value that can be applied without a restart changes.</summary>
        public event Action ValuesChanged;

        public ModConfig(ConfigFile file)
        {
            const string sSplit = "1 - Splitting";
            const string sDry = "2 - Drying";
            const string sBurn = "3 - Burning";
            const string sLook = "4 - Appearance";

            SplittingStation = Bind(file, sSplit, "Splitting station", "ArtisanTable",
                "Crafting station the tyrispik recipe is attached to. ArtisanTable puts the " +
                "whole chain behind Moder, so tar is never cheaper than walking into the " +
                "plains for it; Workbench or Forge move it much earlier. Read once while the " +
                "game loads, so it needs a restart - of the server and of every client.",
                CraftingStations.GetAcceptableValueList());

            CoreWoodPerBatch = Bind(file, sSplit, "Core wood per batch", 10,
                "Core wood (kjerneved) consumed per batch of tyrispik. Written straight into " +
                "the live recipe, so a new value counts the next time you open the station - " +
                "no restart.",
                new AcceptableValueRange<int>(1, 100));

            ResinPerBatch = Bind(file, sSplit, "Resin per batch", 5,
                "Resin consumed per batch of tyrispik, on top of the core wood. Takes effect " +
                "without a restart, with one exception: 0 removes resin from the recipe " +
                "entirely, and adding or removing an ingredient only happens while the recipe " +
                "is being built, so that one waits for a restart.",
                new AcceptableValueRange<int>(0, 100));

            DryingStation = Bind(file, sDry, "Drying station", "BlastFurnace",
                "Smelter-type station that dries raw tyrispik into dried tyrispik, burning " +
                "fuel as it normally does. BlastFurnace is post-Moder and matches the default " +
                "splitting station; Smelter or CharcoalKiln make tar reachable in the black " +
                "forest instead. Read once while the game loads, so it needs a restart - of " +
                "the server and of every client.",
                Smelters.GetAcceptableValueList());

            DryingSeconds = Bind(file, sDry, "Drying seconds", 0,
                "Real seconds one tyrispik spends drying. 0 hands the timing back to the " +
                "station, which dries a spik as fast as it smelts a bar of metal. Examples: " +
                "60 is a minute per spik, 300 five minutes, 3600 an hour; the ceiling, 86400, " +
                "is a full day. Only tyrispik is affected - ore in the same furnace still " +
                "smelts at the vanilla speed, so you can retime the tar chain without " +
                "retiming the game. Takes effect immediately, on furnaces already running too.",
                new AcceptableValueRange<int>(0, 86400));

            TarPerBatch = Bind(file, sBurn, "Tar per batch", 25,
                "Tar you get for tapping the fermenter, per batch of dried tyrispik. The " +
                "default, 25, is half a stack: generous next to the real process, which gave " +
                "tar worth a fifth of the weight of the wood, but the chain is three stations " +
                "long and gated behind Moder, so a run is meant to be worth the walk. Drop it " +
                "to 6 or so if you want the historical trickle instead. Takes effect without " +
                "a restart, but a fermenter that is already placed and loaded keeps the yield " +
                "it was built with until its zone reloads.",
                new AcceptableValueRange<int>(1, 50));

            CoalPerBatch = Bind(file, sBurn, "Coal per batch", 10,
                "Charcoal left behind by the burn, handed over together with the tar. A real " +
                "burn left a lot of it, and 10 comfortably pays back what the furnace spent " +
                "drying the wood, so a tar run keeps itself in fuel; 0 turns the by-product " +
                "off if you would rather the chain cost fuel outright. Same timing as the " +
                "tar: live, but placed fermenters keep their old yield until the zone reloads.",
                new AcceptableValueRange<int>(0, 50));

            BurningSeconds = Bind(file, sBurn, "Burning seconds", 0,
                "Real seconds one batch of dried tyrispik smoulders in the fermenter. 0 keeps " +
                "the fermenter's own duration, which is the long vanilla mead cycle and the " +
                "slowest step of the chain. Examples: 600 is ten minutes a batch, 1800 half " +
                "an hour, 86400 a full day. Only tyrispik is affected - mead still ferments " +
                "at the vanilla speed. Takes effect immediately, including on fermenters " +
                "already burning, so lowering it can finish a batch that is under way.",
                new AcceptableValueRange<int>(0, 86400));

            CustomModels = BindLocal(file, sLook, "Custom models", true,
                "Whether the two tyrispik items use the bundle model, wood texture and icons " +
                "shipped in the assets folder beside the plugin. Off, they fall back to the " +
                "vanilla items they are cloned from - a round log and a piece of elder bark - " +
                "which is also what happens by itself if the assets folder is missing. Purely " +
                "what this machine draws: it is not synced, a server does not care, and " +
                "players on the same server can disagree about it. Read once while the game " +
                "loads, so it needs a restart.",
                null);

            ModelScale = BindLocal(file, sLook, "Model scale", 0.5f,
                "How big the shipped model is where it lies on the ground, as a multiplier on " +
                "the size the model was built at. The mesh is about a metre and a fifth long " +
                "at 1, so the default 0.5 makes a bundle roughly 0.6 m across - a generous " +
                "armful - while 0.25 brings it down to the 0.3 m the concept art calls for " +
                "and 0.1 to a handful of splinters. Only the look changes: weight, stack " +
                "size and the collider you walk into are untouched. Local, like the setting " +
                "above. Takes effect without a restart, but tyrispik already lying on the " +
                "ground keeps the size it was dropped at.",
                new AcceptableValueRange<float>(0.1f, 3f));

            CoreWoodPerBatch.SettingChanged += OnSettingChanged;
            ResinPerBatch.SettingChanged += OnSettingChanged;
            TarPerBatch.SettingChanged += OnSettingChanged;
            CoalPerBatch.SettingChanged += OnSettingChanged;
            ModelScale.SettingChanged += OnSettingChanged;
        }

        private void OnSettingChanged(object sender, EventArgs e)
        {
            ValuesChanged?.Invoke();
        }

        /// <summary>Admin only: locked on clients, the server's value wins.</summary>
        private static ConfigEntry<T> Bind<T>(ConfigFile file, string section, string key, T value,
            string description, AcceptableValueBase acceptableValues)
        {
            return file.Bind(section, key, value, new ConfigDescription(description, acceptableValues,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
        }

        /// <summary>Not synced: this machine's own, and nothing a server needs to agree on.</summary>
        private static ConfigEntry<T> BindLocal<T>(ConfigFile file, string section, string key, T value,
            string description, AcceptableValueBase acceptableValues)
        {
            return file.Bind(section, key, value, new ConfigDescription(description, acceptableValues,
                new ConfigurationManagerAttributes { IsAdminOnly = false }));
        }
    }
}
