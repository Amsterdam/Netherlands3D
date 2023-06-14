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
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.Events
{
    [CreateAssetMenu(fileName = "TriggerEvent", menuName = "EventContainers/TriggerEvent", order = 0)]
    [System.Serializable]
    public class TriggerEvent : ScriptableObject
    {
        public string eventName;
        public string description;

        protected UnityEvent started = new UnityEvent();
        protected UnityEvent received = new UnityEvent();
        protected UnityEvent cancelled =  new UnityEvent();

        private void OnValidate()
        {
            if (eventName == "")
                eventName = this.name;
        }

        [Obsolete("Invoke is deprecated, please use InvokeStarted instead.")]
        public void Invoke()
        {
            InvokeStarted();
        }

        public void InvokeStarted()
        {
            started.Invoke();
        }

        public void InvokeReceived()
        {
            received.Invoke();
        }
        public void InvokeCancelled()
        {
            cancelled.Invoke();
        }

        public void AddListenerStarted(UnityAction action)
        {
            started.AddListener(action);
        }

        public void RemoveListenerStarted(UnityAction action)
        {
            started.RemoveListener(action);
        }

        public void RemoveAllListenersStarted()
        {
            started.RemoveAllListeners();
        }

        public void AddListenerReceived(UnityAction action)
        {
            received.AddListener(action);
        }

        public void RemoveListenerReceived(UnityAction action)
        {
            received.RemoveListener(action);
        }

        public void RemoveAllListenersReceived()
        {
            received.RemoveAllListeners();
        }

        public void AddListenerCancelled(UnityAction action)
        {
            cancelled.AddListener(action);
        }

        public void RemoveListenerCancelled(UnityAction action)
        {
            cancelled.RemoveListener(action);
        }

        public void RemoveAllListenersCancelled()
        {
            cancelled.RemoveAllListeners();
        }
    }
}