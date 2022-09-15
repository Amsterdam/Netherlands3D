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
        [SerializeField, Tooltip("Contains the list of points the line is made of")] private Vector3ListEvent lineHasChanged;

        [Header("Settings")]
        [SerializeField] Color lineColor = Color.red;
        [SerializeField] Color closedLoopLineColor = Color.red;
        [SerializeField] private float lineWidthMultiplier = 10.0f;
        [SerializeField] private float maxSelectionDistanceFromCamera = 10000;
        [SerializeField] private bool useWorldSpace = false;
        [SerializeField] private bool snapToStart = true;
        [SerializeField, Tooltip("Closing a polygon shape is required. If set to false, you can output lines.")] private bool requireClosedPolygon = true;
        [SerializeField, Tooltip("If you click close to the starting point the loop will finish")] private bool closeLoopAtStart = true;
        [SerializeField] private int maxPoints = 1000;
        [SerializeField] private int minPointsToCloseLoop = 3;
        [SerializeField] private float minPointDistance = 0.1f;
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

        [SerializeField] private LineRenderer polygonLineRenderer;
        [SerializeField] private LineRenderer previewLineRenderer;
        [SerializeField] private List<Vector3> positions = new List<Vector3>();
        private Vector3 lastAddedPoint = default;
        private Vector3 selectionStartPosition = default;
        private Vector3 currentWorldCoordinate = default;
        private Vector3 previousFrameWorldCoordinate = default;
        private Vector2 previousFrameScreenCoordinate = default;
        private Vector3 lastNormal = Vector3.zero;
        private Plane worldPlane;

        private bool closedLoop = false;
        private bool snappingToStartPoint = false;
        private bool polygonFinished = false;
        private bool previewLineCrossed = false;
        private bool autoDrawPolygon = false;
        private bool requireReleaseBeforeRedraw = false;
        private Camera mainCamera;

        private float lastTapTime = 0;

        [SerializeField] private Transform pointerRepresentation;

        void Awake()
        {
            mainCamera = Camera.main;
            polygonLineRenderer.startColor = polygonLineRenderer.endColor = lineColor;
            polygonLineRenderer.widthMultiplier = lineWidthMultiplier;
            polygonLineRenderer.positionCount = 1;
            polygonLineRenderer.loop = false;

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
            finishAction.performed += context => CloseLoop(true);

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

            UpdatePreviewLine();
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
            else if (requireReleaseBeforeRedraw && !clickAction.IsPressed())
            {
                requireReleaseBeforeRedraw = false;
            }
            previousFrameWorldCoordinate = currentWorldCoordinate;
        }

        /// <summary>
        /// Draw the preview line between last placed point and current pointer position
        /// </summary>
        private void UpdatePreviewLine()
        {
            if (positions.Count == 0) return;

            snappingToStartPoint = false;
            if(snapToStart && positions.Count > 2)
            {
                if (Vector3.Distance(currentWorldCoordinate, positions[0]) < minPointDistance)
                {
                    currentWorldCoordinate = positions[0];
                    snappingToStartPoint = true;
                }
            }

            var previewLineFirstPoint = positions[positions.Count - 1];
            var previewLineLastPoint = currentWorldCoordinate;
            previewLineRenderer.SetPosition(0, previewLineFirstPoint);
            previewLineRenderer.SetPosition(1, previewLineLastPoint);
            previewLineCrossed = false;

            //Compare all lines in drawing if we do not cross (except last, we cant cross that one)
            for (int i = 1; i < polygonLineRenderer.positionCount-1; i++)
            {
                if (snappingToStartPoint && i == 1) continue; //Skip first line check if we are snapping to it

                var comparisonStart = polygonLineRenderer.GetPosition(i - 1);
                var comparisonEnd = polygonLineRenderer.GetPosition(i);
                if (LinesIntersectOnPlane(previewLineFirstPoint, previewLineLastPoint, comparisonStart, comparisonEnd))
                {
                    previewLineCrossed = true;
                    previewLineRenderer.startColor = previewLineRenderer.endColor = Color.red;
                    Debug.Log("Preview line crosses another line");
                    return;
                }
            }

            previewLineRenderer.startColor = previewLineRenderer.endColor = Color.green;
        }

        /// <summary>
        /// Compare line with placed lines to check if they do not intersect.
        /// </summary>
        /// <param name="linePointA">Start point of the line we want to check</param>
        /// <param name="linePointB">End point of the line we want to check</param>
        /// <param name="skipFirst">Skip the first line in our chain</param>
        /// <param name="skipLast">Skip the last line in our chain</param>
        /// <returns>Returns true if an intersection was found</returns>
        private bool LineCrossesOtherLine(Vector3 linePointA, Vector3 linePointB, bool skipFirst = false, bool skipLast = false)
        {
            int startIndex = (skipFirst) ? 2 : 1;
            int endIndex = (skipLast) ? polygonLineRenderer.positionCount-1 : polygonLineRenderer.positionCount;
            for (int i = startIndex; i < endIndex; i++)
            {
                var comparisonStart = polygonLineRenderer.GetPosition(i - 1);
                var comparisonEnd = polygonLineRenderer.GetPosition(i);
                if (LinesIntersectOnPlane(linePointA, linePointB, comparisonStart, comparisonEnd))
                {
                    Debug.Log("Line is crossing other line! This is not allowed.");
                    return true;
                }
            }
            return false;
        }


        public static bool LinesIntersectOnPlane(Vector3 lineOneA, Vector3 lineOneB, Vector3 lineTwoA, Vector3 lineTwoB)
        {
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
            //Clear on fresh start
            if(polygonFinished) ClearPolygon(true);

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

        private void CloseLoop(bool connectLastPointToStart, bool checkPreviewLine = true)
        {
            if (requireClosedPolygon)
            {
                if (positions.Count < minPointsToCloseLoop)
                {
                    Debug.Log("Not closing loop. Need more points.");
                    return;
                }

                if (checkPreviewLine && previewLineCrossed)
                {
                    Debug.Log("Not closing loop. Preview line is crossing another line");
                    return;
                }

                if (connectLastPointToStart)
                {
                    var lastPointOnTopOfFirst = (Vector3.Distance(positions[0], positions[positions.Count - 1]) < minPointDistance);
                    if (lastPointOnTopOfFirst)
                    {
                        Debug.Log("Closing loop by placing last point on first");
                        positions[positions.Count - 1] = positions[0];
                    }
                    else
                    {
                        Debug.Log("Try to add a finishing line.");
                        var closingLineStart = positions[positions.Count - 1];
                        var closingLineEnd = positions[0];
                        if (LineCrossesOtherLine(closingLineStart, closingLineEnd, true, true))
                        {
                            Debug.Log("Cant close loop, closing line will cross another line.");
                            return;
                        }
                        else
                        {
                            positions.Add(closingLineEnd);
                        }
                    }
                }
            }

            closedLoop = true;
            polygonLineRenderer.startColor = polygonLineRenderer.endColor = closedLoopLineColor;

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
                    CloseLoop(true);
                    return;
                }
                else
                {
                    lastTapTime = Time.time;
                    previousFrameScreenCoordinate = currentPointerPosition;
                }
            }

            AddPoint(currentWorldCoordinate);
        }

        private void ClearPolygon(bool redraw = false)
        {
            polygonFinished = false;

            polygonLineRenderer.startColor = polygonLineRenderer.endColor = lineColor;
            closedLoop = false;
            previewLineCrossed = false;
            positions.Clear();
            lastNormal = Vector3.zero;
            previewLineRenderer.enabled = false;

            if (redraw)
                UpdateLine();
        }

        private void AddPoint(Vector3 pointPosition)
        {
            //Clear on fresh start
            if (polygonFinished) ClearPolygon(true);

            //Added at start? finish and select
            if (positions.Count == 0)
            {
                Debug.Log("Placing first point at " + pointPosition);
                previewLineRenderer.enabled = true;
                selectionStartPosition = pointPosition;
                positions.Add(pointPosition);
                lastAddedPoint = pointPosition;
            }
            else if (previewLineCrossed)
            {
                Debug.Log("Cant place point. Crossing own line.");
                return;
            }
            else if (Vector3.Distance(pointPosition, positions[positions.Count - 1]) < minPointDistance)
            {
                Debug.Log("Point at same location as previous, skipping this one.");
                return;
            }
            else
            {
                Debug.Log("Adding new point.");
                positions.Add(pointPosition);
                lastAddedPoint = pointPosition;
                if ((positions.Count >= maxPoints) || closeLoopAtStart && snappingToStartPoint)
                {
                    CloseLoop(true);
                }
            }

            

            if (!closedLoop)
                UpdateLine();
        }

        /// <summary>
        /// Apply positions to LineRenderer positions
        /// </summary>
        private void UpdateLine()
        {
            polygonLineRenderer.positionCount = positions.Count;
            polygonLineRenderer.SetPositions(positions.ToArray());

            if (positions.Count > 1 && lineHasChanged)
            {
                lineHasChanged.started.Invoke(positions);
            }
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
            polygonFinished = true;

            previewLineRenderer.enabled = false;

            var polygonIsClockwise = PolygonIsClockwise(positions);
            if ((windingOrder == WindingOrder.COUNTERCLOCKWISE && polygonIsClockwise) || (windingOrder == WindingOrder.CLOCKWISE && !polygonIsClockwise))
            {
                Debug.Log($"Forcing to {windingOrder}");
                positions.Reverse();
            }

            if(positions.Count > 1)
                selectedPolygonArea.started.Invoke(positions);
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