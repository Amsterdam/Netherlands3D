using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.Core
{
    public class Vector3ListEventListener : MonoBehaviour
    {
        [SerializeField]
        private Vector3ListEvent onEvent;

        [SerializeField]
        private UnityEvent<List<Vector3>> trigger;

        void Awake()
        {
            if (onEvent)
            {
                onEvent.AddListenerStarted(Invoke);
            }
        }

        public void Invoke(List<Vector3> value)
        {
            trigger.Invoke(value);
        }
    }
}
