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
    [SerializeField]
    private bool multiplySpeedBasedOnHeight = true;

    [Header("Speed")]
    [SerializeField]
    private float moveSpeed = 1.0f;
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
    private BoolEvent dragModifier;
    [SerializeField]
    private BoolEvent rotateModifier;
    [SerializeField]
    private BoolEvent firstPersonModifier;

    private Vector3 lastPointerPosition;
    private Vector3 zoomTarget;
    private Camera cameraComponent;
    private Plane worldPlane;

    void Awake()
    {
        cameraComponent = GetComponent<Camera>();
        worldPlane = new Plane(Vector3.up, Vector3.zero);

        horizontalInput.started.AddListener(MoveHorizontally);
        verticalInput.started.AddListener(MoveForwardBackwards);
        upDownInput.started.AddListener(MoveUpDown);

        zoomToPointerInput.started.AddListener(ZoomToPointer);
        pointerPosition.started.AddListener(SetPointerPosition);
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
        Gizmos.DrawSphere(zoomTarget, 1.0f);
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
		CalculateSpeed();
		this.transform.Translate(Vector3.right * amount * speed * Time.deltaTime, Space.Self);
	}

	public void MoveForwardBackwards(float amount)
    {
        CalculateSpeed();
        var forwardDirection = this.transform.forward;
        if(moveForwardOnPlane)
        {
            forwardDirection.y = 0;
        }
        this.transform.Translate(forwardDirection.normalized * amount * moveSpeed * Time.deltaTime, Space.World);
    }

    public void MoveUpDown(float amount)
    {
        this.transform.Translate(Vector3.up * amount * moveSpeed * Time.deltaTime, Space.World);
    }

    private void CalculateSpeed()
    {
        speed = (multiplySpeedBasedOnHeight) ? moveSpeed * this.transform.position.y : moveSpeed;
        speedZoom = (multiplySpeedBasedOnHeight) ? zoomSpeed * this.transform.position.y : zoomSpeed;
        
        //Clamp speeds
        speed = Mathf.Clamp(speed, minimumSpeed, maximumSpeed);
        speedZoom = Mathf.Clamp(speedZoom, minimumSpeed, maximumSpeed);
    }

    void Update()
    {
        
    }
}
