using Netherlands3D.Events;
using RuntimeHandle;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class HandleGameObjectsContainer : MonoBehaviour
{
    private RuntimeTransformHandle handle;
    private InputSystemUIInputModule inputSystemUIInputModule;

    private GameObject selectedObject;

    public HandleType handleType = HandleType.POSITION;

    [SerializeField] BoolEvent draggingHandle;

    private void Start()
    {
        if (!inputSystemUIInputModule && EventSystem.current)
            inputSystemUIInputModule = EventSystem.current.GetComponent<InputSystemUIInputModule>();

        if (!inputSystemUIInputModule)
        {
            Debug.LogError("InputSystemUIInputModule required to register clicks", this.gameObject);
            return;
        }

        //Add click listener for all objects that add gizmo on click
        inputSystemUIInputModule.leftClick.action.performed += SelectObject;
    }

    /// <summary>
    /// 
    /// </summary>
    private void SelectObject(InputAction.CallbackContext obj)
    {
        ClearHandle();

        var selectedObject = inputSystemUIInputModule.GetLastRaycastResult(0).gameObject;
        if(selectedObject != null)
        {
            this.selectedObject = selectedObject;
            HandleToTransform(selectedObject.transform);
        }
    }

    /// <summary>
    /// Clears current handle
    /// </summary>
    private void ClearHandle()
    {
        if (handle) Destroy(handle);
    }

    private void HandleToTransform(Transform objectTransform)
    {
        handle = RuntimeTransformHandle.Create(objectTransform, handleType);

        handle.startedDraggingHandle.AddListener(StartedDragging);
        handle.endedDraggingHandle.AddListener(EndedDragging);
    }

    private void StartedDragging()
    {
        if(draggingHandle)
            draggingHandle.InvokeStarted(true);
    }
    private void EndedDragging()
    {
        if (draggingHandle)
            draggingHandle.InvokeStarted(false);
    }

    private void OnDisable()
    {
        Destroy(handle);
    }
}
