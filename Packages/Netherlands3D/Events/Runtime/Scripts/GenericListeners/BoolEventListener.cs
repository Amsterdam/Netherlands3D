/*
*  Copyright (C) X Gemeente
*                X Amsterdam
*                X Economic Services Departments
*
*  Licensed under the EUPL, Version 1.2 or later (the "License");
*  You may not use this work except in compliance with the License.
*  You may obtain a copy of the License at:
*
*    https://github.com/Amsterdam/3DAmsterdam/blob/master/LICENSE.txt
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" basis,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
*  implied. See the License for the specific language governing
*  permissions and limitations under the License.
*/
using Netherlands3D.Events;
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.Core
{
    public class BoolEventListener : MonoBehaviour
    {
        [SerializeField, Tooltip("Optional")] private BoolEvent onEvent;

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
