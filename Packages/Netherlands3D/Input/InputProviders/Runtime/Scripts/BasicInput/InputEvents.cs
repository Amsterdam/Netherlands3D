using Netherlands3D.Events;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Netherlands3D
{
    public class InputEvents : MonoBehaviour
    {
        [Header("Invoke events")]
        [SerializeField]
        private Vector3Event clickOnScreenPosition;
        [SerializeField]
        private Vector3Event secondaryClickOnScreenPosition;

        private float lastClickDown = 0;
        private float clickTime = 0.5f; //seconds

#if ENABLE_LEGACY_INPUT_MANAGER
        void Update()
        {
            if (!IsOverInterface())
            {
                if(Input.GetMouseButtonDown(0))
                {
                    lastClickDown = Time.time;
                }
                if (Input.GetMouseButtonUp(0))
                {
                    var clickStart = (Time.time - lastClickDown);
                    if (clickStart < clickTime)
                        clickOnScreenPosition.Invoke(Input.mousePosition);
                }
                else if (Input.GetMouseButtonUp(1))
                {
                    secondaryClickOnScreenPosition.Invoke(Input.mousePosition);
                }
            }
        }
#endif
        private bool IsOverInterface()
        {
            if (!EventSystem.current) return false;
            if (EventSystem.current.IsPointerOverGameObject()) return true;
            return false;
        }
    }
}