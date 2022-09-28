using netDxf;
using netDxf.Entities;
using Netherlands3D.Core;
using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace Netherlands3D.ProfileRendering
{
    public class OutputDXFLinesFile : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern void DownloadFile(byte[] array, int byteLength, string fileName);
        [Header("Input")]
        [SerializeField] StringEvent onReceiveLayerName;
        [SerializeField] ColorEvent onReceiveLayerColor;
        [SerializeField] Vector3ListEvent onReceiveLayerLines;
        [SerializeField] TriggerEvent onReadyForExport;
        [Header("Output")]
        [SerializeField] FloatEvent outputProgress;

        [Header("Settings")]
        [SerializeField] private bool binary = false;
        [SerializeField] private string outputFileName = "Profile_Netherlands3D.dxf";

        private DxfDocument dxfDocument;
        private netDxf.Tables.Layer targetDxfLayer;
        private Transform coordinateSystem;


        void Awake()
        {
            onReceiveLayerName.started.AddListener(AddDXFLayer);
            onReceiveLayerLines.started.AddListener(AddDXFLayerLines);
            onReceiveLayerColor.started.AddListener(SetDXFLayerColor);
            onReadyForExport.started.AddListener(FinishDXFDocument);
        }

        private void ConfigBaseDocument()
        {
            if (dxfDocument == null)
            {
                dxfDocument = new DxfDocument();
                dxfDocument.DrawingVariables.InsUnits = netDxf.Units.DrawingUnits.Meters;
            }
        }

        private void SetDXFLayerColor(Color color)
        {
            targetDxfLayer.Color = new AciColor(color.r,color.g,color.b);
        }

        private void AddDXFLayer(string layerName)
        {
            ConfigBaseDocument();
            targetDxfLayer = new netDxf.Tables.Layer(layerName);
            dxfDocument.Layers.Add(targetDxfLayer);
        }

        private void AddDXFLayerLines(List<UnityEngine.Vector3> lines)
        {
            if (lines == null || lines.Count < 2) return;

            CreateCoordinateSystem(lines);

            for (int i = 0; i < lines.Count; i += 2)
            {
                var rdStart = coordinateSystem.InverseTransformPoint(lines[i]);
                var rdEnd = coordinateSystem.InverseTransformPoint(lines[i + 1]);

                netDxf.Vector2 lineStart = new netDxf.Vector2(rdStart.x, rdStart.y);
                netDxf.Vector2 lineEnd = new netDxf.Vector2(rdEnd.x, rdEnd.y);

                Line lineEntity = new Line(lineStart, lineEnd);
                lineEntity.Layer = targetDxfLayer;
                dxfDocument.AddEntity(lineEntity);
            }
        }

        private void CreateCoordinateSystem(List<UnityEngine.Vector3> lines)
        {
            if (coordinateSystem) return;

            var first = lines[0];
            first.y = 0;
            var last = lines[lines.Count - 1];
            last.y = 0;
            coordinateSystem = new GameObject().transform;
            UnityEngine.Vector3 directionRight = (last - first).normalized;
            coordinateSystem.position = first;
            coordinateSystem.right = directionRight;
        }

        private void FinishDXFDocument()
        {
            Destroy(coordinateSystem.gameObject);
            StartCoroutine(FinishAndSave());
        }

        private IEnumerator FinishAndSave()
        {
            if (outputProgress) outputProgress.Invoke(1.0f);
            yield return new WaitForEndOfFrame();
            SaveFile(dxfDocument);
        }

        public void SaveFile(DxfDocument dxfDocument)
        {
#if UNITY_EDITOR
            string path = EditorUtility.SaveFilePanel("Save profile DXF", "", outputFileName, "dxf");
            if (path.Length != 0)
            {
                if (dxfDocument.Save(path, binary))
                {
                    Debug.Log($"Saved: {path}");
                }
                else
                {
                    Debug.Log($"Could not save: {path}");
                }
            }
#elif !UNITY_EDITOR && UNITY_WEBGL
            using (var stream = new MemoryStream())
            {
                if (dxfDocument.Save(stream, binary))
                {
                    var streamArray = stream.ToArray();
                    DownloadFile(streamArray, streamArray.Length, outputFileName);
                }
                else
                {
                    Debug.Log("cant write file");
                }
            }
#endif
        }
    }
}