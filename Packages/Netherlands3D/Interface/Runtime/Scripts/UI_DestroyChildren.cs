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
	/// Destroy all children of this transform triggered by bool event.
	/// You can use this to clear the container of dynamic spawned content.
	/// </summary>
	public class UI_DestroyChildren : MonoBehaviour
	{
		[Header("Listen to")]
		[SerializeField]
		private BoolEvent destroyChildren;

		[SerializeField]
		private bool inverseBoolean = false;

		private void Awake()
		{
			destroyChildren.AddListenerStarted(DestroyChildren);
		}

		private void DestroyChildren(bool destroy)
		{
			if (destroy || !destroy && inverseBoolean)
			{
				int childCount = transform.childCount;
				for (int i = 0; i < childCount; i++)
				{
					Destroy(transform.GetChild(i).gameObject);
				}
			}
		}
	}
}