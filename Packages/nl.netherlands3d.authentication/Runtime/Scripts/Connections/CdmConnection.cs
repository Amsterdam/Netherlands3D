using System.Collections;
using System.Threading.Tasks;
using Cdm.Authentication.Browser;
using Cdm.Authentication.OAuth2;
using UnityEngine;

namespace Netherlands3D.Authentication.Connections
{
    public class CdmConnection : IConnection
    {
        public event IConnection.OnSignedInDelegate OnSignedIn;
        public event IConnection.OnSignInFailedDelegate OnSignInFailed;
        public event IConnection.OnUserInfoReceivedDelegate OnUserInfoReceived;

        private readonly AuthorizationCodeFlow authorizationCodeFlow;

        public CdmConnection(AuthorizationCodeFlow authorizationCodeFlow)
        {
            this.authorizationCodeFlow = authorizationCodeFlow;
        }

        public void Initialize()
        {
            // Needs no initialization
        }

        public IEnumerator Authenticate()
        {
            using var authenticationSession = new AuthenticationSession(authorizationCodeFlow, new StandaloneBrowser());

            var task = authenticationSession.AuthenticateAsync();
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.Status != TaskStatus.RanToCompletion) {
                OnSignInFailed?.Invoke();
                yield break;
            }

            OnSignedIn?.Invoke(task.Result);
        }

        public IEnumerator SignOut()
        {
            yield return null;
        }

        public IEnumerator FetchUserInfo()
        {
            using var authenticationSession = new AuthenticationSession(authorizationCodeFlow, new StandaloneBrowser());

            if (authenticationSession.SupportsUserInfo())
            {
                yield return null;
            }

            var task = authenticationSession.GetUserInfoAsync();
            yield return new WaitUntil(() => task.IsCompleted);

            OnUserInfoReceived?.Invoke(task.Result);
        }
    }
}
