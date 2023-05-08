using Cdm.Authentication;
using Cdm.Authentication.Clients;
using Newtonsoft.Json;

namespace Netherlands3D.Authentication.Clients
{
    public class FacebookAuth : Cdm.Authentication.Clients.FacebookAuth, IUserInfoProviderExtra
    {
        public FacebookAuth(Configuration configuration) : base(configuration)
        {
        }

        public IUserInfo DeserializeUserInfo(string json)
        {
            return JsonConvert.DeserializeObject<FacebookUserInfo>(json);
        }
    }
}
