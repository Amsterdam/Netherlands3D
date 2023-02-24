#if UNITY_EDITOR
using Netherlands3D.Events;
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class CameraInputSystemProvider : BaseCameraInputProvider
{
#if ENABLE_INPUT_SYSTEM
    [Header("Input")]
    [SerializeField] private InputActionAsset inputActionAsset;
    private InputActionMap cameraActionMap;
    private InputActionMap cameraPointerActionMap;

    private InputAction dragAction;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction flyAction;
    private InputAction rotateAction;
    private InputAction upAction;
    private InputAction downAction;
    private InputAction zoomAction;

    private InputAction rotateModifierAction;
    private InputAction firstPersonModifierAction;

    private InputAction pointerAction;

    private void Awake()
	{
        cameraActionMap = inputActionAsset.FindActionMap("Camera");
        cameraPointerActionMap = inputActionAsset.FindActionMap("CameraPointerActions");
        cameraActionMap.Enable();
        cameraPointerActionMap.Enable();

        moveAction = cameraActionMap.FindAction("Move");
        lookAction = cameraActionMap.FindAction("Look");
        flyAction = cameraActionMap.FindAction("Fly");
        rotateAction = cameraActionMap.FindAction("Rotate");
        upAction = cameraActionMap.FindAction("Up");
        downAction = cameraActionMap.FindAction("Down");
        zoomAction = cameraActionMap.FindAction("Zoom");
        dragAction = cameraPointerActionMap.FindAction("Drag");
        rotateModifierAction = cameraActionMap.FindAction("RotateModifier");
        firstPersonModifierAction = cameraActionMap.FindAction("FirstPersonModifier");
        pointerAction = cameraActionMap.FindAction("Point");
    }

    public void Update()
	{
        //Optionaly ignore camera input when the pointer is interacting with UI
        if (!isDragging && ignoreInputWhenHoveringInterface && EventSystem.current && EventSystem.current.IsPointerOverGameObject())
        {
            ingoringInput = true;
            return;
        }

        //Modifier inputs
        var dragging = !lockDraggingInput && dragAction.IsPressed();

        //Only start sending input again after we stopped dragging
        if (ingoringInput && !dragging)
            ingoringInput = false;
        if (ingoringInput) return;
        isDragging = dragging;

        var rotate = rotateModifierAction.IsPressed();
        var firstPerson = firstPersonModifierAction.IsPressed();

        draggingModifier.started.Invoke(dragging);
        rotateModifier.started.Invoke(rotate);
        firstPersonModifier.started.Invoke(firstPerson);

        //Always send position of main pointer
        var pointer = pointerAction.ReadValue<Vector2>();
        pointerPosition.started.Invoke(pointer);

        //Transform inputs 
        var moveValue = moveAction.ReadValue<Vector2>();
        var lookValue = lookAction.ReadValue<Vector2>();
        var zoomValue = zoomAction.ReadValue<Vector2>();
        var flyValue = flyAction.ReadValue<Vector2>();
        var rotateValue = rotateAction.ReadValue<Vector2>();
        var upPressed = upAction.IsPressed();
        var downPressed = downAction.IsPressed();

        lookInput.started.Invoke(lookValue);

        if (moveValue.magnitude>0)
        {
            horizontalInput.started.Invoke(moveValue.x);
            verticalInput.started.Invoke(moveValue.y);
        }
        if(flyValue.magnitude>0)
        {
            flyInput.started.Invoke(flyValue);
		}
        if (zoomValue.magnitude > 0)
        {
            zoomInput.started.Invoke(zoomValue.y);
        }
        if (rotateValue.magnitude > 0)
        {
            rotateInput.started.Invoke(rotateValue);
        }

        if (upPressed)
        {
            upDownInput.started.Invoke(1);
        }
        else if(downPressed)
        {
            upDownInput.started.Invoke(-1);
        }
    }

#endif
#if UNITY_EDITOR && !ENABLE_INPUT_SYSTEM
    private void OnValidate()
	{
        Debug.LogWarning("Input System Package (New) API is not enabled.\n" +
        "To change this go to Edit/Project Settings/Player and set the Active Input Handling dropdown to 'Input System Package (New)' or 'Both'");
    }
#endif
}
