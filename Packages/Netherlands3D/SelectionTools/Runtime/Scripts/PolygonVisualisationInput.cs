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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Netherlands3D.SelectionTools
{
    [RequireComponent(typeof(PolygonVisualiser))]
    public class PolygonVisualisationInput : MonoBehaviour
    {
        private PolygonVisualiser polygonVisualiser;

        [Header("Listen to events:")]
        [SerializeField] private Vector3ListsEvent drawPolygonEvent;
        [SerializeField] private Vector3ListEvent drawSinglePolygonEvent;
        [SerializeField] private StringEvent setDrawingObjectName;
        [SerializeField] private FloatEvent setExtrusionHeightEvent;
        [SerializeField] private Vector3ListEvent polygonEdited;

        private void Awake()
        {
            polygonVisualiser = GetComponent<PolygonVisualiser>();
        }

        private void OnEnable()
        {
            if (setDrawingObjectName) setDrawingObjectName.AddListenerStarted(polygonVisualiser.SetName);
            if (drawPolygonEvent) drawPolygonEvent.AddListenerStarted(polygonVisualiser.CreatePolygons);
            if (drawSinglePolygonEvent) drawSinglePolygonEvent.AddListenerStarted(polygonVisualiser.CreateSinglePolygon);
            if (setExtrusionHeightEvent) setExtrusionHeightEvent.AddListenerStarted(polygonVisualiser.SetExtrusionHeight);
            if (polygonEdited) polygonEdited.AddListenerStarted(polygonVisualiser.UpdateSelectedPolygon);
        }

        private void OnDisable()
        {
            if (setDrawingObjectName) setDrawingObjectName.RemoveListenerStarted(polygonVisualiser.SetName);
            if (drawPolygonEvent) drawPolygonEvent.RemoveListenerStarted(polygonVisualiser.CreatePolygons);
            if (drawSinglePolygonEvent) drawSinglePolygonEvent.RemoveListenerStarted(polygonVisualiser.CreateSinglePolygon);
            if (setExtrusionHeightEvent) setExtrusionHeightEvent.RemoveListenerStarted(polygonVisualiser.SetExtrusionHeight);
            if (polygonEdited) polygonEdited.RemoveListenerStarted(polygonVisualiser.UpdateSelectedPolygon);
        }
    }
}
