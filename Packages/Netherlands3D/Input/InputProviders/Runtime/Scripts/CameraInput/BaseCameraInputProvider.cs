using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCameraInputProvider : MonoBehaviour
{
    public FloatEvent horizontalInput;
    public FloatEvent verticalInput;
    public Vector3Event lookInput;
    public Vector3Event flyInput;
    public Vector3Event rotateInput;

    public FloatEvent zoomInput;
    public FloatEvent upDownInput;
    public Vector3Event pointerPosition;

    public BoolEvent draggingModifier;
    public BoolEvent rotateModifier;
    public BoolEvent firstPersonModifier;
}