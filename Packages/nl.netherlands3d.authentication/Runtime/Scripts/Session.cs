using System;
using System.Collections;
using Cdm.Authentication;
using Cdm.Authentication.Clients;
using Cdm.Authentication.OAuth2;
using Netherlands3D.Authentication.Clients;
using Netherlands3D.Authentication.Connections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Netherlands3D.Authentication
{
    [CreateAssetMenu(fileName = "New Session", menuName = "ScriptableObjects/Authentication/Session")]
    public class Session : ScriptableObject
    {
        [FormerlySerializedAs("oAuthClient")] [SerializeField]
        private IdentityProvider identityProvider;

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

        public UnityEvent OnSignedIn;
        public UnityEvent OnSignInFailed;
        public UnityEvent OnSignedOut;
        public UnityEvent<IUserInfo> OnUserInfoReceived;

        private IConnection connection;

        private void OnEnable()
        {
            var authorizationCodeFlow = CreateFlow();

            connection = new CdmConnection(authorizationCodeFlow);
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                connection = new WebGLConnection(identityProvider, authorizationCodeFlow);
            }

            connection.Initialize();

            connection.OnSignedIn += SignedIn;
            connection.OnSignInFailed += SignInFailed;
            connection.OnUserInfoReceived += UserInfoReceived;
        }

        private void OnDisable()
        {
            connection.OnSignedIn -= SignedIn;
            connection.OnSignInFailed -= SignInFailed;
            connection.OnUserInfoReceived -= UserInfoReceived;
        }

        public IEnumerator SignIn()
        {
            yield return connection.Authenticate();
        }

        private void SignedIn(AccessTokenResponse accessTokenResponse)
        {
            this.OnSignedIn?.Invoke();

            connection.FetchUserInfo();
        }

        private void SignInFailed()
        {
            this.OnSignInFailed?.Invoke();
        }

        public IEnumerator SignOut()
        {
            yield return connection.SignOut();

            OnSignedOut?.Invoke();
        }

        private void UserInfoReceived(IUserInfo userinfo)
        {
            OnUserInfoReceived?.Invoke(userinfo);
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
            switch (identityProvider)
            {
                case IdentityProvider.Google: auth = new GoogleAuth(configuration); break;
                case IdentityProvider.Facebook: auth = new FacebookAuth(configuration); break;
                case IdentityProvider.Github: auth = new GitHubAuth(configuration); break;
                case IdentityProvider.AzureAD: auth = new AzureADAuth(configuration, tenant); break;
            }

            if (auth == null)
            {
                throw new ArgumentException("Unable to initiate a Session, the selected provider is not supported");
            }

            return auth;
        }
    }
}
