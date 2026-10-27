using HarmonyLib;
using UnityEngine;

namespace TarDistillation
{
    /// <summary>
    ///     A tar burn does not only give tar. "Store mengder trekull blir igjen etter
    ///     brenningen og er verdsatt" - the mile leaves a lot of charcoal behind, and it was
    ///     worth money. So tapping dried tyrispik hands out coal along with the tar.
    /// </summary>
    /// <remarks>
    ///     Patches the private Fermenter.DelayedTap, which is where vanilla instantiates the
    ///     tapped items. RPC_Tap checks IsOwner before it schedules DelayedTap, so this runs
    ///     on exactly one machine and the coal cannot be duplicated in multiplayer.
    /// </remarks>
    [HarmonyPatch(typeof(Fermenter), "DelayedTap")]
    internal static class FermenterCharcoalPatch
    {
        private const string CoalPrefab = "Coal";

        private static void Postfix(Fermenter __instance, int ___m_delayedTapItem,
            bool ___m_delayedTapItemCheated)
        {
            var count = TarDistillationPlugin.Cfg?.CoalPerBatch.Value ?? 0;
            if (count <= 0 || __instance.m_outputPoint == null)
            {
                return;
            }

            var scene = ZNetScene.instance;
            if (scene == null)
            {
                return;
            }

            // What was tapped? The fermenter stores the hash of the item that went in.
            var tapped = scene.GetPrefab(___m_delayedTapItem);
            if (tapped == null || tapped.name != TarDistillationPlugin.DriedSpikPrefab)
            {
                return;
            }

            var coal = scene.GetPrefab(CoalPrefab);
            var coalDrop = coal == null ? null : coal.GetComponent<ItemDrop>();
            if (coalDrop == null)
            {
                Jotunn.Logger.LogWarning("No Coal prefab found, skipping the charcoal by-product");
                return;
            }

            // Vanilla drops the tar a little above the output point; nudge the coal sideways
            // so the two piles do not land inside each other.
            for (var i = 0; i < count; i++)
            {
                var position = __instance.m_outputPoint.position + Vector3.up * 0.3f +
                               UnityEngine.Random.insideUnitSphere * 0.15f;

                var spawned = Object.Instantiate(coalDrop, position, Quaternion.identity);
                ItemDrop.OnCreateNew(spawned, ___m_delayedTapItemCheated);
            }
        }
    }
}
