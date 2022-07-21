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
    private FloatValueUnityEvent trigger;

    void Awake()
    {
        onEvent.started.AddListener(Invoke);
    }

	public void Invoke(float value)
	{
        trigger.Invoke(value);
    }
}
