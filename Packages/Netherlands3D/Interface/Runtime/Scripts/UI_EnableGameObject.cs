/*
*  Copyright (C) X Gemeente
*                X Amsterdam
*                X Economic Services Departments
*
*  Licensed under the EUPL, Version 1.2 or later (the "License");
*  You may not use this work except in compliance with the License.
*  You may obtain a copy of the License at:
*
*    https://github.com/Amsterdam/Netherlands3D/blob/main/LICENSE.txt
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" basis,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
*  implied. See the License for the specific language governing
*  permissions and limitations under the License.
*/
using Netherlands3D.Events;
using UnityEngine;

namespace Netherlands3D.Interface
{
	/// <summary>
	/// Enables this behaviours GameObject based on a bool event trigger
	/// </summary>
	public class UI_EnableGameObject : MonoBehaviour
	{
		[Header("Listen to")]
		[SerializeField]
		private BoolEvent enableGameObject;

		[SerializeField]
		private bool disableOnStart = false;

		private void Awake()
		{
			enableGameObject.AddListenerStarted(EnableGameObject);
		}

		private void EnableGameObject(bool enable)
		{
			this.gameObject.SetActive(enable);
		}

		private void Start()
		{
			if (disableOnStart)
			{
				this.gameObject.SetActive(false);
			}
		}
	}
}