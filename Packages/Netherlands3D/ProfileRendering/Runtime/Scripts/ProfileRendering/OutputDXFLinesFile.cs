using netDxf;
using netDxf.Entities;
using Netherlands3D.Core;
using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Netherlands3D.ProfileRendering
{
    public class OutputDXFLinesFile : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern void DownloadFile(string callbackGameObjectName, string callbackMethodName, string fileName, byte[] array, int byteLength);

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
            onReceiveLayerName.AddListenerStarted(AddDXFLayer);
            onReceiveLayerLines.AddListenerStarted(AddDXFLayerLines);
            onReceiveLayerColor.AddListenerStarted(SetDXFLayerColor);
            onReadyForExport.AddListenerStarted(FinishDXFDocument);
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
            var dxfLayerName = ReturnDXFSafeLayerName(layerName);
            Debug.Log($"Add dxf layer {dxfLayerName} (created from {layerName})");

            ConfigBaseDocument();

            if (dxfDocument.Layers.Contains(dxfLayerName))
            {
                targetDxfLayer = dxfDocument.Layers[dxfLayerName];
            }
            else
            {
                targetDxfLayer = new netDxf.Tables.Layer(dxfLayerName);
                dxfDocument.Layers.Add(targetDxfLayer);
            }
        }

        private string ReturnDXFSafeLayerName(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_')
                {
                    sb.Append(c);
                }
            }
            if(sb.Length == 0)
                return "DefaultLayer";

            return sb.ToString();
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
            if (outputProgress) outputProgress.InvokeStarted(1.0f);
            yield return new WaitForEndOfFrame();
            SaveFile(dxfDocument);

            dxfDocument = null;
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
                    DownloadFile(this.gameObject.name, nameof(FileSaved), outputFileName, streamArray, streamArray.Length);
                }
                else
                {
                    Debug.Log("cant write file");
                }
            }
#endif
        }
        public void FileSaved()
        {
            Debug.Log("DXF saved");
        }
    }
}
