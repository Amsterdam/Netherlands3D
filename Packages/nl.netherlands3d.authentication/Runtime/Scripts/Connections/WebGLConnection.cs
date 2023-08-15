using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using Cdm.Authentication;
using Cdm.Authentication.Browser;
using Cdm.Authentication.OAuth2;
using Netherlands3D.Authentication.Browser;
using Netherlands3D.Authentication.Clients;
using UnityEngine;
using UnityEngine.Networking;

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
        public event IConnection.OnSignedOutDelegate OnSignedOut;
        public event IConnection.OnSignInFailedDelegate OnSignInFailed;
        public event IConnection.OnUserInfoReceivedDelegate OnUserInfoReceived;

        private readonly IdentityProvider identityProvider;
        private readonly AuthorizationCodeFlow authorizationCodeFlow;
        private AccessTokenResponse accessTokenResponse;

        public WebGLConnection(IdentityProvider identityProvider, AuthorizationCodeFlow authorizationCodeFlow)
        {
            this.identityProvider = identityProvider;
            this.authorizationCodeFlow = authorizationCodeFlow;

            oAuthInit();
        }

        public IEnumerator Authenticate()
        {
            accessTokenResponse = null;
            using var authenticationSession = new AuthenticationSession(authorizationCodeFlow, new StandaloneBrowser());

            GameObject oauthCallbackGameObject = new GameObject("WebGlResponseInterceptor");
            WebGlResponseInterceptor interceptor = oauthCallbackGameObject.AddComponent<WebGlResponseInterceptor>();

            interceptor.OnSignedIn += (token) =>
            {
                WebGLAccessTokenResponse response = JsonUtility.FromJson<WebGLAccessTokenResponse>(token);
                accessTokenResponse = new AccessTokenResponse()
                {
                    accessToken = response.access_token,
                    expiresIn = response.expires_in,
                    tokenType = response.token_type,
                    scope = String.Join(' ', response.scopes),
                };
                OnSignedIn?.Invoke(accessTokenResponse);
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
            accessTokenResponse = null;
            oAuthSignOut();

            OnSignedOut?.Invoke();

            yield return null;
        }

        public IEnumerator FetchUserInfo()
        {
            using var webRequest = UnityWebRequest.Get(GetUserInfoEndpoint());
            webRequest.SetRequestHeader("Accept", "application/json");
            webRequest.downloadHandler = new DownloadHandlerBuffer();

            SignWebRequest(webRequest);

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Unable to retrieve user information: " + webRequest.error);
                yield break;
            }

            OnUserInfoReceived?.Invoke(DeserializeUserInfo(webRequest.downloadHandler.text));

            yield return null;
        }

        private string GetUserInfoEndpoint()
        {
            return authorizationCodeFlow is IUserInfoProviderExtra userInfoProvider
                ? userInfoProvider.userInfoUrl
                : null;
        }

        private IUserInfo DeserializeUserInfo(string json)
        {
            return authorizationCodeFlow is IUserInfoProviderExtra userInfoProvider
                ? userInfoProvider.DeserializeUserInfo(json)
                : null;
        }

        public void SignWebRequest(UnityWebRequest webRequest)
        {
            if (accessTokenResponse == null)
            {
                return;
            }

            var token = accessTokenResponse.accessToken;
            webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
        }

        public IEnumerator Refresh()
        {
            // The JSO javascript library self-refreshes if you call authenticate again; so we do.
            yield return Authenticate();
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
