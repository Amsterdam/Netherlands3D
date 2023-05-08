using Cdm.Authentication;
using Cdm.Authentication.Clients;
using Newtonsoft.Json;

namespace Netherlands3D.Authentication.Clients
{
    public class GitHubAuth : Cdm.Authentication.Clients.GitHubAuth, IUserInfoProviderExtra
    {
        public GitHubAuth(Configuration configuration) : base(configuration)
        {
        }

        public IUserInfo DeserializeUserInfo(string json)
        {
            return JsonConvert.DeserializeObject<GitHubUserInfo>(json);
        }
    }
}
