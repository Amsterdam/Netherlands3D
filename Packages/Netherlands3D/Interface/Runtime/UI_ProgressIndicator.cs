using Netherlands3D.Events;
using UnityEngine;

namespace Netherlands3D.Interface
{
    public class UI_ProgressIndicator : MonoBehaviour
    {
        [SerializeField] private RectTransform fillBar;
        [SerializeField] private bool logInConsole;

        private float currentProgress = 0.0f;

        void Awake()
        {
            fillBar.offsetMax = new Vector2(fillBar.offsetMax.x, fillBar.offsetMax.y);
        }

        private void LateUpdate()
        {
            if (currentProgress >= 1 || currentProgress == 0)
                this.gameObject.SetActive(false);
        }

        public void ShowProgress(float progress)
        {
            this.gameObject.SetActive(true);
            currentProgress = progress;

            if (logInConsole) Debug.Log($"<color=#0000FF>Progress:{progress}");

            fillBar.localScale = new Vector3(Mathf.Abs(progress), 1, 1);
        }
    }
}