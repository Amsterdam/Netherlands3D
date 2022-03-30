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
	/// Instantiates a new GameObject on a event trigger
	/// </summary>
	public class UI_SpawnGameObject : MonoBehaviour
	{
		[Header("Listen to")]
		[SerializeField]
		private TriggerEvent onSpawnGameObject;

		[SerializeField]
		private GameObject gameObjectTemplate;

		[SerializeField]
		private Transform targetContainer;

		private void Awake()
		{
			if (!targetContainer)
			{
				targetContainer = this.transform;
			}
			onSpawnGameObject.started.AddListener(Spawn);
		}

		private void Spawn()
		{
			var newGameObject = Instantiate(gameObjectTemplate, targetContainer);
			newGameObject.SetActive(true);
		}
	}
}