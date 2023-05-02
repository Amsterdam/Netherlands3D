using UnityEngine;

namespace Netherlands3D.Authentication.Browser
{
    /// <summary>
    /// Callback object for responses from oauth.libjs.
    ///
    /// This object will receive state updates from the OAuth jslib and pass these state updates to anyone listening.
    /// The WebGLConnection object will instantiate a GameObject with this MonoBehaviour when needed as a way to receive
    /// callbacks without needing to be a MonoBehaviour itself.
    /// </summary>
    public class WebGlResponseInterceptor : MonoBehaviour
    {
        public delegate void OnSignedInDelegate(string token);
        public delegate void OnSignInFailedDelegate();

        public OnSignedInDelegate OnSignedIn;
        public OnSignInFailedDelegate OnSignInFailed;

        public void SignedIn(string token)
        {
            OnSignedIn(token);
        }

        public void SignInFailed()
        {
            OnSignInFailed();
        }
    }
}
