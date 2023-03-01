using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.Core
{
    public class BoolEventListener : MonoBehaviour
    {
        [SerializeField] private BoolEvent onEvent;
        [SerializeField] private UnityEvent<bool> onTriggered;
        [SerializeField] private UnityEvent<bool> onTrue;
        [SerializeField] private UnityEvent<bool> onFalse;

        void Awake()
        { 
            if(onEvent)
            {
                onEvent.AddListenerStarted(Invoke);
            }
        }

        public void Invoke(bool value)
        {
            onTriggered.Invoke(value);
            if (value==true)
            {
                onTrue.Invoke(true);
            }
            else
            {
                onFalse.Invoke(true);
            }
        }
    }
}
