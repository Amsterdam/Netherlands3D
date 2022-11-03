using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DateTimeEventListener : MonoBehaviour
{
    [SerializeField]
    private DateTimeEvent onEvent;

    [SerializeField]
    private DateTimeUnityEvent trigger;

    void Awake()
    {
        if (onEvent)
        {
            onEvent.started.AddListener(Invoke);
        }
    }

    public void Invoke(DateTime value)
	{
        trigger.Invoke(value);
    }
}
