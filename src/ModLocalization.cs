using System.Collections.Generic;
using Jotunn.Managers;

namespace TarDistillation
{
    /// <summary>
    ///     Item names and flavour text. The wording follows the traditional Norwegian process
    ///     (see https://no.wikipedia.org/wiki/Tjærebrenning): tyri is the pitch-soaked
    ///     heartwood of old pine stumps, it is split into "spik", dried, then burned in a
    ///     covered "mile" that sweats out tar and leaves charcoal behind.
    /// </summary>
    internal static class ModLocalization
    {
        public const string SpikName = "$td_tyrispik";
        public const string SpikDescription = "$td_tyrispik_desc";
        public const string DriedName = "$td_tyrispik_dried";
        public const string DriedDescription = "$td_tyrispik_dried_desc";

        public static void Register()
        {
            var localization = LocalizationManager.Instance.GetLocalization();

            localization.AddTranslation("English", new Dictionary<string, string>
            {
                { "td_tyrispik", "Tyrispik" },
                {
                    "td_tyrispik_desc",
                    "Pitch-soaked heartwood from a pine stump that has stood rotting for ten " +
                    "winters or more, cleaned and split fine into spik. Still raw with sap and " +
                    "water - it must be dried before it will give up its tar."
                },
                { "td_tyrispik_dried", "Dried Tyrispik" },
                {
                    "td_tyrispik_dried_desc",
                    "Split tyri, dried hard. Stack it in a covered mile and choke the air, and " +
                    "the tar sweats out of the wood - leaving good charcoal behind."
                },
            });

            localization.AddTranslation("Norwegian", new Dictionary<string, string>
            {
                { "td_tyrispik", "Tyrispik" },
                {
                    "td_tyrispik_desc",
                    "Kvaerik kjerneved fra en furustubbe som har stått og rotnet i ti vintre " +
                    "eller mer, renset og finkløyvd til spik. Ennå rå av sevje og vann - den " +
                    "må tørkes før den gir fra seg tjære."
                },
                { "td_tyrispik_dried", "Tørket tyrispik" },
                {
                    "td_tyrispik_dried_desc",
                    "Finkløyvd tyri, tørket hard. Stables i en tildekket mile med lite luft, " +
                    "så siver tjæra ut av veden - og etterlater god trekull."
                },
            });
        }
    }
}
