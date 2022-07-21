using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Events.GenericListeners
{
    public class ColorEventListener : MonoBehaviour
    {
        [SerializeField]
        private ColorEvent onEvent;

        [SerializeField]
        private ColorValueUnityEvent trigger;

        void Awake()
        {
            onEvent.started.AddListener(Invoke);
        }

        public void Invoke(Color value)
        {
            trigger.Invoke(value);
        }
    }
}