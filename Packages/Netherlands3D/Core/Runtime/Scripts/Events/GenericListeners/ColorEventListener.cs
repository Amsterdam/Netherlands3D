using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.Events.GenericListeners
{
    public class ColorEventListener : MonoBehaviour
    {
        [SerializeField]
        private ColorEvent onEvent;

        [SerializeField]
        private UnityEvent<Color> trigger;

        void Awake()
        {
            if (onEvent)
            {
                onEvent.AddListenerStarted(Invoke);
            }
        }

        public void Invoke(Color value)
        {
            trigger.Invoke(value);
        }
    }
}