using System;
using System.Collections.Generic;
using System.Linq;
using Cdm.Authentication.OAuth2;

namespace Netherlands3D.Authentication.Clients
{
    public class Factory
    {
        public AuthorizationCodeFlow Create(
            IdentityProvider identityProvider,
            AuthorizationCodeFlow.Configuration configuration,
            List<IdentityProviderSpecificSetting> settings
        )
        {
            if (ValidateProviderSpecificSettings(identityProvider, settings) == false)
            {
                throw new ArgumentException("Missing required setting for the given identity provider");
            }

            return identityProvider switch
            {
                IdentityProvider.Google => new GoogleAuth(configuration),
                IdentityProvider.Facebook => new FacebookAuth(configuration),
                IdentityProvider.Github => new GitHubAuth(configuration),
                IdentityProvider.AzureAD => new AzureADAuth(
                    configuration,
                    IdentityProviderSpecificSettings.Fetch(settings, "tenant")
                ),
                _ => throw new ArgumentException("Unable to initiate a Session, the selected provider is not supported")
            };
        }

        public static List<string> GetRequiredProviderSpecificSettings(IdentityProvider identityProvider)
        {
            return identityProvider switch
            {
                IdentityProvider.AzureAD => new List<string> { "tenant" },
                _ => new List<string>()
            };
        }

        public static bool ValidateProviderSpecificSettings(
            IdentityProvider identityProvider,
            List<IdentityProviderSpecificSetting> settings
        )
        {
            return GetRequiredProviderSpecificSettings(identityProvider)
                .All(
                    setting => settings.Any(t => string.Equals(t.Key, setting, StringComparison.OrdinalIgnoreCase))
                );
        }
    }
}
