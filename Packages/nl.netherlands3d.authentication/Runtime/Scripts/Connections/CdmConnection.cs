using System.Collections;
using System.Text;
using System.Threading.Tasks;
using Cdm.Authentication.Browser;
using Cdm.Authentication.OAuth2;
using UnityEngine;
using UnityEngine.Networking;

namespace Netherlands3D.Authentication.Connections
{
    public class CdmConnection : IConnection
    {
        public event IConnection.OnSignedInDelegate OnSignedIn;
        public event IConnection.OnSignedOutDelegate OnSignedOut;
        public event IConnection.OnSignInFailedDelegate OnSignInFailed;
        public event IConnection.OnUserInfoReceivedDelegate OnUserInfoReceived;

        private AccessTokenResponse accessTokenResponse;
        private readonly AuthenticationSession authenticationSession;

        public CdmConnection(AuthorizationCodeFlow authorizationCodeFlow)
        {
            authenticationSession = new AuthenticationSession(authorizationCodeFlow, new StandaloneBrowser());
        }

        ~CdmConnection()
        {
            authenticationSession.Dispose();
        }

        public IEnumerator Authenticate()
        {
            accessTokenResponse = null;

            var task = authenticationSession.AuthenticateAsync();
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.Status != TaskStatus.RanToCompletion) {
                OnSignInFailed?.Invoke();
                yield break;
            }

            accessTokenResponse = task.Result;
            OnSignedIn?.Invoke(accessTokenResponse);
        }

        public IEnumerator SignOut()
        {
            accessTokenResponse = null;
            OnSignedOut?.Invoke();
            yield return null;
        }

        public IEnumerator FetchUserInfo()
        {
            if (authenticationSession.SupportsUserInfo())
            {
                yield return null;
            }

            var task = authenticationSession.GetUserInfoAsync();
            yield return new WaitUntil(() => task.IsCompleted);

            OnUserInfoReceived?.Invoke(task.Result);
        }

        public void SignWebRequest(UnityWebRequest webRequest)
        {
            if (accessTokenResponse == null)
            {
                return;
            }

            var token = Encoding.UTF8.GetBytes(accessTokenResponse.accessToken);
            webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
        }

        public IEnumerator Refresh()
        {
            // Refreshing is part of the CDM package and does not need to be done manually
            yield break;
        }
    }
}
