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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.Events
{
    [CreateAssetMenu(fileName = "Vector2IntEvent", menuName = "EventContainers/Vector2IntEvent", order = 0)]
    [System.Serializable]
    public class Vector2IntEvent : EventContainer<Vector2Int>
    {
        public override void InvokeStarted(Vector2Int vector2IntContent)
        {
            started.Invoke(vector2IntContent);
        }
    }
}