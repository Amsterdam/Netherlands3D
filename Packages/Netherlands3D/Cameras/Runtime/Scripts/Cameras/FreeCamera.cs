using Netherlands3D.Events;
using System;
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
    [SerializeField]
    private bool multiplySpeedBasedOnHeight = true;

    [Header("Speeds")]
    [SerializeField]
    private float moveSpeed = 1.0f;
    [SerializeField]
    private float upAndDownSpeed = 10.0f;
    [SerializeField]
    private float dragSpeed = 1.0f;
    [SerializeField]
    private float easing = 1.0f;
    [SerializeField]
    private float zoomSpeed = 1.0f;
    private float speed = 1.0f;
    private float dynamicZoomSpeed = 1.0f;

    [SerializeField]
    private float minimumSpeed = 5.0f;
    [SerializeField]
    private float maximumSpeed = 1000.0f;
    [SerializeField]
    private float dragRotateSpeed = 1.0f;

    [SerializeField]
    private float rotateAroundPointSpeed = 1.0f;

    [Header("Gamepad")]
    [SerializeField]
    private float gamepadRotateSpeed = 1.0f;
    [SerializeField]
    private float gamepadMoveSpeed = 1.0f;

    [Header("Limits")]
    [SerializeField]
    private float maxPointerDistance = 10000;
    [SerializeField]
    private float maxCameraHeight = 1500;
    [SerializeField]
    private float minCameraHeight = -500;
    [SerializeField]
    private bool useRotationLimits = true;

    [Header("Listen to input events")]
    [SerializeField]
    private FloatEvent horizontalInput;
    [SerializeField]
    private FloatEvent verticalInput;
    [SerializeField]
    private FloatEvent upDownInput;
    [SerializeField]
    private Vector3Event lookInput;
    [SerializeField]
    private Vector3Event flyInput;
    [SerializeField]
    private Vector3Event rotateInput;

    [SerializeField]
    private FloatEvent zoomToPointerInput;
    [SerializeField]
    private Vector3Event pointerPosition;

    [SerializeField]
    private BoolEvent dragModifier;
    [SerializeField]
    private BoolEvent rotateModifier;
    [SerializeField]
    private BoolEvent firstPersonModifier;

    private Vector3 lastPointerPosition;
    private Vector3 pointerDelta;
    private Vector3 zoomTarget;
    private Camera cameraComponent;
    private Plane worldPlane;

    private Vector3 dragStart;
    private Vector3 dragVelocity;

    private bool dragging = false;
    private bool rotate = false;
    private bool rotatingAroundPoint = false;
    private bool firstPersonRotate = false;

    private Quaternion previousRotation;
    private Vector3 previousPosition;

    void Awake()
    {
        cameraComponent = GetComponent<Camera>();
        worldPlane = new Plane(Vector3.up, Vector3.zero);

        horizontalInput.started.AddListener(MoveHorizontally);
        verticalInput.started.AddListener(MoveForwardBackwards);
        upDownInput.started.AddListener(MoveUpDown);
        lookInput.started.AddListener(PointerDelta);
        flyInput.started.AddListener(FreeFly);
        rotateInput.started.AddListener(RotateAroundOwnAxis);

        zoomToPointerInput.started.AddListener(ZoomToPointer);
        pointerPosition.started.AddListener(SetPointerPosition);

        dragModifier.started.AddListener(Drag);
        rotateModifier.started.AddListener(Rotate);
        firstPersonModifier.started.AddListener(RotateFirstPerson);
    }

	private void PointerDelta(Vector3 pointerDelta)
	{
        if (rotate)
		{
			if (!rotatingAroundPoint)
			{
				dragStart = GetWorldPoint();
			}
			rotatingAroundPoint = true;
			RotateAroundPoint(pointerDelta);
		}
		else if (dragging && firstPersonRotate)
        {
            pointerDelta.x = -pointerDelta.x;
            DragRotateAroundOwnAxis(pointerDelta);
        }
    }

    /// <summary>
    /// Drag rotate via pointer delta
    /// </summary>
    /// <param name="value"></param>
    private void DragRotateAroundOwnAxis(Vector3 value)
	{
		StopEasing();
		CalculateSpeed();

		StorePreviousTransform();

		this.transform.Rotate(0, value.x * dragRotateSpeed, 0, Space.World);
		this.transform.Rotate(value.y * dragRotateSpeed, 0, 0, Space.Self);

		RevertIfOverAxis();
	}

    private void RotateAroundOwnAxis(Vector3 value)
    {
        StopEasing();
        CalculateSpeed();

        StorePreviousTransform();

        this.transform.Rotate(0, value.x * gamepadRotateSpeed * Time.deltaTime, 0, Space.World);
        this.transform.Rotate(value.y * gamepadRotateSpeed * Time.deltaTime, 0, 0, Space.Self);

        RevertIfOverAxis();
    }

    private void StorePreviousTransform()
	{
		previousRotation = this.transform.rotation;
		previousPosition = this.transform.position;
	}

    private void RotateAroundPoint(Vector3 pointerDelta)
	{
        StopEasing();

        StorePreviousTransform();

        this.transform.RotateAround(dragStart, this.transform.right, -pointerDelta.y * rotateAroundPointSpeed);
		this.transform.RotateAround(dragStart, Vector3.up, pointerDelta.x * rotateAroundPointSpeed);

        RevertIfOverAxis();
    }

    /// <summary>
    /// If we use rotation limits, restore previous position/rotation if we passed straight up or down.
    /// This avoids getting upside down.
    /// </summary>
	private void RevertIfOverAxis()
	{
        if (!useRotationLimits) return;

        var overAxis = Vector3.Dot(Vector3.up, this.transform.up);
        if (overAxis < 0)
        {
            this.transform.SetPositionAndRotation(previousPosition, previousRotation);
        }
    }

	private void FreeFly(Vector3 value)
    {
        StopEasing();
        CalculateSpeed();
        this.transform.Translate(value.x * gamepadMoveSpeed * Time.deltaTime, 0, value.y * gamepadMoveSpeed * Time.deltaTime, Space.Self);
    }

	private void Rotate(bool rotate)
	{
        this.rotate = rotate;
        if (!rotate) rotatingAroundPoint = false;
    }
    private void RotateFirstPerson(bool rotateFirstPerson)
    {
        this.firstPersonRotate = rotateFirstPerson;
    }

    void Update()
	{
        EaseDragTarget();

        Clamp();
	}

	private void Clamp()
	{
		if(this.transform.position.y > maxCameraHeight)
        {
            this.transform.position = new Vector3(this.transform.position.x, maxCameraHeight, this.transform.position.z);
        }
        else if (this.transform.position.y < minCameraHeight)
        {
            this.transform.position = new Vector3(this.transform.position.x, minCameraHeight, this.transform.position.z);
        }
    }

	private void EaseDragTarget()
	{
        dragVelocity = new Vector3(Mathf.Lerp(dragVelocity.x,0, Time.deltaTime * easing), 0, Mathf.Lerp(dragVelocity.z, 0, Time.deltaTime * easing));
        if (!dragging && dragVelocity.magnitude > 0)
        {
            this.transform.Translate(-dragVelocity * Time.deltaTime * dragSpeed,Space.World);
		}
	}

    private void StopEasing()
    {
        dragVelocity = Vector3.zero;
    }

	private void Drag(bool isDragging)
	{
        if (!dragToMoveCamera) return;

		if(!dragging && isDragging)
        {
            dragStart = GetWorldPoint();
            dragStart.y = this.transform.position.y;
        }
        else if(dragging && !rotatingAroundPoint && !firstPersonRotate)
        {
            var cameraPlanePoint = GetWorldPoint();
            cameraPlanePoint.y = this.transform.position.y;
            var dragDirection = cameraPlanePoint - dragStart;

            this.transform.Translate(-dragDirection * Time.deltaTime * dragSpeed, Space.World);

            dragVelocity = cameraPlanePoint - dragStart;
        }
        dragging = isDragging;
    }

	public void ZoomToPointer(float amount)
	{
        dragging = false;
        rotatingAroundPoint = false;

        CalculateSpeed();
        zoomTarget = GetWorldPoint();
        var direction = zoomTarget - this.transform.position;
        var targetIsBehind = Vector3.Dot(this.transform.forward, direction) < 0;
        if (targetIsBehind) direction = -direction;

        this.transform.Translate(direction.normalized * dynamicZoomSpeed * amount, Space.World);
	}

	/// <summary>
	/// Returns a position on the world 0 plane
	/// </summary>
	/// <param name="screenPoint">Optional screen position. Defaults to pointer input position.</param>
	/// <returns>World position</returns>
	private Vector3 GetWorldPoint(Vector3 screenPoint = default)
	{
		if (screenPoint == default) 
        {
            screenPoint = lastPointerPosition;
        }

		var screenRay = cameraComponent.ScreenPointToRay(screenPoint);
		worldPlane.Raycast(screenRay, out float distance);
		var samplePoint = screenRay.GetPoint(Mathf.Min(maxPointerDistance, distance));

        return samplePoint;
	}

	public void SetPointerPosition(Vector3 pointerPosition)
    {
        lastPointerPosition = pointerPosition;
    }

    public void MoveHorizontally(float amount)
	{
        StopEasing();

        CalculateSpeed();
		this.transform.Translate(Vector3.right * amount * speed * Time.deltaTime, Space.Self);
	}

	public void MoveForwardBackwards(float amount)
    {
        StopEasing();

        CalculateSpeed();
        var forwardDirection = this.transform.forward;
        if(moveForwardOnPlane)
        {
            forwardDirection.y = 0;
        }
        this.transform.Translate(forwardDirection.normalized * amount * speed * Time.deltaTime, Space.World);
    }

    public void MoveUpDown(float amount)
    {
        StopEasing();
        this.transform.Translate(Vector3.up * amount * upAndDownSpeed * Time.deltaTime, Space.World);
    }

    private void CalculateSpeed()
    {
        speed = (multiplySpeedBasedOnHeight) ? moveSpeed * Mathf.Abs(this.transform.position.y) : moveSpeed;
        dynamicZoomSpeed = (multiplySpeedBasedOnHeight) ? zoomSpeed * Mathf.Abs(this.transform.position.y) : zoomSpeed;
        
        //Clamp speeds
        speed = Mathf.Clamp(speed, minimumSpeed, maximumSpeed);
        dynamicZoomSpeed = Mathf.Clamp(dynamicZoomSpeed, minimumSpeed, maximumSpeed);
    }
    private void OnDrawGizmos()
    {
        if (dragging || rotatingAroundPoint)
        {
            Gizmos.DrawSphere(dragStart, 1.0f);
        }
        else
        {
            Gizmos.DrawSphere(zoomTarget, 1.0f);
        }
    }
}
