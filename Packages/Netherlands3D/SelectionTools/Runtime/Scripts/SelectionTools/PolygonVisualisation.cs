using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Netherlands3D.Events;
using Poly2Tri;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Netherlands3D.SelectionTools
{
    public class PolygonVisualisation : MonoBehaviour, IPointerClickHandler
    {
        private List<IList<Vector3>> polygons;
        public List<IList<Vector3>> Polygons => polygons; //todo
        //public ReadOnlyCollection<ReadOnlyCollection<Vector3>> Polygons
        //{
        //    get
        //    {
        //        List<ReadOnlyCollection<Vector3>> roPolygons = new List<ReadOnlyCollection<Vector3>>();
        //        foreach (var polygon in polygons)
        //        {
        //            var p = (List<Vector3>)polygon;
        //            roPolygons.Add(p.AsReadOnly());
        //        }
        //        return roPolygons.AsReadOnly();
        //    }
        //}

        [Header("Events")]
        //[SerializeField]
        public UnityEvent<PolygonVisualisation> reselectVisualisedPolygon = new UnityEvent<PolygonVisualisation>();

        //[Header("Mesh")]
        private float extrusionHeight;
        private bool addBottom;
        private bool createInwardMesh;
        private Vector2 uvCoordinate;

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
            }
        }
        private Material lineMaterial;
        private Color lineColor;

        /// <summary>
        /// Sets a reference of the polygon to be visualised
        /// </summary>
        /// <param name="polygon"></param>
        public void Initialize(List<IList<Vector3>> sourcePolygons, float extrusionHeight, bool addBottom, bool createInwardMesh, Material lineMaterial, Color lineColor, Vector2 uvCoordinate = new Vector2())
        {
            polygons = sourcePolygons;
            this.extrusionHeight = extrusionHeight;
            this.addBottom = addBottom;
            this.createInwardMesh = createInwardMesh;
            this.uvCoordinate = uvCoordinate;
            this.lineMaterial = lineMaterial;
            this.lineColor = lineColor;

            UpdateVisualisation(sourcePolygons);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            ReselectPolygon();
        }

        public void ReselectPolygon()
        {
            reselectVisualisedPolygon.Invoke(this);
        }

        public void UpdateVisualisation(List<Vector3> newPolygon)
        {
            UpdateVisualisation(new List<IList<Vector3>>() { newPolygon });
        }

        public void UpdateVisualisation(List<IList<Vector3>> newPolygon)
        {
            polygons = newPolygon;

            var polygon2D = PolygonCalculator.FlattenPolygon(newPolygon[0], new Plane(Vector3.up,0));
            var clockwise = PolygonCalculator.PolygonIsClockwise(polygon2D);

            if (clockwise == createInwardMesh) // (clockwise && inward) || (!clockwise && !inward)
            {
                foreach (var contour in newPolygon)
                {
                    var list = (List<Vector3>)contour;
                    list.Reverse();
                }
            }

            var mesh = PolygonVisualisationUtility.CreatePolygonMesh(polygons, extrusionHeight, addBottom, uvCoordinate);
            GetComponent<MeshFilter>().mesh = mesh;

            var mc = GetComponent<MeshCollider>();
            if (mc)
                mc.sharedMesh = mesh;

            UpdateLineRenderers();
        }

        public void UpdateLineRenderers() //todo: reuse existing line renderers if this is possible and if this is significantly more performant
        {
            DestroyLineRenderers();
            lineRenderers = PolygonVisualisationUtility.CreateLineRenderers(polygons, lineMaterial, lineColor, transform);
        }

        private void DestroyLineRenderers()
        {
            for (int i = lineRenderers.Count - 1; i >= 0; i--)
            {
                Destroy(lineRenderers[i].gameObject);
            }
            lineRenderers = new List<LineRenderer>();
        }

        private void EnableLineRenderers(bool enable) // to set this programatically, set the property DrawLine
        {
            foreach (var line in lineRenderers)
            {
                line.gameObject.SetActive(enable);
            }
        }
    }
}