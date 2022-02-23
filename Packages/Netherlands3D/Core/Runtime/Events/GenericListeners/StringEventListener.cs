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
    private StringUnityEvent trigger;

    void Awake()
    {
        onEvent.started.AddListener(Invoke);
    }

	private void Invoke(string arg0)
	{
        trigger.Invoke(arg0);
    }
}
