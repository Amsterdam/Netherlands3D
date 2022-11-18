using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.Core
{
    public class DoubleArrayEventListener : MonoBehaviour
    {
        [SerializeField]
        private DoubleArrayEvent onEvent;

        [SerializeField]
        private DoubleArrayEvent trigger;

        void Awake()
        {
            if (onEvent)
            {
                onEvent.started.AddListener(Invoke);
            }
        }

        public void Invoke(double[] value)
        {
            trigger.Invoke(value);
        }
    }
}
