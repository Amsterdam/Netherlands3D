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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Netherlands3D.SelectionTools
{
    [RequireComponent(typeof(LineRenderer))]
    public class PolygonSelection : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionAsset inputActionAsset;
        private InputActionMap polygonSelectionActionMap;

        [Header("Invoke")]
        [SerializeField] private BoolEvent blockCameraDrag;
        [SerializeField] private Vector3ListEvent selectedPolygonArea;

        [Header("Settings")]
        [SerializeField] Color lineColor = Color.red;
        [SerializeField] Color closedLoopLineColor = Color.red;
        [SerializeField] private float lineWidthMultiplier = 10.0f;
        [SerializeField] private float maxSelectionDistanceFromCamera = 10000;
        [SerializeField] private bool useWorldSpace = false;
        [SerializeField] private float closeLoopDistance = 10.0f;
        [SerializeField] private int minPointsToCloseLoop = 1;
        [SerializeField] private float minDistanceBetweenAutoPoints = 10.0f;
        [SerializeField] private float minDirectionThresholdForAutoPoints = 0.8f;


        private InputAction pointerAction;
        private InputAction tapAction;
        private InputAction tapSecondaryAction;
        private InputAction clickAction;
        private InputAction modifierAction;

        private LineRenderer lineRenderer;
        [SerializeField] private List<Vector3> positions = new List<Vector3>();
        private Vector3 lastAddedPoint = default;
        private Vector3 selectionStartPosition = default;
        private Vector3 currentWorldCoordinate = default;
        private Vector3 previousFrameWorldCoordinate = default;
        private Vector3 lastNormal = Vector3.up;
        private Plane worldPlane;

        private bool closedLoop = false;
        private bool autoDrawPolygon = false;
        private bool requireReleaseBeforeRedraw = false;
        private Camera mainCamera;

        [SerializeField] private Transform pointerRepresentation;

        void Awake()
        {
            mainCamera = Camera.main;
            lineRenderer = this.GetComponent<LineRenderer>();
            lineRenderer.startColor = lineRenderer.endColor = lineColor;
            lineRenderer.widthMultiplier = lineWidthMultiplier;
            lineRenderer.positionCount = 1;

            polygonSelectionActionMap = inputActionAsset.FindActionMap("AreaSelection");
            tapAction = polygonSelectionActionMap.FindAction("Tap");
            tapSecondaryAction = polygonSelectionActionMap.FindAction("TapSecondary");
            clickAction = polygonSelectionActionMap.FindAction("Click");
            pointerAction = polygonSelectionActionMap.FindAction("Point");
            modifierAction = polygonSelectionActionMap.FindAction("Modifier");

            tapAction.performed += context => Tap();
            clickAction.performed += context => StartClick();
            clickAction.canceled += context => Release();

            worldPlane = (useWorldSpace) ? new Plane(Vector3.up, Vector3.zero) : new Plane(this.transform.up, this.transform.position);
        }

        private void OnEnable()
        {
            polygonSelectionActionMap.Enable();
        }

        private void OnDisable()
        {
            autoDrawPolygon = false;
            blockCameraDrag.started.Invoke(false);
            polygonSelectionActionMap.Disable();
        }

        private void Update()
        {
            var currentPointerPosition = pointerAction.ReadValue<Vector2>();
            currentWorldCoordinate = GetCoordinateInWorld(currentPointerPosition);

            if(!closedLoop) SnapLastPositionToPointer();

            pointerRepresentation.position = currentWorldCoordinate;

            if (!autoDrawPolygon && clickAction.IsPressed() && modifierAction.IsPressed())
            {
                autoDrawPolygon = true;
                blockCameraDrag.started.Invoke(true);
            }
            else if (autoDrawPolygon && !clickAction.IsPressed())
            {
                autoDrawPolygon = false;
                blockCameraDrag.started.Invoke(false);
            }

            if (!requireReleaseBeforeRedraw && autoDrawPolygon)
            {
                AutoAddPoint();
            }
            else if(requireReleaseBeforeRedraw && !clickAction.IsPressed())
            {
                requireReleaseBeforeRedraw = false;
            }
            previousFrameWorldCoordinate = currentWorldCoordinate;
        }

        private void SnapLastPositionToPointer()
        {
            if(lineRenderer.positionCount > 0)
            {
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, currentWorldCoordinate);
            }
        }

        /// <summary>
        /// Automatically add a new point if our pointer position changed direction (based on threshold) on the 2D plane 
        /// </summary>
        private void AutoAddPoint()
        {
            //automatically add a new point if pointer is far enough from last point, or edge normal is different enough from last line
            if (positions.Count == 0)
            {
                AddPoint(currentWorldCoordinate);
            }
            else
            {
                var normal = (currentWorldCoordinate - previousFrameWorldCoordinate);
                var distance = normal.sqrMagnitude;
                var normalisedNormal = normal.normalized;
                var directionThreshold = Vector3.Dot(normalisedNormal, lastNormal);
                if (distance > minDistanceBetweenAutoPoints && directionThreshold != 0 && directionThreshold < minDirectionThresholdForAutoPoints)
                {
                    AddPoint(previousFrameWorldCoordinate);
                    lastNormal = normalisedNormal;
                }
            }
        }

        private void CloseLoop()
        {
            //Connect loop to be closed by placing endpoint at same position as start ( or snap last point if close enough )
            closedLoop = true;
            lineRenderer.startColor = lineRenderer.endColor = closedLoopLineColor;
            positions.Add(positions[0]);

            requireReleaseBeforeRedraw = true;

            FinishPolygon();
        }

        private void Tap()
        {
            var currentPointerPosition = pointerAction.ReadValue<Vector2>();
            currentWorldCoordinate = GetCoordinateInWorld(currentPointerPosition);
            AddPoint(currentWorldCoordinate);
        }
        
        private void ClearPolygon()
        {
            lineRenderer.startColor = lineRenderer.endColor = lineColor;
            closedLoop = false;
            positions.Clear();
            lastNormal = Vector3.up;
            UpdateLine();
        }

        private void AddPoint(Vector3 pointPosition)
        {
            if(closedLoop)
            {
                ClearPolygon();
            }

            Debug.Log("Add point at " + pointPosition);
            //Added at start? finish and select
            if(positions.Count == 0)
            {
                selectionStartPosition = pointPosition;
                positions.Add(pointPosition); 
                lastAddedPoint = pointPosition;

                //Add extra point that we snap to our pointer to preview the next line
                positions.Add(pointPosition);
            }
            else
            {
                //Add point at our current preview position
                positions[positions.Count-1] = pointPosition;

                if (positions.Count > minPointsToCloseLoop)
                {
                    var distanceToStartingPoint = Vector3.Distance(positions[0], pointPosition);
                    Debug.Log($"distanceBetweenLastTwoPoints {distanceToStartingPoint}");
                    if (distanceToStartingPoint < closeLoopDistance)
                    {
                        CloseLoop();
                    }
                }
            }

            if(!closedLoop)
            {
                //Add new point for previewing
                lastAddedPoint = pointPosition;
                positions.Add(pointPosition);

                UpdateLine();
            }
        }
        private void UpdateLine()
        {
            lineRenderer.positionCount = positions.Count;
            lineRenderer.SetPositions(positions.ToArray());
        }


        private void StartClick()
        {
            var currentPointerPosition = pointerAction.ReadValue<Vector2>();
            selectionStartPosition = GetCoordinateInWorld(currentPointerPosition);
        }

        private void Release()
        {
            var currentPointerPosition = pointerAction.ReadValue<Vector2>();
            var selectionEndPosition = GetCoordinateInWorld(currentPointerPosition);
        }

        private void FinishPolygon()
        {
            Debug.Log($"Make selection.");
            selectedPolygonArea.started.Invoke(positions);
        }

        /// <summary>
        /// Get the position of a screen point in world coordinates ( on a plane )
        /// </summary>
        /// <param name="screenPoint">The point in screenpoint coordinates</param>
        /// <returns></returns>
        private Vector3 GetCoordinateInWorld(Vector3 screenPoint)
        {
            var screenRay = Camera.main.ScreenPointToRay(screenPoint);

            worldPlane.Raycast(screenRay, out float distance);
            var samplePoint = screenRay.GetPoint(Mathf.Min(maxSelectionDistanceFromCamera, distance));

            return samplePoint;
        }
    }
}