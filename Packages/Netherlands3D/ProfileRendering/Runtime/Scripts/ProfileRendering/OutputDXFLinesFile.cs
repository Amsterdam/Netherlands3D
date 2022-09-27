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
        [SerializeField] Vector3ListEvent onReceiveLines;
        [Header("Output")]
        [SerializeField] FloatEvent outputProgress;

        [Header("Settings")]
        [SerializeField] private bool binary = false;
        [SerializeField] private int addLinesPerFrame = 1000;
        [SerializeField] private string outputFileName = "Profile_Netherlands3D.dxf";


        void Awake()
        {
            onReceiveLines.started.AddListener(OutputAsDXF);
        }

        private void OutputAsDXF(List<UnityEngine.Vector3> lines)
        {
            StartCoroutine(CreateDXFDocument(lines));
        }

        private IEnumerator CreateDXFDocument(List<UnityEngine.Vector3> lines)
        {
            DxfDocument dxfDocument = new DxfDocument();
            dxfDocument.DrawingVariables.InsUnits = netDxf.Units.DrawingUnits.Meters;

            var first = lines[0];
            first.y = 0;
            var last = lines[lines.Count - 1];
            last.y = 0;

            var coordinateSystem = new GameObject().transform;
            UnityEngine.Vector3 directionRight = (last - first).normalized;
            coordinateSystem.position = first;
            coordinateSystem.right = directionRight;

            for (int i = 0; i < lines.Count; i += 2)
            {
                if ((i % addLinesPerFrame) == 0)
                {
                    if (outputProgress && i > 0) outputProgress.Invoke((float)i / lines.Count);
                    yield return new WaitForEndOfFrame();
                }

                var rdStart = coordinateSystem.InverseTransformPoint(lines[i]);
                var rdEnd = coordinateSystem.InverseTransformPoint(lines[i + 1]);

                netDxf.Vector2 lineStart = new netDxf.Vector2(rdStart.x, rdStart.y);
                netDxf.Vector2 lineEnd = new netDxf.Vector2(rdEnd.x, rdEnd.y);

                Line entity = new Line(lineStart, lineEnd);
                dxfDocument.AddEntity(entity);
            }

            Destroy(coordinateSystem.gameObject);

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