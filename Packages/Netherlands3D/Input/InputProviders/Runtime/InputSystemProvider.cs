using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystemProvider : BaseInputProvider
{
    private InputAction dragAction;
    private InputAction moveAction;
    private InputAction lookAction;
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
        var move = moveAction.ReadValue<Vector2>();
        var look = lookAction.ReadValue<Vector2>();
        var zoom = zoomAction.ReadValue<Vector2>();

        var up = upAction.IsPressed();
        var down = downAction.IsPressed();
        if (move.magnitude>0)
        {
            horizontalInput.started.Invoke(move.x);
            verticalInput.started.Invoke(move.y);
        }
        if (zoom.magnitude > 0)
        {
            zoomInput.started.Invoke(zoom.y);
        }

        if (up)
        {
            upDownInput.started.Invoke(1);
        }
        else if(down)
        {
            upDownInput.started.Invoke(-1);
        }
    }
}
