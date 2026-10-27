using HarmonyLib;
using UnityEngine.SceneManagement;

namespace TarDistillation
{
    /// <summary>
    ///     Safety net for dedicated servers.
    /// </summary>
    /// <remarks>
    ///     Jotunn raises OnVanillaPrefabsAvailable from a patch on ObjectDB.CopyOtherDB, and
    ///     the only caller of that is FejdStartup, the main menu. A headless server is a
    ///     different build of the game, so rather than assume it walks the same path, the
    ///     content is also built here, right before Jotunn's own ObjectDB.Awake prefix hands
    ///     its queues to the ObjectDB. Whichever path runs first wins; the other does nothing.
    /// </remarks>
    [HarmonyPatch(typeof(ObjectDB), "Awake")]
    internal static class ObjectDbFallbackPatch
    {
        [HarmonyPriority(Priority.First)]
        private static void Prefix()
        {
            // Only the world scene. In the menu scene the prefabs to clone are not all there,
            // and on a client the normal path has already run by then anyway.
            if (SceneManager.GetActiveScene().name != "main")
            {
                return;
            }

            TarDistillationPlugin.EnsureContent();
        }
    }
}
