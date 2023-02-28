#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraOldInputProvider : BaseCameraInputProvider
{
    private Vector2 previousPointerPosition;
#if ENABLE_LEGACY_INPUT_MANAGER
    private void Start()
	{
        previousPointerPosition = Input.mousePosition;
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
        var dragging = Input.GetMouseButton(0);

        //Only start sending input again after we stopped dragging
        if (ingoringInput && !dragging)
            ingoringInput = false;
        if (ingoringInput) return;
        isDragging = dragging;

        var rotate = Input.GetMouseButton(2) || (dragging && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)));
        var firstPerson = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        draggingModifier.Invoke(dragging);
        rotateModifier.Invoke(rotate);
        firstPersonModifier.Invoke(firstPerson);

        //Always send position of main pointer, and calculate its delta
        Vector2 pointer = Input.mousePosition;
        pointerPosition.Invoke(pointer);
        var pointerDelta = (pointer - previousPointerPosition);
        previousPointerPosition = pointer;
        lookInput.Invoke(pointerDelta);

        //Transform inputs 
        var moveValue = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        var zoomValue = Input.mouseScrollDelta;
        var upPressed = Input.GetKey(KeyCode.PageUp);
        var downPressed = Input.GetKey(KeyCode.PageDown);

        if (moveValue.magnitude>0)
        {
            horizontalInput.Invoke(moveValue.x);
            verticalInput.Invoke(moveValue.y);
        }
        if (zoomValue.magnitude > 0)
        {
            zoomInput.Invoke(zoomValue.y);
        }
        if (upPressed)
        {
            upDownInput.Invoke(1);
        }
        else if(downPressed)
        {
            upDownInput.Invoke(-1);
        }
    }
#endif
#if UNITY_EDITOR && !ENABLE_LEGACY_INPUT_MANAGER
    private void OnValidate()
	{
        Debug.LogWarning("Input Manager (Old) API is not enabled.\n" +
        "To change this go to Edit/Project Settings/Player and set the Active Input Handling dropdown to 'Input Manager (Old)' or 'Both'");
    }
#endif
}
