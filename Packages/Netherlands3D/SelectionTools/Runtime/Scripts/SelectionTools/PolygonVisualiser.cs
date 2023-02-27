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
using UnityEngine.UIElements;

namespace Netherlands3D.SelectionTools
{
    /// <summary>
    /// Use countour lists as input to draw solidified 2D shapes in the 3D world
    /// </summary>
    public class PolygonVisualiser : MonoBehaviour
    {
        [Header("Listen to events:")]
        [SerializeField]
        private Vector3ListsEvent drawPolygonEvent;
        [SerializeField]
        private Vector3ListEvent drawSinglePolygonEvent;
        [SerializeField]
        private StringEvent setDrawingObjectName;
        [SerializeField]
        private FloatEvent setExtrusionHeightEvent;
        [SerializeField]
        private Vector3ListEvent polygonEdited;

        [SerializeField]
        private string newDrawingObjectName = "";

        [Header("Mesh")]
        [SerializeField]
        private Material defaultMaterial;

        [SerializeField]
        private float extrusionHeight = 100.0f;

        private int maxPolygons = 10000;
        private int polygonCount = 0;

        [SerializeField]
        private bool createInwardMesh;
        [SerializeField]
        private bool addBottom = false;
        [SerializeField]
        private bool receiveShadows = false;
        [SerializeField]
        private bool setUVCoordinates = false;
        [SerializeField]
        private Vector2 uvCoordinate = Vector2.zero;

        [SerializeField]
        private bool addColliders = false;

        [Header("Line")]
        [SerializeField]
        private Material lineMaterial;
        [SerializeField]
        private Color lineColor = Color.white;

        [Header("Invoke events")]
        [SerializeField]
        GameObjectEvent createdPolygonGameObject;
        [SerializeField]
        Vector3ListEvent polygonReselected;

        void Awake()
        {
            if (setDrawingObjectName) setDrawingObjectName.AddListenerStarted(SetName);
            if (drawPolygonEvent) drawPolygonEvent.AddListenerStarted(CreatePolygons);
            if (drawSinglePolygonEvent) drawSinglePolygonEvent.AddListenerStarted(CreateSinglePolygon);
            if (setExtrusionHeightEvent) setExtrusionHeightEvent.AddListenerStarted(SetExtrusionHeight);
        }

        public void SetExtrusionHeight(float extrusionHeight)
        {
            this.extrusionHeight = extrusionHeight;
        }

        public void SetName(string drawingObjectName)
        {
            newDrawingObjectName = drawingObjectName;
        }

        public void CreateSinglePolygon(List<Vector3> contour)
        {
            var contours = new List<IList<Vector3>> { contour };
            CreatePolygons(contours);
        }

        public void CreatePolygons(List<IList<Vector3>> contours)
        {
            CreateAndReturnPolygons(contours);
        }

        //Treat first contour as outer contour, and extra contours as holes
        public GameObject CreateAndReturnPolygons(List<IList<Vector3>> contours)
        {
            if (polygonCount >= maxPolygons) return null;
            polygonCount++;

            Mesh newPolygonMesh = PolygonVisualisation.CreatePolygonMesh(contours, extrusionHeight, addBottom, uvCoordinate);
            if (newPolygonMesh == null)
                return null;

            var newPolygonObject = new GameObject();
#if UNITY_EDITOR
            //Do not bother setting object name outside of Editor untill we need it.
            newPolygonObject.name = newDrawingObjectName;
#endif
            var meshFilter = newPolygonObject.AddComponent<MeshFilter>(); //mesh is created by the PolygonVisualisation script
            var meshRenderer = newPolygonObject.AddComponent<MeshRenderer>();
            meshRenderer.material = defaultMaterial;
            meshRenderer.receiveShadows = receiveShadows;

            if (addColliders)
                newPolygonObject.AddComponent<MeshCollider>().sharedMesh = newPolygonMesh;

            var polygonVisualisation = newPolygonObject.AddComponent<PolygonVisualisation>();
            polygonVisualisation.Initialize(contours, extrusionHeight, addBottom, createInwardMesh, polygonReselected, polygonEdited, lineMaterial, lineColor, uvCoordinate);

            newPolygonObject.transform.SetParent(this.transform);
            newPolygonObject.transform.Translate(0, extrusionHeight, 0);

            if (createdPolygonGameObject) createdPolygonGameObject.InvokeStarted(newPolygonObject);
            return newPolygonObject;
        }
    }
}