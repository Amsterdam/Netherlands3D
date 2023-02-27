using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Events.GenericListeners
{
    public class ObjectEventListener : MonoBehaviour
    {
        [SerializeField]
        private ObjectEvent onEvent;

        [SerializeField]
        private ObjectValueUnityEvent trigger;

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