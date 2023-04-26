using System;
using System.Collections;
using Cdm.Authentication.Clients;
using Cdm.Authentication.OAuth2;
using Netherlands3D.Authentication.Clients;
using Netherlands3D.Authentication.Connections;
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

        public UnityEvent OnSignedIn;
        public UnityEvent OnSignInFailed;
        public UnityEvent OnSignedOut;

        private IConnection connection;

        private void OnEnable()
        {
            var authorizationCodeFlow = CreateFlow();

            connection = new CdmConnection(authorizationCodeFlow);
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                connection = new WebGLConnection(authorizationCodeFlow);
            }

            connection.Initialize();

            connection.OnSignedIn += SignedIn;
            connection.OnSignInFailed += SignInFailed;
        }

        private void OnDisable()
        {
            connection.OnSignedIn -= SignedIn;
            connection.OnSignInFailed -= SignInFailed;
        }

        public IEnumerator SignIn()
        {
            yield return connection.Authenticate();
        }

        private void SignedIn(AccessTokenResponse accessTokenResponse)
        {
            this.OnSignedIn?.Invoke();
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
