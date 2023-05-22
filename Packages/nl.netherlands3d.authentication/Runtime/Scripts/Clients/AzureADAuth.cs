using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Cdm.Authentication;
using Cdm.Authentication.OAuth2;
using Cdm.Authentication.Utils;
using Newtonsoft.Json;

namespace Netherlands3D.Authentication.Clients
{
    public class AzureADAuth : AuthorizationCodeFlow, IUserInfoProvider, IUserInfoProviderExtra
    {
        private readonly string tenant;

        public AzureADAuth(Configuration configuration, string tenant) : base(configuration)
        {
            this.tenant = tenant;
        }

        public override string authorizationUrl => $"https://login.microsoftonline.com/{tenant}/oauth2/v2.0/authorize";
        public override string accessTokenUrl => $"https://login.microsoftonline.com/{tenant}/oauth2/v2.0/token";
        public string userInfoUrl => "https://graph.microsoft.com/v1.0/me";

        public async Task<IUserInfo> GetUserInfoAsync(CancellationToken cancellationToken = default)
        {
            if (accessTokenResponse == null)
                throw new AccessTokenRequestException(new AccessTokenRequestError()
                {
                    code = AccessTokenRequestErrorCode.InvalidGrant,
                    description = "Authentication required."
                }, null);

            var authenticationHeader = accessTokenResponse.GetAuthenticationHeader();
            return await UserInfoParser.GetUserInfoAsync<AzureADUserInfo>(
                httpClient, userInfoUrl, authenticationHeader, cancellationToken);
        }

        public IUserInfo DeserializeUserInfo(string json)
        {
            return JsonConvert.DeserializeObject<AzureADUserInfo>(json);
        }
    }

    [DataContract]
    public class AzureADUserInfo : IUserInfo
    {
        [DataMember(Name = "id", IsRequired = true)]
        public string id { get; set; }

        [DataMember(Name = "displayName")]
        public string name { get; set; }

        [DataMember(Name = "mail")]
        public string email { get; set; }

        public string picture => null;
    }
}
