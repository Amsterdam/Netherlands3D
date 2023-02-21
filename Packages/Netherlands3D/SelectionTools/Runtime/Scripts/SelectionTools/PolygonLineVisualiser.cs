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
using Netherlands3D.Events;
using UnityEngine;

namespace Netherlands3D.SelectionTools
{
    public class PolygonLineVisualiser : MonoBehaviour
    {
        [Header("Listen to events:")]
        [SerializeField]
        private Vector3ListsEvent drawPolygonEvent;
        [SerializeField]
        private Vector3ListEvent drawSinglePolygonEvent;
        [Header("Line Settings:")]
        [SerializeField]
        private Material lineMaterial;
        [SerializeField]
        private Color lineColour = Color.white;

        private List<GameObject> polygonVisualisationObjects = new List<GameObject>();

        private void OnEnable()
        {
            if (drawPolygonEvent) drawPolygonEvent.started.AddListener(CreatePolygons);
            if (drawSinglePolygonEvent) drawSinglePolygonEvent.started.AddListener(CreateSinglePolygon);
        }

        private void OnDisable()
        {
            if (drawPolygonEvent) drawPolygonEvent.started.RemoveAllListeners();
            if (drawSinglePolygonEvent) drawSinglePolygonEvent.started.RemoveAllListeners();
        }

        public void CreateSinglePolygon(List<Vector3> contour)
        {
            var contours = new List<IList<Vector3>> { contour };
            CreatePolygons(contours);
        }

        public void CreatePolygons(List<IList<Vector3>> contours)
        {
            polygonVisualisationObjects.AddRange(CreateAndReturnPolygons(contours));
        }

        public void DestroyVisualisationObjects()
        {
            for (int i = polygonVisualisationObjects.Count - 1; i >= 0; i--)
            {
                Destroy(polygonVisualisationObjects[i]);
            }
            polygonVisualisationObjects = new List<GameObject>();
        }

        /// <summary>
        /// creates a list of gameobjects parented to this object and adds a line renderer to each that renders the contours
        /// </summary>
        /// <param name="contour"></param>
        private List<GameObject> CreateAndReturnPolygons(List<IList<Vector3>> contours)
        {
            var list = new List<GameObject>();
            foreach (var contour in contours)
            {
                list.Add(CreateAndReturnPolygon(contour as List<Vector3>));
            }
            return list;
        }

        /// <summary>
        /// creates a gameobject parented to this object and adds a line renderer that renders the contour
        /// </summary>
        /// <param name="contour"></param>
        private GameObject CreateAndReturnPolygon(List<Vector3> contour)
        {
            var newPolygonObject = new GameObject();
            newPolygonObject.transform.SetParent(transform);
            newPolygonObject.name = "PolygonOutline";
            var lineRenderer = newPolygonObject.AddComponent<LineRenderer>();
            lineRenderer.material = lineMaterial;
            lineRenderer.startColor = lineColour;
            lineRenderer.endColor = lineColour;

            SetEndPointIfNeeded(contour);

            lineRenderer.positionCount = contour.Count;
            lineRenderer.SetPositions(contour.ToArray()); //does not work for some reason

            return newPolygonObject;
        }

        /// <summary>
        /// duplicates the first point at the end if needed to draw a closed loop.
        /// </summary>
        /// <param name="contour"></param>
        private void SetEndPointIfNeeded(List<Vector3> contour)
        {
            if (contour.Count < 2)
                return;

            if (contour[0] == contour[contour.Count - 1])
                return;

            contour.Add(contour[0]);
        }
    }
}
