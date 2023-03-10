using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.Core
{
    public class Vector3EventListener : MonoBehaviour
    {
        [SerializeField]
        private Vector3Event onEvent;

        [SerializeField]
        private UnityEvent<Vector3> trigger;

        void Awake()
        {
            if (onEvent)
            {
                onEvent.AddListenerStarted(Invoke);
            }
        }

        public void Invoke(Vector3 value)
        {
            trigger.Invoke(value);
        }
    }
}
