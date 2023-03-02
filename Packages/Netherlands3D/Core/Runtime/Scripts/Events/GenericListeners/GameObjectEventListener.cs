using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.Events.GenericListeners
{
    public class GameObjectEventListener : MonoBehaviour
    {
        [SerializeField]
        private GameObjectEvent onEvent;

        [SerializeField]
        private UnityEvent<GameObject> trigger;

        void Awake()
        {
            if (onEvent)
            {
                onEvent.AddListenerStarted(Invoke);
            }
        }

        public void Invoke(GameObject value)
        {
            trigger.Invoke(value);
        }
    }
}