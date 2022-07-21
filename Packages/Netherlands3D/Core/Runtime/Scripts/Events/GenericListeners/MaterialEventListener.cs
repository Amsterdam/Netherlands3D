using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Events.GenericListeners
{
    public class MaterialEventListener : MonoBehaviour
    {
        [SerializeField]
        private MaterialEvent onEvent;

        [SerializeField]
        private MaterialValueUnityEvent trigger;

        void Awake()
        {
            onEvent.started.AddListener(Invoke);
        }

        public void Invoke(Material value)
        {
            trigger.Invoke(value);
        }
    }
}