using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleLayerMask : MonoBehaviour
{
    [SerializeField] private bool invertBoolean = false;

    public LayerMask layerMaskOnTrue;
    public LayerMask layerMaskOnFalse;

    [Header("Invoke")]
    public LayerMaskEvent layerMaskEvent;
    public void InvokeLayerMask(LayerMask layerMask)
    {
        layerMaskEvent.InvokeStarted(layerMask);
    }

    public void InvokeMask(bool toggle)
    {
        if (invertBoolean) toggle = !toggle;

        if(toggle)
        {
            InvokeLayerMask(layerMaskOnTrue);
        }
        else
        {
            InvokeLayerMask(layerMaskOnFalse);
        }
    }
}
