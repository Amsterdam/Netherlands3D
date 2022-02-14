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
        [SerializeField]
        private BoolEvent onEvent;

        [SerializeField]
        private BoolValueUnityEvent onTriggered;

        [SerializeField]
        private BoolValueUnityEvent onTrue;

        [SerializeField]
        private BoolValueUnityEvent onFalse;
        // Start is called before the first frame update
        void Start()
        {
            onEvent.started.AddListener(Invoke);
        }

        private void Invoke(bool arg0)
        {
            onTriggered.Invoke(arg0);
            if (arg0==true)
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
