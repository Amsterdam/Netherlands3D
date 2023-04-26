using System;
using System.Collections;
using System.Runtime.InteropServices;
using Cdm.Authentication.Browser;
using Cdm.Authentication.OAuth2;
using Netherlands3D.Authentication.Browser;
using UnityEngine;

namespace Netherlands3D.Authentication.Connections
{
    public class WebGLConnection : IConnection
    {
        [DllImport("__Internal")]
        private static extern void oAuthInit();

        [DllImport("__Internal")]
        private static extern void oAuthSignIn(
            string authorizationEndpoint,
            string tokenEndpoint,
            string clientId,
            string clientSecret,
            string redirectUri,
            string scopes
        );

        [DllImport("__Internal")]
        private static extern void oAuthSignOut();

        public event IConnection.OnSignedInDelegate OnSignedIn;
        public event IConnection.OnSignInFailedDelegate OnSignInFailed;
        public event IConnection.OnUserInfoReceivedDelegate OnUserInfoReceived;

        private readonly AuthorizationCodeFlow authorizationCodeFlow;

        public WebGLConnection(AuthorizationCodeFlow authorizationCodeFlow)
        {
            this.authorizationCodeFlow = authorizationCodeFlow;
        }

        public void Initialize()
        {
            oAuthInit();
        }

        public IEnumerator Authenticate()
        {
            using var authenticationSession = new AuthenticationSession(authorizationCodeFlow, new StandaloneBrowser());

            GameObject oauthCallbackGameObject = new GameObject("WebGlResponseInterceptor");
            WebGlResponseInterceptor interceptor = oauthCallbackGameObject.AddComponent<WebGlResponseInterceptor>();

            interceptor.OnSignedIn += (token) =>
            {
                WebGLAccessTokenResponse response = JsonUtility.FromJson<WebGLAccessTokenResponse>(token);
                Debug.Log(response);
                OnSignedIn?.Invoke(new AccessTokenResponse()
                {
                    accessToken = response.access_token,
                    expiresIn = response.expires_in,
                    refreshToken = "",
                    tokenType = response.token_type,
                    scope = String.Join(' ', response.scopes),
                });
                GameObject.Destroy(oauthCallbackGameObject);
            };
            interceptor.OnSignInFailed += () =>
            {
                OnSignInFailed?.Invoke();
                GameObject.Destroy(oauthCallbackGameObject);
            };

            oAuthSignIn(
                authorizationCodeFlow.authorizationUrl,
                authorizationCodeFlow.accessTokenUrl,
                authorizationCodeFlow.configuration.clientId,
                authorizationCodeFlow.configuration.clientSecret,
                authorizationCodeFlow.configuration.redirectUri,
                authorizationCodeFlow.configuration.scope
            );

            yield return null;
        }

        public IEnumerator SignOut()
        {
            oAuthSignOut();

            yield return null;
        }


        public IEnumerator FetchUserInfo()
        {
            OnUserInfoReceived?.Invoke(null);

            yield return null;
        }
    }

    [Serializable]
    public class WebGLAccessTokenResponse
    {
        public string access_token;
        public long? expires;
        public long? expires_in;
        public long? received;
        public string id_token;
        public string[] scopes;
        public string token_type;
    }
}
