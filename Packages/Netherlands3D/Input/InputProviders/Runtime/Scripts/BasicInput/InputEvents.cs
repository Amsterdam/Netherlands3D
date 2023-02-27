using Netherlands3D.Events;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Netherlands3D
{
    public class InputEvents : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionAsset inputActionAsset;
        private InputActionMap baseInteractionActionMap;
        private InputAction tapAction;
        private InputAction tapSecondaryAction;
        private InputAction pointerPositionAction;

        [Header("Invoke events")]
        [SerializeField] private Vector3Event clickOnScreenPosition;
        [SerializeField] private Vector3Event secondaryClickOnScreenPosition;

        private void Awake()
        {
            baseInteractionActionMap = inputActionAsset.FindActionMap("BaseSelectionInput");
            tapAction = baseInteractionActionMap.FindAction("Tap");
            tapSecondaryAction = baseInteractionActionMap.FindAction("TapSecondary");
            pointerPositionAction = baseInteractionActionMap.FindAction("PointerPosition");

            tapAction.performed += context => Tap();
            tapSecondaryAction.performed += context => TapSecondary();
        }

        private void OnEnable()
        {
            baseInteractionActionMap.Enable();
        }
        private void OnDisable()
        {
            baseInteractionActionMap.Disable();
        }

        private void Tap()
        {
            if (!IsOverInterface())
            {
                var currentPointerPosition = pointerPositionAction.ReadValue<Vector2>();
                Debug.Log($"Click at :{currentPointerPosition}");
                clickOnScreenPosition.InvokeStarted(currentPointerPosition);
            }
        }

        private void TapSecondary()
        {
            if (!IsOverInterface())
            {
                var currentPointerPosition = pointerPositionAction.ReadValue<Vector2>();
                Debug.Log($"Secondary click at :{currentPointerPosition}");
                secondaryClickOnScreenPosition.InvokeStarted(currentPointerPosition);
            }
        }

        private bool IsOverInterface()
        {
            if (!EventSystem.current) return false;
            if (EventSystem.current.IsPointerOverGameObject()) return true;
            return false;
        }
    }
}