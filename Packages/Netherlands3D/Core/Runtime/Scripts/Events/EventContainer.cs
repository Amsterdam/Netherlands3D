﻿/*
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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public abstract class EventContainer<T0,T1> : ScriptableObject where T0 : UnityEvent<T1>
{
    public string eventName;
    public string description;

    [HideInInspector]
    protected T0 started;
    [HideInInspector]
    protected UnityEvent received;
    [HideInInspector]
    protected UnityEvent cancelled;

    private void OnValidate()
    {
        if (eventName == "")
            eventName = this.name;
    }

    public abstract void Invoke(T1 payload);
    public void InvokeReceived()
    {
        received.Invoke();
    }
    public void InvokeCancelled()
    {
        cancelled.Invoke();
    }

    public void AddListenerStarted(UnityAction<T1> action)
    {
        started.AddListener(action);
    }

    public void RemoveListenerStarted(UnityAction<T1> action)
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