using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.Events.GenericListeners
{
    public class ObjectEventListener : MonoBehaviour
    {
        [SerializeField]
        private ObjectEvent onEvent;

        [SerializeField]
        private UnityEvent<object> trigger;

        void Awake()
        {
            if (onEvent)
            {
                onEvent.AddListenerStarted(Invoke);
            }
        }

        public void Invoke(object value)
        {
            trigger.Invoke(value);
        }
    }
}