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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.Events
{
	[System.Serializable]
	public class Vector2ListValueUnityEvent : UnityEvent<List<Vector2>> { }

	[CreateAssetMenu(fileName = "Vector2ListEvent", menuName = "EventContainers/Vector2ListEvent", order = 0)]
	[System.Serializable]
	public class Vector2ListEvent : EventContainer<Vector2ListValueUnityEvent, List<Vector2>>
	{
        [SerializeField]
        private bool sendAsCopy = true;

		public override void Invoke(List<Vector2> listVector2Content)
		{
            if (sendAsCopy)
            {
                var copy = new List<Vector2>(listVector2Content);
                started.Invoke(copy);
            }
            else
            {
                started.Invoke(listVector2Content);
            }
        }
	}
}