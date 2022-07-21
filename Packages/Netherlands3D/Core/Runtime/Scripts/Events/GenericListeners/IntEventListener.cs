using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class IntEventListener : MonoBehaviour
{
    [SerializeField]
    private IntEvent onEvent;

    [SerializeField]
    private IntValueUnityEvent trigger;

    void Awake()
    {
        if (onEvent)
        {
            onEvent.started.AddListener(Invoke);
        }
    }

	public void Invoke(int value)
	{
        trigger.Invoke(value);
    }
}
