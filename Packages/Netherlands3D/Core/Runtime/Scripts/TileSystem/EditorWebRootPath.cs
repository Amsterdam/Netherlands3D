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
using Netherlands3D.TileSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Core
{
	public class EditorWebRootPath : MonoBehaviour
	{
		[SerializeField]
		private string serverRootWebAddress = "https://acc.3d.amsterdam.nl";
#if UNITY_EDITOR
		void Awake()
		{
			TurnAllRelativeIntoFullPaths();
		}

		private void TurnAllRelativeIntoFullPaths()
		{
			Layer[] layers = GetComponentsInChildren<Layer>();
			foreach (var layer in layers)
			{
				foreach (var dataset in layer.Datasets)
				{
					if (dataset.path.StartsWith("/"))
					{
						//Turn relative paths into full path
						dataset.path = serverRootWebAddress + dataset.path;
					}
				}
			}
		}
#endif
	}
}