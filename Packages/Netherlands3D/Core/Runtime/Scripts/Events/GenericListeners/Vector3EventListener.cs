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
        private Vector3ValueUnityEvent trigger;

        void Awake()
        {
            if (onEvent)
            {
                onEvent.started.AddListener(Invoke);
            }
        }

        public void Invoke(Vector3 value)
        {
            trigger.Invoke(value);
        }
    }
}
