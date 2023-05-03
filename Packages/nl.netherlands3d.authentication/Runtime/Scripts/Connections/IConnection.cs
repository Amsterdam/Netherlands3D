using System.Collections;
using Cdm.Authentication;
using Cdm.Authentication.OAuth2;
using UnityEngine.Networking;

namespace Netherlands3D.Authentication.Connections
{
    public interface IConnection
    {
        public delegate void OnSignedInDelegate(AccessTokenResponse accessTokenResponse);
        public delegate void OnSignInFailedDelegate();
        public delegate void OnSignedOutDelegate();
        public delegate void OnUserInfoReceivedDelegate(IUserInfo userInfo);

        public event OnSignedInDelegate OnSignedIn;
        public event OnSignedOutDelegate OnSignedOut;
        public event OnSignInFailedDelegate OnSignInFailed;
        public event OnUserInfoReceivedDelegate OnUserInfoReceived;

        public IEnumerator Authenticate();

        public IEnumerator SignOut();

        public IEnumerator FetchUserInfo();

        public void SignWebRequest(UnityWebRequest webRequest);

        public IEnumerator Refresh();
    }
}
