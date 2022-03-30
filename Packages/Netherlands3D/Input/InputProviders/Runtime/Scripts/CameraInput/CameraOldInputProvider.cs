using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraOldInputProvider : BaseCameraInputProvider
{
    private Vector2 previousPointerPosition;

	private void Start()
	{
        previousPointerPosition = Input.mousePosition;
    }

	public void Update()
	{
        //Modifier inputs
        var dragging = Input.GetMouseButton(0);
        var rotate = Input.GetMouseButton(2) || (dragging && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)));
        var firstPerson = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        draggingModifier.started.Invoke(dragging);
        rotateModifier.started.Invoke(rotate);
        firstPersonModifier.started.Invoke(firstPerson);

        //Always send position of main pointer, and calculate its delta
        Vector2 pointer = Input.mousePosition;
        pointerPosition.started.Invoke(pointer);
        var pointerDelta = (pointer - previousPointerPosition);
        previousPointerPosition = pointer;

        //Transform inputs 
        var moveValue = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        var zoomValue = Input.mouseScrollDelta;
        var upPressed = Input.GetKey(KeyCode.PageUp);
        var downPressed = Input.GetKey(KeyCode.PageDown);

        if (moveValue.magnitude>0)
        {
            horizontalInput.started.Invoke(moveValue.x);
            verticalInput.started.Invoke(moveValue.y);
        }
        if (zoomValue.magnitude > 0)
        {
            zoomInput.started.Invoke(zoomValue.y);
        }
        if (pointerDelta.magnitude > 0)
        {
            lookInput.started.Invoke(pointerDelta);
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
}
