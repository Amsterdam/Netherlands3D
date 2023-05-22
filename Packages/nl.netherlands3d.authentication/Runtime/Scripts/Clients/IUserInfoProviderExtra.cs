using Cdm.Authentication;

namespace Netherlands3D.Authentication.Clients
{
    public interface IUserInfoProviderExtra
    {
        public string userInfoUrl { get; }

        public IUserInfo DeserializeUserInfo(string json);
    }
}
