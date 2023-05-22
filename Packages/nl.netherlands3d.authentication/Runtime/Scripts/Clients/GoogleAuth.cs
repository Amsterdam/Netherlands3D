using Cdm.Authentication;
using Cdm.Authentication.Clients;
using Newtonsoft.Json;

namespace Netherlands3D.Authentication.Clients
{
    public class GoogleAuth : Cdm.Authentication.Clients.GoogleAuth, IUserInfoProviderExtra
    {
        public GoogleAuth(Configuration configuration) : base(configuration)
        {
        }

        public IUserInfo DeserializeUserInfo(string json)
        {
            return JsonConvert.DeserializeObject<GoogleUserInfo>(json);
        }
    }
}
