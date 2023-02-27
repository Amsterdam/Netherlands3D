using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Events.EventUtilities
{
    public class BoolToggleGameObject : MonoBehaviour
    {
        [SerializeField] private BoolEvent onEvent;
        [SerializeField, Tooltip("Inverts the incoming boolean")] private bool invertBoolean = false;
        [SerializeField, Tooltip("GameObject disables after adding bool listener")] private bool disableAtLateStart = true;

        void Awake()
        {
            onEvent.AddListenerStarted(Invoke);

            if(disableAtLateStart)
                gameObject.AddComponent<DisableAtStart>();
        }

        public void Invoke(bool value)
        {
            gameObject.SetActive(invertBoolean ? !value : value);
        }
    }
}