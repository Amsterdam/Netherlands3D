using System;
using Cdm.Authentication;
using Cdm.Authentication.Browser;
using Cdm.Authentication.Clients;
using Cdm.Authentication.OAuth2;
using Netherlands3D.Authentication.Clients;
using Netherlands3D.Authentication.Browser;
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.Authentication
{
    [CreateAssetMenu(fileName = "New Session", menuName = "ScriptableObjects/Authentication/Session")]
    public class Session : ScriptableObject
    {
        [SerializeField]
        private OAuthClient oAuthClient;

        [SerializeField]
        [Tooltip("Some clients have a tenant name as part of their url, for example Azure AD")]
        private string tenant;

        [SerializeField]
        [Tooltip("The client id as defined with the OAuth app for the associated provider")]
        private string clientId;

        [SerializeField]
        [Tooltip("The client secret as defined with the OAuth app for the associated provider")]
        private string clientSecret;

        [SerializeField]
        [Tooltip("An imaginary URL to which the OAuthProvider will return the call; this is intercepted by an HTTPListener")]
        private string redirectUri = "http://localhost:8080/oauth/callback.html";

        [SerializeField]
        private string scope = "openid profile";

        public UnityEvent<IUserInfo> OnSignedIn;
        public UnityEvent OnSignInFailed;
        public UnityEvent OnSignedOut;

        private AccessTokenResponse accessTokenResponse;
        private IUserInfo userInfo;

        public bool IsAnonymous => this.accessTokenResponse == null;
        public string AccessToken => this.accessTokenResponse?.accessToken;
        public IUserInfo UserInfo => userInfo;

        public async void SignIn()
        {
            IBrowser browser = new StandaloneBrowser();
            #if UNITY_WEBGL && !UNITY_EDITOR
                browser = new WebGLBrowser();
            #endif

            using var authenticationSession = new AuthenticationSession(CreateFlow(), browser);
            try
            {
                accessTokenResponse = await authenticationSession.AuthenticateAsync();
            }
            catch
            {
                OnSignInFailed?.Invoke();
                throw;
            }

            userInfo = null;
            try
            {
                if (authenticationSession.SupportsUserInfo())
                {
                    userInfo = await authenticationSession.GetUserInfoAsync();
                }
            }
            catch
            {
                Debug.LogWarning("Retrieving the User Info for the Signed in account failed");
            }

            OnSignedIn?.Invoke(userInfo);
        }

        public void SignOut()
        {
            this.accessTokenResponse = null;
            OnSignedOut?.Invoke();
        }

        private AuthorizationCodeFlow CreateFlow()
        {
            var configuration = new AuthorizationCodeFlow.Configuration()
            {
                clientId = this.clientId,
                clientSecret = this.clientSecret,
                redirectUri = this.redirectUri,
                scope = this.scope
            };

            AuthorizationCodeFlow auth = null;
            switch (oAuthClient)
            {
                case OAuthClient.Google: auth = new GoogleAuth(configuration); break;
                case OAuthClient.Facebook: auth = new FacebookAuth(configuration); break;
                case OAuthClient.Github: auth = new GitHubAuth(configuration); break;
                case OAuthClient.AzureAD: auth = new AzureADAuth(configuration, tenant); break;
            }

            if (auth == null)
            {
                throw new ArgumentException("Unable to initiate a Session, the selected provider is not supported");
            }

            return auth;
        }
    }
}
