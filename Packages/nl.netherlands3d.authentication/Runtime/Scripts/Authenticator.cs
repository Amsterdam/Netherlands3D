using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.Authentication
{
    public class Authenticator : MonoBehaviour
    {
        [SerializeField]
        private Session session;

        [SerializeField]
        private UnityEvent onSignedIn;

        [SerializeField]
        private UnityEvent onSignInFailed;

        [SerializeField]
        private UnityEvent onSignedOut;

        public void SignIn()
        {
            StartCoroutine(session.SignIn());
        }

        public void SignOut()
        {
            StartCoroutine(session.SignOut());
        }

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

        private void OnSignIn()
        {
            onSignedIn?.Invoke();
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
