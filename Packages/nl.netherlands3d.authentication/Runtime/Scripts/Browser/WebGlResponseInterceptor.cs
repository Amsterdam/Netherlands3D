using System;
using UnityEngine;

namespace Netherlands3D.Authentication.Browser
{
    public class WebGlResponseInterceptor : MonoBehaviour
    {
        public delegate void OnAuthResultsDelegate(string authResult);

        public OnAuthResultsDelegate OnAuthResults;

        public void GetAuthResults(string authResult)
        {
            Debug.Log(authResult);

            OnAuthResults(authResult);
        }
    }
}
