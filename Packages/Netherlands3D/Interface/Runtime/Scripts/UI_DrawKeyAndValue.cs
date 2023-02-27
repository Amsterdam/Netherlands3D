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
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Interface
{
    /// <summary>
    /// Spawns a new key/value UI item in a target container
    /// </summary>
    public class UI_DrawKeyAndValue : MonoBehaviour
    {
        [SerializeField]
        private UI_KeyValuePair keyValuePairTemplate;

        [Header("Listen to")]
        [SerializeField]
        private StringListEvent onReceivedKeyValuePair;

        [SerializeField]
        private Transform targetContainer;

        void Awake()
        {
            if (!targetContainer)
            {
                targetContainer = this.transform;
            }
            onReceivedKeyValuePair.AddListenerStarted(DrawKeyValuePair);
        }

        private void DrawKeyValuePair(List<string> keyValuePair)
        {
            var newKeyValuePair = Instantiate(keyValuePairTemplate, targetContainer);
            newKeyValuePair.SetValues(keyValuePair[0], keyValuePair[1]);
            newKeyValuePair.gameObject.SetActive(true);
        }
    }
}