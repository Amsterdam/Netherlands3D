#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

public class CameraInputSystemProvider : BaseCameraInputProvider
{
#if ENABLE_INPUT_SYSTEM
    [Header("Input")]
    [SerializeField] private InputActionAsset inputActionAsset;
    [Header("Interface input ignore")]
    [Tooltip("The CameraInputProvider will lock the input when a interaction starts while the pointer is over any of these layers")]
    [SerializeField] private LayerMask lockInputLayers = 32; //UI layer 5th bit is a 1
    [SerializeField] private float pinchStrength = 1000.0f;

    private InputActionMap cameraActionMap;
    private InputActionMap cameraPointerActionMap;

    private InputAction dragAction;
    private InputAction secondaryDragAction;
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

    private InputSystemUIInputModule inputSystemUIInputModule;

    private float previousPinchDistance = 0;
    private bool pinching = false;

    private bool havePreviousPinch = false;

    private Vector2 previousPrimaryPointerPosition;
    private Vector2 previousSecondaryPointerPosition;

    [Header("Multi touch to input")]
    [SerializeField] private float touchRotateMultiplier = 10.0f;
    [SerializeField] private float touchPitchMultiplier = 10.0f;

    public bool OverLockingObject
    {
        get
        {
            if (!inputSystemUIInputModule)
                return false;

            GameObject gameObjectUnderPoint = inputSystemUIInputModule.GetLastRaycastResult(0).gameObject;
            if (gameObjectUnderPoint && gameObjectUnderPoint.IsInLayerMask(lockInputLayers))
            {
                return true;
            }
            return false;
        }
    }

    private void Awake()
    {
        if (EventSystem.current)
            inputSystemUIInputModule = EventSystem.current.GetComponent<InputSystemUIInputModule>();

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
        secondaryDragAction = cameraPointerActionMap.FindAction("DragSecondary");
        rotateModifierAction = cameraActionMap.FindAction("RotateModifier");
        firstPersonModifierAction = cameraActionMap.FindAction("FirstPersonModifier");
        pointerAction = cameraActionMap.FindAction("Point");
    }

    public void Update()
    {
        //Optionaly ignore camera input when the pointer is interacting with UI
        if (!isDragging && OverLockingObject)
        {
            ingoringInput = true;
            return;
        }

        //Main inputs
        var dragging = !lockDraggingInput && dragAction.IsPressed();

        //Only start sending input again after we stopped dragging
        if (ingoringInput && !dragging)
            ingoringInput = false;

        if (ingoringInput) return;

        isDragging = dragging;

        //Modifiers
        var rotate = rotateModifierAction.IsPressed();
        var firstPerson = firstPersonModifierAction.IsPressed();

        //Always send position of main pointer
        var pointerPosition = pointerAction.ReadValue<Vector2>();

        //Transform inputs 
        var moveValue = moveAction.ReadValue<Vector2>();
        var lookValue = lookAction.ReadValue<Vector2>();
        var zoomValue = zoomAction.ReadValue<Vector2>();
        var flyValue = flyAction.ReadValue<Vector2>();
        var rotateValue = rotateAction.ReadValue<Vector2>();
        var upPressed = upAction.IsPressed();
        var downPressed = downAction.IsPressed();

        lookInput.InvokeStarted(lookValue);

        //If there is a secondary input, convert pinch into zoom
        var secondaryPointerPosition = secondaryDragAction.ReadValue<Vector2>();
        var draggingSecondary = !lockDraggingInput && secondaryPointerPosition.magnitude > 0;
#if UNITY_EDITOR
        //Inverse secondary touch input X in editor so we can test pinch distance with mouse touch debugging
        secondaryPointerPosition.x = Screen.width - secondaryPointerPosition.x;

        if(Keyboard.current.shiftKey.IsPressed())
            secondaryPointerPosition.y = Screen.height - secondaryPointerPosition.y;
#endif


        //Optional pinch input that overrides center pointer and zoom
        if (!pinching && draggingSecondary)
        {
            pinching = true;
            previousPinchDistance = Vector2.Distance(pointerPosition, secondaryPointerPosition);
        }
        else if (pinching && !draggingSecondary)
        {
            pinching = false;
            havePreviousPinch = false;
        }
        if (pinching)
        {
            var primaryPointerPosition = pointerPosition;
            
            if (havePreviousPinch)
            {
                //Override main pointer to be in the middle of the two touches
                pointerPosition = Vector2.Lerp(pointerPosition, secondaryPointerPosition, 0.5f);

                //Override default zoom input with pinch zoom
                var currentPinchDistance = Vector2.Distance(primaryPointerPosition, secondaryPointerPosition);
                var pinchDelta = currentPinchDistance - previousPinchDistance;
                zoomValue.y = (pinchDelta / Screen.height) * pinchStrength;

                previousPinchDistance = currentPinchDistance;

                //Override rotate around point on pinch rotate
                var rotationDelta = touchRotateMultiplier * TouchesToRotationDelta(previousPrimaryPointerPosition, previousSecondaryPointerPosition, primaryPointerPosition, secondaryPointerPosition) / Screen.height;   
                Debug.Log("rotationDelta " + rotationDelta);
                if(Mathf.Abs(rotationDelta) > 0)
                {
                    rotate = true;
                    lookValue.x = rotationDelta;
                }

                //Override pitch movement simultaneously moving two fingers up and down
                var upAndDownDelta = touchPitchMultiplier * TouchesUpDownDelta(previousPrimaryPointerPosition, previousSecondaryPointerPosition, primaryPointerPosition, secondaryPointerPosition) / Screen.height;
                Debug.Log("upAndDownDelta " + upAndDownDelta);
                if (Mathf.Abs(upAndDownDelta) > 0)
                {
                    rotate = true;
                    lookValue.y = upAndDownDelta;
                }
            }

            //Store positions
            previousPrimaryPointerPosition = primaryPointerPosition;
            previousSecondaryPointerPosition = secondaryPointerPosition;
            havePreviousPinch = true;
        }

        //Send modifiers
        draggingModifier.InvokeStarted(dragging);
        rotateModifier.InvokeStarted(rotate);
        firstPersonModifier.InvokeStarted(firstPerson);

        //Always send main pointer position
        base.pointerPosition.InvokeStarted(pointerPosition);

        //Invoke values as events
        InvokeEvents(dragging, pointerPosition, moveValue, zoomValue, flyValue, rotateValue, upPressed, downPressed);
    }

