using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeCamera : MonoBehaviour
{
    [Header("Options")]
    [Tooltip("Move forward on world plane instead of camera forward")]
    [SerializeField]
    private bool moveForwardOnPlane = true;
    [SerializeField]
    private bool dragToMoveCamera = true;

    [Header("Speed")]
    [SerializeField]
    private float moveSpeed = 1.0f;
    [SerializeField]
    private float rotateSpeed = 1.0f;

    [Header("Listen to input events")]
    [SerializeField]
    private FloatEvent horizontalInput;
    [SerializeField]
    private FloatEvent verticalInput;
    [SerializeField]
    private FloatEvent upDownInput;

    [SerializeField]
    private FloatEvent zoomToPointerInput;
    [SerializeField]
    private Vector3Event pointerPosition;

    [SerializeField]
    private BoolEvent rotateModifier;

    [SerializeField]
    private BoolEvent firstPersonModifier;

    private Vector3 lastPointerPosition;

    void Awake()
    {
        horizontalInput.started.AddListener(MoveHorizontally);
        verticalInput.started.AddListener(MoveForwardBackwards);
        upDownInput.started.AddListener(MoveUpDown);

        zoomToPointerInput.started.AddListener(ZoomToPointer);
        pointerPosition.started.AddListener(SetPointerPosition);
    }

    public void ZoomToPointer(float amount)
    {
        //lastPointerPosition

    }
    public void SetPointerPosition(Vector3 pointerPosition)
    {
        lastPointerPosition = pointerPosition;
    }

    public void MoveHorizontally(float amount)
    {
        this.transform.Translate(Vector3.left * amount * moveSpeed * Time.deltaTime, Space.Self);
	}

    public void MoveForwardBackwards(float amount)
    {
        var forwardDirection = this.transform.forward;
        if(moveForwardOnPlane)
        {
            forwardDirection.y = 0;
        }
        this.transform.Translate(forwardDirection.normalized * amount * moveSpeed * Time.deltaTime, Space.Self);
    }

    public void MoveUpDown(float amount)
    {
        this.transform.Translate(Vector3.up * amount * moveSpeed * Time.deltaTime, Space.Self);
    }

    void Update()
    {
        
    }
}
