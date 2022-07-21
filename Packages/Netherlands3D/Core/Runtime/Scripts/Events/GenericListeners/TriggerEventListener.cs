using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;
using UnityEngine.Events;

namespace Netherlands3D.Core
{
    public class TriggerEventListener : MonoBehaviour
    {
        [SerializeField]
        private TriggerEvent onEvent;

        [SerializeField]
        private UnityEvent trigger;

        void Awake()
        {
            onEvent.started.AddListener(Invoke);
        }

        public void Invoke()
        {
            trigger.Invoke();
        }
    }
}
