namespace TarDistillation
{
    /// <summary>
    ///     Marker class read by reflection, both by BepInEx.ConfigurationManager and by
    ///     Jotunn's SynchronizationManager. The field names are the contract, so they are
    ///     spelled exactly as those two expect; only the ones used here are declared.
    /// </summary>
    internal class ConfigurationManagerAttributes
    {
        /// <summary>Jotunn locks the entry on clients and syncs the server value instead.</summary>
        public bool? IsAdminOnly;
    }
}
