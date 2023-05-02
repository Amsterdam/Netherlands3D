using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using Cdm.Authentication;
using Cdm.Authentication.Browser;
using Cdm.Authentication.Clients;
using Cdm.Authentication.OAuth2;
using Netherlands3D.Authentication.Browser;
using Netherlands3D.Authentication.Clients;
using UnityEngine;
using UnityEngine.Networking;
using IUserInfo = Cdm.Authentication.IUserInfo;

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
        }

        public void Initialize()
        {
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
                Debug.Log(response.access_token);
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
            string url = identityProvider switch
            {
                IdentityProvider.Google => ((GoogleAuth)authorizationCodeFlow).userInfoUrl,
                IdentityProvider.Facebook => ((FacebookAuth)authorizationCodeFlow).userInfoUrl,
                IdentityProvider.Github => ((GitHubAuth)authorizationCodeFlow).userInfoUrl,
                IdentityProvider.AzureAD => ((AzureADAuth)authorizationCodeFlow).userInfoUrl,
                _ => throw new ArgumentOutOfRangeException()
            };


            using var webRequest = UnityWebRequest.Get(url);
            webRequest.SetRequestHeader("Accept", "application/json");
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.timeout = 10;

            SignWebRequest(webRequest);

            yield return webRequest.SendWebRequest();

            var json = webRequest.downloadHandler.text;

            IUserInfo userInfo = identityProvider switch
            {
                IdentityProvider.Google => JsonUtility.FromJson<GoogleUserInfo>(json),
                IdentityProvider.Facebook => JsonUtility.FromJson<FacebookUserInfo>(json),
                IdentityProvider.Github => JsonUtility.FromJson<GitHubUserInfo>(json),
                IdentityProvider.AzureAD => JsonUtility.FromJson<AzureADUserInfo>(json),
                _ => throw new ArgumentOutOfRangeException()
            };

            OnUserInfoReceived?.Invoke(userInfo);

            yield return null;
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
