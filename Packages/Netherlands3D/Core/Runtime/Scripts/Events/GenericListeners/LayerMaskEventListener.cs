using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LayerMaskEventListener : MonoBehaviour
{
    [SerializeField]
    private LayerMaskEvent onEvent;

    [SerializeField]
    private UnityEvent<LayerMask> trigger;

    void Awake()
    {
        if (onEvent)
        {
            onEvent.AddListenerStarted(Invoke);
        }
    }

	public void Invoke(LayerMask value)
	{
        trigger.Invoke(value);
    }
}
