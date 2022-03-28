using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseInputProvider : MonoBehaviour
{
    public FloatEvent horizontalInput;
    public FloatEvent verticalInput;
    public Vector3Event lookInput;

    public FloatEvent zoomInput;
    public FloatEvent upDownInput;
    public FloatEvent zoomToPointerInput;
    public Vector3Event pointerPosition;

    public BoolEvent draggingModifier;
    public BoolEvent rotateModifier;
    public BoolEvent firstPersonModifier;
}