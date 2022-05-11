using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Events.GenericListeners
{
    public class GameObjectEventListener : MonoBehaviour
    {
        [SerializeField]
        private GameObjectEvent gameObjectEvent;

        [SerializeField]
        private GameObjectValueUnityEvent onEvent;

        void Awake()
        {
            gameObjectEvent.started.AddListener(ObjectReceived);
        }

        void ObjectReceived(GameObject receivedObject)
        {
            onEvent.Invoke(receivedObject);
        }
    }
}