    private void InvokeEvents(bool dragging, Vector2 pointerPosition, Vector2 moveValue, Vector2 zoomValue, Vector2 flyValue, Vector2 rotateValue, bool upPressed, bool downPressed)
    {
        var requiresSmoothMovement = false;

        if (dragging)
        {
            requiresSmoothMovement = true;
        }

        if (moveValue.magnitude > 0)
        {
            horizontalInput.InvokeStarted(moveValue.x);
            verticalInput.InvokeStarted(moveValue.y);

            requiresSmoothMovement = true;
        }
        if (flyValue.magnitude > 0)
        {
            flyInput.InvokeStarted(flyValue);

            requiresSmoothMovement = true;
        }
        if (zoomValue.magnitude > 0)
        {
            zoomInput.InvokeStarted(zoomValue.y);
        }
        if (rotateValue.magnitude > 0)
        {
            rotateInput.InvokeStarted(rotateValue);

            requiresSmoothMovement = true;
        }
        if (upPressed)
        {
            upDownInput.InvokeStarted(1);
        }
        else if (downPressed)
        {
            upDownInput.InvokeStarted(-1);
        }


        if(pauseHeavyProcess)
            pauseHeavyProcess.InvokeStarted(requiresSmoothMovement);
    }

    private void OnDrawGizmos()
    {
        Camera cam = Camera.main;

        Vector3 touchPos1World = cam.ScreenToWorldPoint(new Vector3(previousPrimaryPointerPosition.x, previousPrimaryPointerPosition.y, cam.nearClipPlane + 1));
        Vector3 touchPos2World = cam.ScreenToWorldPoint(new Vector3(previousSecondaryPointerPosition.x, previousSecondaryPointerPosition.y, cam.nearClipPlane + 1));

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(touchPos1World, 0.01f);
        Gizmos.DrawSphere(touchPos2World, 0.01f);

    }

    /// <summary>
    /// Converts two previous and current touch positions into rotation delta
    /// </summary>
    public static float TouchesToRotationDelta(Vector2 prevTouchPos1, Vector2 prevTouchPos2, Vector2 currTouchPos1, Vector2 currTouchPos2)
    {
        // Calculate the previous and current direction vectors between the two touch points
        Vector2 prevDir = prevTouchPos2 - prevTouchPos1;
        Vector2 currDir = currTouchPos2 - currTouchPos1;

        // Calculate the previous and current angles between the two touch points
        float prevAngle = Mathf.Atan2(prevDir.y, prevDir.x) * Mathf.Rad2Deg;
        float currAngle = Mathf.Atan2(currDir.y, currDir.x) * Mathf.Rad2Deg;

        // Calculate the rotation delta between the previous and current angles
        float rotationDelta = Mathf.DeltaAngle(prevAngle, currAngle);

        return rotationDelta;
    }

    /// <summary>
    /// Combines two touches to get their up/down delta
    /// </summary>
    public static float TouchesUpDownDelta(Vector2 prevTouchPos1, Vector2 prevTouchPos2, Vector2 currTouchPos1, Vector2 currTouchPos2)
    {
        // Calculate the y-axis delta between the previous and current touch positions
        float prevY = (prevTouchPos1.y + prevTouchPos2.y) * 0.5f;
        float currY = (currTouchPos1.y + currTouchPos2.y) * 0.5f;
        float upDownDelta = currY - prevY;

        return upDownDelta;
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
