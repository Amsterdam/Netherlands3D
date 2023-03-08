using Netherlands3D.Events;
using RuntimeHandle;
using System;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class HandleGameObjectsContainer : MonoBehaviour
{
    private RuntimeTransformHandle handle;
    private InputSystemUIInputModule inputSystemUIInputModule;

    private GameObject selectedObject;
    private HandleType handleType = HandleType.POSITION;

    [Header("Invoke events")]
    [SerializeField] BoolEvent draggingHandle;

    [Header("Listen to events")]
    [SerializeField] GameObjectEvent addGameObject;

    public HandleType HandleType { 
        get => handleType; 
        set {
            handleType = value;
            ChangedHandleType();
        } 
    }

    public GameObject SelectedObject { get => selectedObject; private set => selectedObject = value; }

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

    private void AddGameObject(GameObject newGameObject)
    {
        newGameObject.transform.parent = this.transform;
    }

    /// <summary>
    /// Swap existing handle to new type if type was changed
    /// </summary>
    private void ChangedHandleType()
    {
        if(handle)
        {
            var newHandle = RuntimeTransformHandle.Create(handle.target, HandleType);
            Destroy(handle);
            handle = newHandle;
        }
    }

    /// <summary>
    /// Check if our click selected a gameobject 
    /// </summary>
    private void SelectObject(InputAction.CallbackContext callback)
    {
        if (callback.phase == InputActionPhase.Started)
        {
            Debug.Log("Clicked, trying to select an object");
            var selectedObject = inputSystemUIInputModule.GetLastRaycastResult(0).gameObject;
            if (selectedObject != null)
            {
                Debug.Log($"Select object: {selectedObject}", selectedObject);
                var allowSelection = selectedObject.transform.parent == this.transform;
                if (allowSelection)
                {
                    this.SelectedObject = selectedObject;
                    HandleToTransform(selectedObject.transform);
                }
                else if (!selectedObject.transform.root.GetComponentInChildren<RuntimeTransformHandle>())
                {
                    HideHandle();
                }
            }
            else
            {
                Debug.Log($"Deselecting objects");
                HideHandle();
            }
        }
    }

    /// <summary>
    /// Clears current handle
    /// </summary>
    private void HideHandle()
    {
        if (handle)
            handle.gameObject.SetActive(false);

        EndedDragging();
    }

    private void HandleToTransform(Transform objectTransform)
    {
        if (!handle)
        {
            handle = RuntimeTransformHandle.Create(objectTransform, HandleType);
            handle.startedDraggingHandle.AddListener(StartedDragging);
            handle.endedDraggingHandle.AddListener(EndedDragging);
        }

        handle.target = objectTransform;
        handle.gameObject.SetActive(true);
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
    private void OnEnable()
    {
        if (addGameObject)
            addGameObject.RemoveListenerStarted(AddGameObject);
    }
    private void OnDisable()
    {
        if (addGameObject)
            addGameObject.RemoveListenerStarted(AddGameObject);

        if(handle)
            Destroy(handle);
    }
}
