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
        [System.Serializable]
        public enum WindingOrder
        {
            CLOCKWISE,
            COUNTERCLOCKWISE
        }

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
        [SerializeField] private WindingOrder windingOrder = WindingOrder.CLOCKWISE;
        [SerializeField] private bool doubleClickToCloseLoop = true;
        [SerializeField] private float doubleClickTimer = 0.5f;
        [SerializeField] private float doubleClickDistance = 10.0f;
        [SerializeField] private bool displayLineUntilRedraw = true;

        private InputAction pointerAction;
        private InputAction tapAction;
        private InputAction escapeAction;
        private InputAction finishAction;
        private InputAction tapSecondaryAction;
        private InputAction clickAction;
        private InputAction modifierAction;

        private LineRenderer lineRenderer;
        [SerializeField] private List<Vector3> positions = new List<Vector3>();
        private Vector3 lastAddedPoint = default;
        private Vector3 selectionStartPosition = default;
        private Vector3 currentWorldCoordinate = default;
        private Vector3 previousFrameWorldCoordinate = default;
        private Vector2 previousFrameScreenCoordinate = default;
        private Vector3 lastNormal = Vector3.zero;
        private Plane worldPlane;

        private bool closedLoop = false;
        private bool lineCrossed = false;
        private bool autoDrawPolygon = false;
        private bool requireReleaseBeforeRedraw = false;
        private Camera mainCamera;

        private float lastTapTime = 0;

        [SerializeField] private Transform pointerRepresentation;

        void Awake()
        {
            mainCamera = Camera.main;
            lineRenderer = this.GetComponent<LineRenderer>();
            lineRenderer.startColor = lineRenderer.endColor = lineColor;
            lineRenderer.widthMultiplier = lineWidthMultiplier;
            lineRenderer.positionCount = 1;

            polygonSelectionActionMap = inputActionAsset.FindActionMap("PolygonSelection");
            tapAction = polygonSelectionActionMap.FindAction("Tap");
            escapeAction = polygonSelectionActionMap.FindAction("Escape");
            finishAction = polygonSelectionActionMap.FindAction("Finish");
            tapSecondaryAction = polygonSelectionActionMap.FindAction("TapSecondary");
            clickAction = polygonSelectionActionMap.FindAction("Click");
            pointerAction = polygonSelectionActionMap.FindAction("Point");
            modifierAction = polygonSelectionActionMap.FindAction("Modifier");

            tapAction.performed += context => Tap();
            clickAction.performed += context => StartClick();
            clickAction.canceled += context => Release();
            escapeAction.canceled += context => ClearPolygon(true);
            finishAction.performed += context => CloseLoop(false,false);

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

        /// <summary>
        /// Keep last line part attached to pointer, to preview next placement.
        /// Turn red if the line crosses another line ( no cross sections allowed in our polygon! )
        /// </summary>
        private void SnapLastPositionToPointer()
        {
            if(positions.Count > 0)
            {
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, currentWorldCoordinate);
                if(positions.Count > 3)
                {
                    lineCrossed = LastLineCrossesOtherLine();
                    if (lineCrossed)
                    {
                        lineRenderer.endColor = Color.red;
                    }
                    else
                    {
                        lineRenderer.endColor = lineColor;
                    }
                }
                else
                {
                    lineCrossed = false;
                }
            }
        }

        private bool LastLineCrossesOtherLine(bool includeClosingLineCheck = false)
        {
            var lastPoint = lineRenderer.GetPosition(lineRenderer.positionCount-1);
            var previousPoint = lineRenderer.GetPosition(lineRenderer.positionCount - 2);
            for (int i = 1; i < lineRenderer.positionCount; i++)
            {
                var comparisonStart = lineRenderer.GetPosition(i - 1);
                var comparisonEnd = lineRenderer.GetPosition(i);
                if (LinesIntersectOnPlane(lastPoint,previousPoint, comparisonStart,comparisonEnd))
                {
                    Debug.Log("Line to be placed is crossing another line");
                    return true;
                }
            }

            if(includeClosingLineCheck)
            {
                var firstPoint = lineRenderer.GetPosition(0);
                for (int i = 1; i < lineRenderer.positionCount-1; i++)
                {
                    var comparisonStart = lineRenderer.GetPosition(i - 1);
                    var comparisonEnd = lineRenderer.GetPosition(i);
                    if (LinesIntersectOnPlane(lastPoint, firstPoint, comparisonStart, comparisonEnd))
                    {
                        Debug.Log("Closing line crosses another line");
                        return true;
                    }
                }
            }
            return false;
        }


        public static bool LinesIntersectOnPlane(Vector3 lineOneA, Vector3 lineOneB, Vector3 lineTwoA, Vector3 lineTwoB) { 
            return 
                (((lineTwoB.z - lineOneA.z) * (lineTwoA.x - lineOneA.x) > (lineTwoA.z - lineOneA.z) * (lineTwoB.x - lineOneA.x)) != 
                ((lineTwoB.z - lineOneB.z) * (lineTwoA.x - lineOneB.x) > (lineTwoA.z - lineOneB.z) * (lineTwoB.x - lineOneB.x)) && 
                ((lineTwoA.z - lineOneA.z) * (lineOneB.x - lineOneA.x) > (lineOneB.z - lineOneA.z) * (lineTwoA.x - lineOneA.x)) != 
                ((lineTwoB.z - lineOneA.z) * (lineOneB.x - lineOneA.x) > (lineOneB.z - lineOneA.z) * (lineTwoB.x - lineOneA.x))); 
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
                if (distance > minDistanceBetweenAutoPoints && (lastNormal == Vector3.zero || (directionThreshold != 0 && directionThreshold < minDirectionThresholdForAutoPoints)))
                {
                    AddPoint(previousFrameWorldCoordinate);
                    lastNormal = normalisedNormal;
                }
            }
        }

        private void CloseLoop(bool snapLastPointToEnd, bool checkClosingLine = true)
        {
            if (positions.Count < 3)
            {
                Debug.Log("Not closing loop. Need more points.");
                return;
            }

            if(LastLineCrossesOtherLine(checkClosingLine))
            {
                Debug.Log("Not closing loop. There is an intersection.");
                return;
            }

            //Connect loop to be closed by placing endpoint at same position as start ( or snap last point if close enough )
            closedLoop = true;
            lineRenderer.startColor = lineRenderer.endColor = closedLoopLineColor;

            if (snapLastPointToEnd)
            {
                positions[positions.Count - 1] = positions[0];
            }
            else
            {
                positions.Add(positions[0]);
            }
            UpdateLine();
            FinishPolygon();
        }

        private void Tap()
        {
            var currentPointerPosition = pointerAction.ReadValue<Vector2>();
            currentWorldCoordinate = GetCoordinateInWorld(currentPointerPosition);

            if (doubleClickToCloseLoop)
            {
                if ((Time.time - lastTapTime) < doubleClickTimer && Vector3.Distance(currentPointerPosition, previousFrameScreenCoordinate) < doubleClickDistance)
                {
                    Debug.Log("Double click, closing loop.");
                    CloseLoop(true,false);
                }
                else
                {
                    AddPoint(currentWorldCoordinate);
                }
                lastTapTime = Time.time;
                previousFrameScreenCoordinate = currentPointerPosition;
            }
            else
            {
                AddPoint(currentWorldCoordinate);
            }
        }
        
        private void ClearPolygon(bool redraw = false)
        {
            lineRenderer.startColor = lineRenderer.endColor = lineColor;
            closedLoop = false;
            lineCrossed = false;
            positions.Clear();
            lastNormal = Vector3.zero;

            if(redraw)
                UpdateLine();
        }

        private void AddPoint(Vector3 pointPosition)
        {
            
            Debug.Log("PLacing point at " + pointPosition);

            //Added at start? finish and select
            if(positions.Count == 0)
            {
                selectionStartPosition = pointPosition;
                positions.Add(pointPosition); 
                lastAddedPoint = pointPosition;

                //Add extra point that we snap to our pointer to preview the next line
                positions.Add(pointPosition);
            }
            else if(positions.Count > 1 && pointPosition == positions[positions.Count - 2])
            {
                Debug.Log("Point at same location, skip");
                return;
            }
            else
            {
                if (positions.Count > 2)
                {
                    lineCrossed = LastLineCrossesOtherLine();
                }
                if (lineCrossed)
                {
                    Debug.Log("Cant place point. Crossing own line.");
                    return;
                }

                //Add point at our current preview position
                positions[positions.Count-1] = pointPosition;

                if (positions.Count > minPointsToCloseLoop)
                {
                    CheckLoopClose(pointPosition);
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

        private void CheckLoopClose(Vector3 pointPosition)
        {
            if (positions.Count < 3) return;

            var distanceToStartingPoint = Vector3.Distance(positions[0], pointPosition);
            Debug.Log($"distanceBetweenLastTwoPoints {distanceToStartingPoint}");
            if (distanceToStartingPoint < closeLoopDistance)
            {
                CloseLoop(true,false);
            }
        }

        /// <summary>
        /// Apply positions to LineRenderer positions
        /// </summary>
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
            requireReleaseBeforeRedraw = true;

            var polygonIsClockwise = PolygonIsClockwise(positions);
            if ((windingOrder == WindingOrder.COUNTERCLOCKWISE && polygonIsClockwise) || (windingOrder == WindingOrder.CLOCKWISE && !polygonIsClockwise))
            {
                Debug.Log($"Forcing to {windingOrder}");
                positions.Reverse();
            }

            selectedPolygonArea.started.Invoke(positions);
            ClearPolygon(!displayLineUntilRedraw);
        }

        private bool PolygonIsClockwise(List<Vector3> points)
        {
            bool isClockwise = false;
            double sum = 0;
            for (int i = 0; i < points.Count - 1; i++)
            {
                sum += (points[i + 1].x - points[i].x) * (points[i + 1].z + points[i].z);
            }
            isClockwise = (sum > 0) ? true : false;
            return isClockwise;
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