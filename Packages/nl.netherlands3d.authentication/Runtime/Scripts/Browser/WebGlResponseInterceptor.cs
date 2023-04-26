using System;
using UnityEngine;

namespace Netherlands3D.Authentication.Browser
{
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
