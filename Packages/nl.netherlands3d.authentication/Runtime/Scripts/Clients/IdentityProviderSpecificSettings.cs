using System;
using System.Collections.Generic;

namespace Netherlands3D.Authentication.Clients
{
    /// <remarks>
    /// This class could have extended List, but Unity will not serialize classes that extend List. As these settings
    /// should be managed through the ScriptableObject editor script, this is a static helper class instead.
    /// </remarks>
    public static class IdentityProviderSpecificSettings
    {
        public static string Fetch(List<IdentityProviderSpecificSetting> settings, string settingKey)
        {
            return settings
                .Find(setting => string.Equals(setting.Key, settingKey, StringComparison.OrdinalIgnoreCase))
                ?.Value;
        }

        public static bool Has(List<IdentityProviderSpecificSetting> settings, string settingKey)
        {
            return Fetch(settings, settingKey) != null;
        }
    }
}
