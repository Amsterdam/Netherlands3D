using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System;

namespace Netherlands3D.ProfileRendering
{
    public class OutputSVGLinesFile : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern void DownloadFile(string callbackGameObjectName, string callbackMethodName, string fileName, byte[] array, int byteLength);

        [Header("Input")]
        [SerializeField] Vector3ListEvent onReceiveLayerLines;
        [SerializeField] ColorEvent onReceiveLayerColor;
        [SerializeField] TriggerEvent onReadyForExport;
        [Header("Output")]
        [SerializeField] FloatEvent outputProgress;

        [Header("Settings")]
        [SerializeField] private string outputFileName = "Profile_Netherlands3D.svg";
        [SerializeField] private float strokeWidth = 0.1f;

        [SerializeField] private float documentHeight = 300;
        [SerializeField] private bool useMaterialColorForLines = false;

        private StringBuilder svgStringBuilder;
        private string hexStrokeColor = "#000000";
        private Transform coordinateSystem;

        private float minX = float.MaxValue;
        private float maxX = float.MinValue;
        private float minY = float.MaxValue;
        private float maxY = float.MinValue;

        void Awake()
        {
            if(onReceiveLayerLines)
                onReceiveLayerLines.AddListenerStarted(AddSVGLine);

            if(onReceiveLayerColor)
                onReceiveLayerColor.AddListenerStarted(SetStrokeColor);

            if(onReadyForExport)
                onReadyForExport.AddListenerStarted(FinishSVGDocument);
        }

        public void FinishSVGDocument()
        {
           Destroy(coordinateSystem.gameObject);
           StartCoroutine(CompleteAndSave());
        }

        private IEnumerator CompleteAndSave()
        {
            svgStringBuilder.Insert(0, $"<svg viewBox=\"{minX} {documentHeight/2} {maxX-minX} {documentHeight}\" xmlns=\"http://www.w3.org/2000/svg\">\n");
            svgStringBuilder.AppendLine(" </svg>");

            //Reset min and max for a next run
            minX = float.MaxValue;
            maxX = float.MinValue;
            minY = float.MaxValue;
            maxY = float.MinValue;

            if (outputProgress) outputProgress.InvokeStarted(1.0f);
            yield return new WaitForEndOfFrame();
            SaveFile();
        }

        public void AddSVGLine(List<Vector3> lines)
        {
            if (lines.Count < 2) return;

            CreateBaseSVG();
            CreateCoordinateSystem(lines);
            AppendSVGLine(lines);
        }

        private void AppendSVGLine(List<Vector3> lines)
        {
            for (int i = 0; i < lines.Count; i += 2)
            {
                var lineStart = coordinateSystem.InverseTransformPoint(lines[i]);
                var lineEnd = coordinateSystem.InverseTransformPoint(lines[i + 1]);
                CheckMinMax(lineStart);
                CheckMinMax(lineEnd);

                svgStringBuilder.AppendLine($"<line x1=\"{lineStart.x}\" y1=\"{documentHeight-lineStart.y}\" x2=\"{lineEnd.x}\" y2=\"{documentHeight-lineEnd.y}\" stroke=\"{hexStrokeColor}\" stroke-width=\"{strokeWidth}\" />");
            }
        }

        private void CheckMinMax(Vector3 linePoint)
        {
            if (linePoint.x < minX) minX = linePoint.x;
            else if (linePoint.x >= maxX) maxX = linePoint.x;
            if (linePoint.y < minY) minY = linePoint.y;
            else if (linePoint.y >= maxY) maxY = linePoint.y;
        }

        public void SetStrokeColor(Color color)
        {
            if (useMaterialColorForLines)
            {
                hexStrokeColor = $"#{ColorUtility.ToHtmlStringRGB(color)}";
            }
            else
            {
                hexStrokeColor = "#000000";
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

        private void CreateBaseSVG()
        {
            if(svgStringBuilder == null)
            {
                svgStringBuilder = new StringBuilder();
            }
        }

        public void SaveFile()
        {
#if UNITY_EDITOR
            string path = EditorUtility.SaveFilePanel("Save profile SVG", "", outputFileName, "svg");
            if (path.Length != 0)
            {
                Debug.Log($"Saving svg: {path}");
                File.WriteAllText(path, svgStringBuilder.ToString());
            }
#elif !UNITY_EDITOR && UNITY_WEBGL
            byte[] buffer = Encoding.ASCII.GetBytes(svgStringBuilder.ToString());
            DownloadFile(this.gameObject.name, nameof(FileSaved), outputFileName, buffer, buffer.Length);

#endif
            svgStringBuilder.Clear();
        }

        public void FileSaved()
        {
            Debug.Log("SVG saved");
        }
    }
}
