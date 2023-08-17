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
using TMPro;
using UnityEngine;

namespace Netherlands3D.SelectionTools
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class BoundingBoxToText : MonoBehaviour
    {
        [SerializeField] private string prefix = "bbox=";
        [SerializeField] private string suffix = "";
        private TextMeshProUGUI text;
        [SerializeField] private DoubleArrayEvent boundingBoxEvent;

        void Awake()
        {
            text = GetComponent<TextMeshProUGUI>();
            boundingBoxEvent.AddListenerStarted(DrawBoundsAsText);
        }

        private void DrawBoundsAsText(double[] bbox)
        {
            text.text = $"{prefix}{bbox[0]},{bbox[1]},{bbox[2]},{bbox[3]}{suffix}";
        }
    }
}