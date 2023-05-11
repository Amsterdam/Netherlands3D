using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cdm.Authentication;
using Cdm.Authentication.OAuth2;
using Netherlands3D.Authentication.Clients;
using Netherlands3D.Authentication.Connections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Netherlands3D.Authentication
{
    /// <summary>
    /// Represents a single session with which to authenticate against the given Identity Provider.
    ///
    /// This class is a ScriptableObject so that it can persist between and across scenes without the need
    /// to authenticate in between.
    /// </summary>
    [CreateAssetMenu(fileName = "New Session", menuName = "ScriptableObjects/Authentication/Session")]
    public class Session : ScriptableObject
    {
        [SerializeField]
        private IdentityProvider identityProvider;

        [SerializeField]
        [Tooltip("The client id, as defined with the OAuth app for the associated provider")]
        private string clientId;

        [SerializeField]
        [
            Tooltip(
                "The client secret, as defined with the OAuth app for the associated provider or left empty if your "
                + "provider doesn't want one"
            )
        ]
        private string clientSecret;

        /// <remarks>
        /// On platforms other than WebGL, this URL does not need to exist as an HTTPListener is started that will
        /// intercept any responses to this URL. On WebGL, this needs to be a real URL because WebGL does not support
        /// these type of tricks.
        /// </remarks>
        [SerializeField]
        [
            Tooltip(
                "The URL to the callback endpoint where to find the 'callback.html' file, as defined with the OAuth "
                + "app for the associated provider"
            )
        ]
        private string redirectUri = "http://localhost:8080/oauth/callback.html";

        [SerializeField]
        private string scope = "openid profile";

        [SerializeField]
        private List<IdentityProviderSpecificSetting> identityProviderSpecificSettings = new();

        [Header("Events")]
        public UnityEvent OnSignedIn;
        public UnityEvent OnSignInFailed;
        public UnityEvent OnSignedOut;
        public UnityEvent<IUserInfo> OnUserInfoReceived;

        public AccessTokenResponse AccessToken { get; private set; }

        private IConnection connection;

        private void OnEnable()
        {
            var authorizationCodeFlow = CreateAuthorizationFlow();

            connection = new CdmConnection(authorizationCodeFlow);
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                connection = new WebGLConnection(identityProvider, authorizationCodeFlow);
            }

            connection.OnSignedIn += SignedIn;
            connection.OnSignedOut += SignedOut;
            connection.OnSignInFailed += SignInFailed;
            connection.OnUserInfoReceived += UserInfoReceived;
        }

        private void OnDisable()
        {
            connection.OnSignedIn -= SignedIn;
            connection.OnSignedOut -= SignedOut;
            connection.OnSignInFailed -= SignInFailed;
            connection.OnUserInfoReceived -= UserInfoReceived;
        }

        private void OnValidate()
        {
            var neededSettings = Factory.GetRequiredProviderSpecificSettings(identityProvider);

            // Delete any key that is not needed for this identity provider
            var unnecessarySettings = identityProviderSpecificSettings
                .Where(setting => neededSettings.Contains(setting.Key) == false)
                .ToList();
            foreach (var setting in unnecessarySettings)
            {
                identityProviderSpecificSettings.Remove(setting);
            }

            // Add any missing keys for this identity provider
            var missingSettings = neededSettings
                .Where(setting => IdentityProviderSpecificSettings.Has(identityProviderSpecificSettings, setting) == false)
                .ToList();
            foreach (var setting in missingSettings)
            {
                identityProviderSpecificSettings.Add(new IdentityProviderSpecificSetting() {Key = setting, Value = ""});
            }
        }

        public IEnumerator SignIn()
        {
            AccessToken = null;

            yield return connection.Authenticate();
        }

        public IEnumerator RefreshBeforeExpiry()
        {
            if (AccessToken.HasRefreshToken() == false)
            {
                yield break;
            }

            if (AccessToken.expiresAt == null)
            {
                yield break;
            }

            int seconds = (int)Math.Min(0, Math.Floor((DateTime.Now - AccessToken.expiresAt).Value.TotalSeconds - 60));

            yield return new WaitForSecondsRealtime(seconds);

            yield return connection.Refresh();
        }

        public IEnumerator SignOut()
        {
            AccessToken = null;

            yield return connection.SignOut();
        }

        public IEnumerator FetchUserInfo()
        {
            yield return connection.FetchUserInfo();
        }

        /// <summary>
        /// Instead of adding the authorization header yourself, sign the unity web request by passing it through here.
        /// </summary>
        public void SignWebRequest(UnityWebRequest webRequest)
        {
            connection.SignWebRequest(webRequest);
        }

        private void SignedIn(AccessTokenResponse accessTokenResponse)
        {
            this.AccessToken = accessTokenResponse;
            this.OnSignedIn?.Invoke();
        }

        private void SignInFailed()
        {
            OnSignInFailed?.Invoke();
        }

        private void SignedOut()
        {
            OnSignedOut?.Invoke();
        }

        private void UserInfoReceived(IUserInfo userinfo)
        {
            OnUserInfoReceived?.Invoke(userinfo);
        }

        private AuthorizationCodeFlow CreateAuthorizationFlow()
        {
            return new Factory().Create(
                identityProvider,
                new AuthorizationCodeFlow.Configuration()
                {
                    clientId = this.clientId,
                    clientSecret = this.clientSecret,
                    redirectUri = this.redirectUri,
                    scope = this.scope
                },
                identityProviderSpecificSettings
            );
        }
    }
}
