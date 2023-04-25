using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cdm.Authentication.OAuth2;
using Cdm.Authentication.Utils;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Netherlands3D.Authentication.OAuth2
{

    /// <summary>
    /// Supports 'Authorization Code' flow. Enables user sign-in and access to web APIs on behalf of the user.
    ///
    /// The OAuth 2.0 authorization code grant type, enables a client application to obtain
    /// authorized access to protected resources like web APIs. The auth code flow requires a user-agent that supports
    /// redirection from the authorization server back to your application. For example, a web browser, desktop,
    /// or mobile application operated by a user to sign in to your app and access their data.
    /// </summary>
    public abstract class AuthorizationCodeFlow : Cdm.Authentication.OAuth2.AuthorizationCodeFlow
    {
        protected AuthorizationCodeFlow(Configuration configuration) : base(configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// The state; any additional information that was provided by application and is posted back by service.
        /// </summary>
        /// <seealso cref="AuthorizationCodeRequest.state"/>
        public new string state { get; private set; }

        /// <summary>
        /// Gets the client configuration for the authentication method.
        /// </summary>
        public new Configuration configuration { get; }

        protected new AccessTokenResponse accessTokenResponse { get; private set; }

        /// <summary>
        /// Determines the need for retrieval of a new authorization code.
        /// </summary>
        /// <returns>Indicates if a new authorization code needs to be retrieved.</returns>
        public new bool ShouldRequestAuthorizationCode()
        {
            return accessTokenResponse == null || !accessTokenResponse.HasRefreshToken();
        }

        /// <summary>
        ///  Determines the need for retrieval of a new access token using the refresh token.
        /// </summary>
        /// <remarks>
        /// If <see cref="accessTokenResponse"/> does not exist, then get new authorization code first.
        /// </remarks>
        /// <returns>Indicates if a new access token needs to be retrieved.</returns>
        /// <seealso cref="ShouldRequestAuthorizationCode"/>
        public new bool ShouldRefreshToken()
        {
            return accessTokenResponse.IsNullOrExpired();
        }

        /// <summary>,
        /// Gets an authorization code request URI with the specified <see cref="configuration"/>.
        /// </summary>
        /// <returns>The authorization code request URI.</returns>
        public new string GetAuthorizationUrl()
        {
            // Generate new state.
            state = Guid.NewGuid().ToString("D");

            var parameters = JsonHelper.ToDictionary(new AuthorizationCodeRequest()
            {
                clientId = configuration.clientId,
                redirectUri = configuration.redirectUri,
                scope = configuration.scope,
                state = state
            });

            return UrlBuilder.New(authorizationUrl).SetQueryParameters(parameters).ToString();
        }

        /// <summary>
        /// Asynchronously exchanges code with a token.
        /// </summary>
        /// <param name="redirectUrl">
        /// <see cref="Cdm.Authentication.Browser.BrowserResult.redirectUrl">Redirect URL</see> which is retrieved
        /// from the browser result.
        /// </param>
        /// <param name="cancellationToken">Cancellation token to cancel operation.</param>
        /// <returns>Access token response which contains the access token.</returns>
        /// <exception cref="AuthorizationCodeRequestException"></exception>
        /// <exception cref="Exception"></exception>
        /// <exception cref="SecurityException"></exception>
        /// <exception cref="AccessTokenRequestException"></exception>
        public override async Task<AccessTokenResponse> ExchangeCodeForAccessTokenAsync(string redirectUrl,
            CancellationToken cancellationToken = default)
        {
            var authorizationResponseUri = new Uri(redirectUrl);
            var query = ParseQueryString(authorizationResponseUri.Query);

            // Is there any error?
            if (JsonHelper.TryGetFromNameValueCollection<AuthorizationCodeRequestError>(query, out var authorizationError))
                throw new AuthorizationCodeRequestException(authorizationError);

            if (!JsonHelper.TryGetFromNameValueCollection<AuthorizationCodeResponse>(query, out var authorizationResponse))
                throw new Exception("Authorization code could not get.");

            // Validate authorization response state.
            if (!string.IsNullOrEmpty(state) && state != authorizationResponse.state)
                throw new SecurityException($"Invalid state got: {authorizationResponse.state}");

            var parameters = JsonHelper.ToDictionary(new AccessTokenRequest()
            {
                code = authorizationResponse.code,
                clientId = configuration.clientId,
                clientSecret = configuration.clientSecret,
                redirectUri = configuration.redirectUri
            });

            Debug.Assert(parameters != null);

            accessTokenResponse =
                await GetAccessTokenInternalAsync(parameters, cancellationToken);
            return accessTokenResponse;
        }

        /// <summary>
        /// Simulate HttpUtility.ParseQueryString because it is in System.Web, because it is not supported by WebGL.
        /// </summary>
        /// <remarks>
        /// Because this is not the real thing, it won't be 100%, though it works in this context. What is, for example,
        /// lacking is the support for array-like constructs in HTTP query string (i.e. `key[]=value`).
        /// </remarks>
        /// <param name="queryString"></param>
        /// <returns>A name value collection with each key and value combination</returns>
        private static NameValueCollection ParseQueryString(string queryString)
        {
            var keyValuePairs = queryString
                .TrimStart('?')
                .Split('&')
                .Select(value => value.Split('='))
                .Select(pair => new KeyValuePair<string, string>(pair[0], pair[1]));

            NameValueCollection query = new NameValueCollection();
            foreach (var keyValuePair in keyValuePairs)
            {
                query.Add(keyValuePair.Key, keyValuePair.Value);
            }

            return query;
        }

        /// <summary>
        /// Gets the access token immediately from cache if <see cref="ShouldRefreshToken"/> is <c>false</c>;
        /// or refreshes and returns it using the refresh token.
        /// if available.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel operation.</param>
        /// <exception cref="AccessTokenRequestException">If access token cannot be granted.</exception>
        public new async Task<AccessTokenResponse> GetOrRefreshTokenAsync(
            CancellationToken cancellationToken = default)
        {
            if (ShouldRefreshToken())
            {
                return await RefreshTokenAsync(cancellationToken);
            }

            // Return from the cache immediately.
            return accessTokenResponse;
        }

        /// <summary>
        /// Asynchronously refreshes an access token using the refresh token from the <see cref="accessTokenResponse"/>.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel operation.</param>
        /// <returns>Token response which contains the access token and the refresh token.</returns>
        public new async Task<AccessTokenResponse> RefreshTokenAsync(CancellationToken cancellationToken = default)
        {
            if (accessTokenResponse == null)
                throw new AccessTokenRequestException(new AccessTokenRequestError()
                {
                    code = AccessTokenRequestErrorCode.InvalidGrant,
                    description = "Authentication required."
                }, null);

            return await RefreshTokenAsync(accessTokenResponse.refreshToken, cancellationToken);
        }

        /// <summary>
        /// Asynchronously refreshes an access token using a refresh token.
        /// </summary>
        /// <param name="refreshToken">Refresh token which is used to get a new access token.</param>
        /// <param name="cancellationToken">Cancellation token to cancel operation.</param>
        /// <returns>Token response which contains the access token and the input refresh token.</returns>
        public new async Task<AccessTokenResponse> RefreshTokenAsync(string refreshToken,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                var error = new AccessTokenRequestError()
                {
                    code = AccessTokenRequestErrorCode.InvalidGrant,
                    description = "Refresh token does not exist."
                };

                throw new AccessTokenRequestException(error, null);
            }

            var parameters = JsonHelper.ToDictionary(new RefreshTokenRequest()
            {
                refreshToken = refreshToken,
                scope = configuration.scope
            });

            Debug.Assert(parameters != null);

            var tokenResponse =
                await GetAccessTokenInternalAsync(parameters, cancellationToken);
            if (!tokenResponse.HasRefreshToken())
            {
                tokenResponse.refreshToken = refreshToken;
            }

            accessTokenResponse = tokenResponse;
            return accessTokenResponse;
        }

        protected new async Task<AccessTokenResponse> GetAccessTokenInternalAsync(
            Dictionary<string, string> content,
            CancellationToken cancellationToken = default
        ) {
            Debug.Assert(content != null);

            var authString = $"{configuration.clientId}:{configuration.clientSecret}";
            var base64AuthString = Convert.ToBase64String(Encoding.UTF8.GetBytes(authString));

            var form = new WWWForm();
            foreach (var key in content.Keys)
            {
                form.AddField(key, content[key]);
            }

            using var wr = UnityWebRequest.Post(accessTokenUrl, form);
            wr.SetRequestHeader("Accept", "application/json");
            wr.SetRequestHeader("Authorization", "Basic " + base64AuthString);
            wr.downloadHandler = new DownloadHandlerBuffer();
            wr.timeout = 10;
            var operation = wr.SendWebRequest();

            Debug.Log("Start monitoring the webrequest");
            Debug.Log(wr.url);
            while (operation.isDone == false)
            {
                Debug.Log("Waiting for the WebRequest to finish");
                Debug.Log(wr.error);
                Debug.Log(wr.responseCode);
                if (cancellationToken.IsCancellationRequested)
                {
                    wr.Abort();
                }
            }

            var responseText = wr.downloadHandler.text;
            var responseJson = responseText;

            var tokenResponse = JsonConvert.DeserializeObject<AccessTokenResponse>(responseJson);
            tokenResponse.issuedAt = DateTime.UtcNow;

            return tokenResponse;
        }
    }
}
