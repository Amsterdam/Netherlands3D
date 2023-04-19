using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FloatEventListener : MonoBehaviour
{
    [SerializeField]
    private FloatEvent onEvent;

    [SerializeField]
    private UnityEvent<float> trigger;

    void Awake()
    {
        if (onEvent)
        {
            onEvent.AddListenerStarted(Invoke);
        }
    }

    void Update()
    {
        if (!onEvent) Debug.Log("OnEvent null");
    }

	public void Invoke(float value)
	{
        trigger.Invoke(value);
    }
}
