using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StringEventListener : MonoBehaviour
{
    [SerializeField]
    private StringEvent onEvent;

    [SerializeField]
    private UnityEvent<string> trigger;

    void Awake()
    {
        if (onEvent)
        {
            onEvent.AddListenerStarted(Invoke);
        }
    }

	public void Invoke(string value)
	{
        trigger.Invoke(value);
    }
}
