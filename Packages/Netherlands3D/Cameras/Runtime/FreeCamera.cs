using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeCamera : MonoBehaviour
{
    [SerializeField]
    private bool moveForwardOnPlane = true;

    [SerializeField]
    private float moveSpeed = 1.0f;

    [SerializeField]
    private float rotateSpeed = 1.0f;

    void Start()
    {
        
    }

    public void MoveHorizontally(float amount)
    {
        this.transform.Translate(Vector3.left * amount * Time.deltaTime, Space.Self);
	}

    public void MoveForward(float amount)
    {
        var forwardDirection = this.transform.forward;
        if(moveForwardOnPlane)
        {
            forwardDirection.y = 0;
        }
        this.transform.Translate(forwardDirection.normalized * amount * Time.deltaTime, Space.Self);
    }

    void Update()
    {
        
    }
}
