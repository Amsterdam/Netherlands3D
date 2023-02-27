using System;
using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Events;
using Poly2Tri;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Netherlands3D.SelectionTools
{
    public class PolygonVisualisation : MonoBehaviour, IPointerClickHandler
    {
        public List<IList<Vector3>> Polygons;

        [Header("Events")]
        [SerializeField]
        private Vector3ListEvent reselectVisualisedPolygon;
        [SerializeField]
        private Vector3ListEvent onPolygonEdited;


        //[Header("Mesh")]
        float extrusionHeight;
        bool addBottom;
        bool reverseWindingOrder;
        Vector2 uvCoordinate;

        //[Header("Line")]
        private List<LineRenderer> lineRenderers = new List<LineRenderer>();
        [SerializeField]
        private bool drawLine;
        public bool DrawLine
        {
            get
            {
                return drawLine;
            }
            set
            {
                drawLine = value;
                EnableLineRenderers(value);
                //if (lineRenderers != null && drawLine)
                //{
                //    lineRenderers = CreateLineRenderers();
                //    EnableLineRenderers(true);
                //}
                //else if (lineRenderers == null && !drawLine)
                //    EnableLineRenderers(false);
            }
        }
        private Material lineMaterial;
        private Color lineColor;

        private List<LineRenderer> CreateLineRenderers(List<IList<Vector3>> polygons)
        {
            var list = new List<LineRenderer>();
            foreach (var contour in polygons)
            {
                list.Add(CreateAndReturnPolygonLine(contour as List<Vector3>));
            }
            return list;
        }

        private LineRenderer CreateAndReturnPolygonLine(List<Vector3> contour)
        {
            var newPolygonObject = new GameObject();
            newPolygonObject.transform.SetParent(transform);
            newPolygonObject.name = "PolygonOutline";
            var lineRenderer = newPolygonObject.AddComponent<LineRenderer>();
            lineRenderer.material = lineMaterial;
            lineRenderer.startColor = lineColor;
            lineRenderer.endColor = lineColor;

            lineRenderer.loop = true;

            lineRenderer.positionCount = contour.Count;
            lineRenderer.SetPositions(contour.ToArray()); //does not work for some reason

            return lineRenderer;
        }

        private void EnableLineRenderers(bool enable)
        {
            foreach(var line in lineRenderers)
            {
                line.gameObject.SetActive(enable);
            }
        }

        public void DestroyLineRenderers()
        {
            for (int i = lineRenderers.Count - 1; i >= 0; i--)
            {
                Destroy(lineRenderers[i].gameObject);
            }
            lineRenderers = new List<LineRenderer>();
        }

        public void UpdateLineRenderers()
        {
            DestroyLineRenderers();
            lineRenderers = CreateLineRenderers(Polygons);
        }

        /// <summary>
        /// Sets a reference of the polygon to be visualised
        /// </summary>
        /// <param name="polygon"></param>
        public void Initialize(List<IList<Vector3>> sourcePolygons, float extrusionHeight, bool addBottom, bool reverseWindingOrder, Vector3ListEvent reselectVisualisedPolygon, Vector3ListEvent onPolygonEdited, Material lineMaterial, Color lineColor, Vector2 uvCoordinate = new Vector2())
        {
            Polygons = sourcePolygons;
            this.extrusionHeight = extrusionHeight;
            this.addBottom = addBottom;
            this.reverseWindingOrder = reverseWindingOrder;
            this.uvCoordinate = uvCoordinate;
            this.reselectVisualisedPolygon = reselectVisualisedPolygon;
            this.onPolygonEdited = onPolygonEdited;
            this.lineMaterial = lineMaterial;
            this.lineColor = lineColor;

            //if (updateVisualisation)
            //{
                UpdateVisualisation(sourcePolygons);
            //}

            ReselectPolygon();
        }

        public void UpdateVisualisation(List<Vector3> newPolygon)
        {
            UpdateVisualisation(new List<IList<Vector3>>() { newPolygon });
        }

        public void UpdateVisualisation(List<IList<Vector3>> newPolygon)
        {
            Polygons = newPolygon;
            var mesh = CreatepolygonMesh(Polygons, extrusionHeight, addBottom, reverseWindingOrder, uvCoordinate);
            GetComponent<MeshFilter>().mesh = mesh;

            UpdateLineRenderers();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            ReselectPolygon();
        }

        private void ReselectPolygon()
        {
            onPolygonEdited.RemoveAllListenersStarted();
            reselectVisualisedPolygon.InvokeStarted(Polygons[0] as List<Vector3>);
            onPolygonEdited.AddListenerStarted(UpdateVisualisation);
        }

        public static Mesh CreatepolygonMesh(List<IList<Vector3>> contours, float extrusionHeight, bool addBottom, bool reverseWindingOrder = false, Vector2 uvCoordinate = new Vector2())
        {
            var polygon = new Poly2Mesh.Polygon();
            var outerContour = (List<Vector3>)contours[0];

            if (outerContour.Count < 3) return null;
            if (reverseWindingOrder) outerContour.Reverse();
            polygon.outside = outerContour;

            for (int i = 0; i < polygon.outside.Count; i++)
            {
                polygon.outside[i] = new Vector3(polygon.outside[i].x, polygon.outside[i].y, polygon.outside[i].z);
            }

            if (contours.Count > 1)
            {
                for (int i = 1; i < contours.Count; i++)
                {
                    var holeContour = (List<Vector3>)contours[i];
                    FixSequentialDoubles(holeContour);

                    if (holeContour.Count > 2)
                    {
                        if (reverseWindingOrder) holeContour.Reverse();
                        polygon.holes.Add(holeContour);
                    }
                }
            }
            var newPolygonMesh = Poly2Mesh.CreateMesh(polygon, extrusionHeight, addBottom);
            if (newPolygonMesh) newPolygonMesh.RecalculateNormals();

            //if (setUVCoordinates)
            //{
            SetUVCoordinates(newPolygonMesh, uvCoordinate);
            //}

            return newPolygonMesh;
        }

        /// <summary>
        /// Poly2Mesh has problems with polygons that have points in the same position.
        /// Lets move them a bit.
        /// </summary>
        /// <param name="contour"></param>
        private static void FixSequentialDoubles(List<Vector3> contour)
        {
            var removedSomeDoubles = false;
            for (int i = contour.Count - 2; i >= 0; i--)
            {
                if (contour[i] == contour[i + 1])
                {
                    contour.RemoveAt(i + 1);
                    removedSomeDoubles = true;
                }
            }
            if (removedSomeDoubles) Debug.Log("Removed some doubles");
        }

        private static void SetUVCoordinates(Mesh newPolygonMesh, Vector2 uvCoordinate)
        {
            var uvs = new Vector2[newPolygonMesh.vertexCount];
            for (int i = 0; i < uvs.Length; i++)
            {
                uvs[i] = uvCoordinate;
            }
            newPolygonMesh.uv = uvs;
        }
    }
}