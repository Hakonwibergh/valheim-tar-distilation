using System;
using HarmonyLib;

namespace TarDistillation
{
    /// <summary>
    ///     Gives drying its own duration, without changing how fast the same furnace smelts ore.
    /// </summary>
    /// <remarks>
    ///     Smelter.m_secPerProduct is per station, not per conversion, so it is swapped for the
    ///     length of the one UpdateSmelter call that is working on a tyrispik and put straight
    ///     back afterwards. The field belongs to the placed instance, the call is synchronous,
    ///     and the original value is restored whether or not it was changed.
    /// </remarks>
    [HarmonyPatch(typeof(Smelter), "UpdateSmelter")]
    internal static class SmelterTimePatch
    {
        private static readonly Func<Smelter, string> GetQueuedOre =
            AccessTools.MethodDelegate<Func<Smelter, string>>(
                AccessTools.Method(typeof(Smelter), "GetQueuedOre"));

        private static void Prefix(Smelter __instance, out float __state)
        {
            __state = __instance.m_secPerProduct;

            var seconds = TarDistillationPlugin.Cfg?.DryingSeconds.Value ?? 0;
            if (seconds <= 0)
            {
                return;
            }

            if (GetQueuedOre(__instance) != TarDistillationPlugin.SpikPrefab)
            {
                return;
            }

            __instance.m_secPerProduct = seconds;
        }

        private static void Postfix(Smelter __instance, float __state)
        {
            __instance.m_secPerProduct = __state;
        }
    }
}
