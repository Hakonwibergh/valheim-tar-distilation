using System;
using HarmonyLib;

namespace TarDistillation
{
    /// <summary>
    ///     Gives the tar burn its own duration, without touching how long mead takes.
    /// </summary>
    /// <remarks>
    ///     Vanilla stores one duration per fermenter prefab, not per conversion, so the
    ///     duration itself cannot be made item specific. The comparison in GetStatus is
    ///     "elapsed > m_fermentationDuration", so scaling the elapsed time moves the finish
    ///     line instead: at a factor of duration/configured, elapsed crosses the station's
    ///     duration exactly when the configured number of seconds has passed. Hover text
    ///     reads from the same method, so the progress it shows stays honest.
    /// </remarks>
    [HarmonyPatch(typeof(Fermenter), "GetFermentationTime")]
    internal static class FermenterTimePatch
    {
        private static readonly Func<Fermenter, int> GetContent =
            AccessTools.MethodDelegate<Func<Fermenter, int>>(
                AccessTools.Method(typeof(Fermenter), "GetContent"));

        private static void Postfix(Fermenter __instance, ref double __result)
        {
            var seconds = TarDistillationPlugin.Cfg?.BurningSeconds.Value ?? 0;
            if (seconds <= 0 || __result <= 0.0)
            {
                return;
            }

            var duration = __instance.m_fermentationDuration;
            if (duration <= 0f)
            {
                return;
            }

            var scene = ZNetScene.instance;
            if (scene == null)
            {
                return;
            }

            var content = scene.GetPrefab(GetContent(__instance));
            if (content == null || content.name != TarDistillationPlugin.DriedSpikPrefab)
            {
                return;
            }

            __result *= duration / (double)seconds;
        }
    }
}
