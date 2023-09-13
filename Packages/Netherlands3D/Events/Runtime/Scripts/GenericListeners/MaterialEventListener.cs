using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.Events.GenericListeners
{
    public class MaterialEventListener : MonoBehaviour
    {
        [SerializeField]
        private MaterialEvent onEvent;

        [SerializeField]
        private UnityEvent<Material> trigger;

        void Awake()
        {
            if (onEvent)
            {
                onEvent.AddListenerStarted(Invoke);
            }
        }

        public void Invoke(Material value)
        {
            trigger.Invoke(value);
        }
    }
}