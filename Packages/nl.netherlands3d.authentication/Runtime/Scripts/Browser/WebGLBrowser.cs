using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Cdm.Authentication.Browser;
using UnityEngine;

namespace Netherlands3D.Authentication.Browser
{
    public class WebGLBrowser : IBrowser
    {
        private TaskCompletionSource<BrowserResult> _taskCompletionSource;

        [DllImport("__Internal")]
        private static extern void startAuthentication(string authRequest);

        public async Task<BrowserResult> StartAsync(string loginUrl, string redirectUrl, CancellationToken cancellationToken = new CancellationToken())
        {
            _taskCompletionSource = new TaskCompletionSource<BrowserResult>();
            cancellationToken.Register(() =>
            {
                _taskCompletionSource?.TrySetCanceled();
            });

            // Instantiate an interceptor that is used as a recipient for when the redirect happens
            GameObject oauthCallbackGameObject = new GameObject("OAuthCallback");
            WebGlResponseInterceptor interceptor = oauthCallbackGameObject.AddComponent<WebGlResponseInterceptor>();
            interceptor.OnAuthResults += GetAuthResults;

            Debug.Log(loginUrl);
            Debug.Log(redirectUrl);
            startAuthentication(loginUrl);

            BrowserResult browserResult = await _taskCompletionSource.Task;

            // Clean up the interceptor; we do not need it anymore
            GameObject.Destroy(oauthCallbackGameObject);

            return browserResult;
        }

        private void GetAuthResults(string authResult)
        {
            string incomingUrl = authResult;

            _taskCompletionSource.SetResult(new BrowserResult(BrowserStatus.Success, incomingUrl));
        }
    }
}
