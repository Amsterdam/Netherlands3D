using System;
using System.Collections;
using System.Collections.Generic;
using Cdm.Authentication;
using Netherlands3D.Authentication;
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D
{
    public class SessionListener : MonoBehaviour
    {
        [SerializeField]
        private Session session;

        [SerializeField]
        private UnityEvent<IUserInfo> onSignedIn;

        [SerializeField]
        private UnityEvent onSignInFailed;

        [SerializeField]
        private UnityEvent onSignedOut;

        private void OnEnable()
        {
            session.OnSignedIn.AddListener(OnSignIn);
            session.OnSignedOut.AddListener(OnSignOut);
            session.OnSignInFailed.AddListener(OnFailSignIn);
        }

        private void OnDisable()
        {
            session.OnSignedIn.RemoveListener(OnSignIn);
            session.OnSignedOut.RemoveListener(OnSignOut);
            session.OnSignInFailed.RemoveListener(OnFailSignIn);
        }

        private void OnSignIn(IUserInfo userInfo)
        {
            onSignedIn?.Invoke(userInfo);
        }

        private void OnSignOut()
        {
            onSignedOut?.Invoke();
        }

        private void OnFailSignIn()
        {
            onSignInFailed?.Invoke();
        }
    }
}
