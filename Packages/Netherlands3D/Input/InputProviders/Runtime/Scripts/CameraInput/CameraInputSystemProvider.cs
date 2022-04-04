using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraInputSystemProvider : BaseCameraInputProvider
{
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

    private PlayerInput playerInput;

	private void Awake()
	{
        playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        flyAction = playerInput.actions["Fly"];
        rotateAction = playerInput.actions["Rotate"];
        upAction = playerInput.actions["Up"];
        downAction = playerInput.actions["Down"];
        zoomAction = playerInput.actions["Zoom"];

        dragAction = playerInput.actions["Drag"];
        rotateModifierAction = playerInput.actions["RotateModifier"];
        firstPersonModifierAction = playerInput.actions["FirstPersonModifier"];

        pointerAction = playerInput.actions["Point"];
    }

	public void Update()
	{
        //Modifier inputs
        var dragging = dragAction.IsPressed();
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
}
