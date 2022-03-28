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

    [Header("Speed")]
    [SerializeField]
    private float moveSpeed = 1.0f;
    [SerializeField]
    private float dragSpeed = 1.0f;
    [SerializeField]
    private float easing = 1.0f;
    [SerializeField]
    private float zoomSpeed = 1.0f;
    private float speed = 1.0f;
    private float speedZoom = 1.0f;

    [SerializeField]
    private float minimumSpeed = 5.0f;
    [SerializeField]
    private float maximumSpeed = 1000.0f;
    [SerializeField]
    private float rotateSpeed = 1.0f;

    [Header("Limits")]
    private float maxPointerDistance = 10000;
    [SerializeField]
    private bool useRotationLimits = true;
    [SerializeField]
    private float rotateMinLimit = -90;
    private float rotateMaxLimit = 90;
    private float pitchRotation;

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
    private bool rotating = false;
    private bool firstPersonRotate = false;


    void Awake()
    {
        cameraComponent = GetComponent<Camera>();
        worldPlane = new Plane(Vector3.up, Vector3.zero);

        pitchRotation = this.transform.localEulerAngles.x;

        horizontalInput.started.AddListener(MoveHorizontally);
        verticalInput.started.AddListener(MoveForwardBackwards);
        upDownInput.started.AddListener(MoveUpDown);
        lookInput.started.AddListener(PointerDelta);

        zoomToPointerInput.started.AddListener(ZoomToPointer);
        pointerPosition.started.AddListener(SetPointerPosition);

        dragModifier.started.AddListener(Drag);
        rotateModifier.started.AddListener(Rotate);
    }

	private void PointerDelta(Vector3 pointerDelta)
	{
        if (rotate)
        {
            if(!rotating)
            {
                dragStart = GetWorldPoint();
            }
            rotating = true;

            pitchRotation += pointerDelta.y * rotateSpeed;
            print(this.transform.localEulerAngles.x);
            //ClampXRotation(90);
            print("Clamped" + this.transform.localEulerAngles.x);

            this.transform.localEulerAngles = new Vector3(pitchRotation, this.transform.localEulerAngles.y, this.transform.localEulerAngles.z);
            this.transform.RotateAround(dragStart, Vector3.up, -pointerDelta.x * rotateSpeed);

            ClampXRotation();
        }
        else if (firstPersonRotate)
        {
            FirstPersonRotate();
        }
    }

    private void ClampXRotation(float clampAngle = 90)
    {
        pitchRotation = Mathf.Clamp(pitchRotation, rotateMinLimit, rotateMaxLimit);
    }

	private void Rotate(bool rotate)
	{
        this.rotate = rotate;
        if (!rotate) rotating = false;

    }

	void Update()
	{
        EaseDragTarget();
	}

    private void RotateAroundPoint()
    {
        
	}

    private void FirstPersonRotate()
    {

    }

    private void EaseDragTarget()
	{
        dragVelocity = new Vector3(Mathf.Lerp(dragVelocity.x,0, Time.deltaTime * easing), 0, Mathf.Lerp(dragVelocity.z, 0, Time.deltaTime * easing));
        if (!dragging & dragVelocity.magnitude > 0)
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
        else if(dragging)
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
        CalculateSpeed();
        Debug.Log(amount);
        zoomTarget = GetWorldPoint();
        var direction = zoomTarget - this.transform.position;
        this.transform.Translate(direction.normalized * speedZoom * amount, Space.World);
	}

	private void OnDrawGizmos()
	{
        if (dragging || rotating)
        {
            Gizmos.DrawSphere(dragStart, 1.0f);
        }
        else
        {
            Gizmos.DrawSphere(zoomTarget, 1.0f);
        }
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

        this.transform.Translate(Vector3.up * amount * speed * Time.deltaTime, Space.World);
    }

    private void CalculateSpeed()
    {
        speed = (multiplySpeedBasedOnHeight) ? moveSpeed * this.transform.position.y : moveSpeed;
        speedZoom = (multiplySpeedBasedOnHeight) ? zoomSpeed * this.transform.position.y : zoomSpeed;
        
        //Clamp speeds
        speed = Mathf.Clamp(speed, minimumSpeed, maximumSpeed);
        speedZoom = Mathf.Clamp(speedZoom, minimumSpeed, maximumSpeed);
    }
}
