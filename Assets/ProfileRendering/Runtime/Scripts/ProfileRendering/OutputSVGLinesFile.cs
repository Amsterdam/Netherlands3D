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

namespace Netherlands3D.ProfileRendering
{
    public class OutputSVGLinesFile : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern void DownloadFile(byte[] array, int byteLength, string fileName);

        [Header("Input")]
        [SerializeField] Vector3ListEvent onReceiveLayerLines;
        [SerializeField] ColorEvent onReceiveLayerColor;
        [SerializeField] TriggerEvent onReadyForExport;
        [Header("Output")]
        [SerializeField] FloatEvent outputProgress;

        [Header("Settings")]
        [SerializeField] private string outputFileName = "Profile_Netherlands3D.svg";
        [SerializeField] private int addLinesPerFrame = 1000;
        [SerializeField] private float strokeWidth = 0.1f;

        private StringBuilder svgStringBuilder;

        void Awake()
        {
            onReceiveLayerLines.started.AddListener(OutputAsSVG);
        }

        private void OutputAsSVG(List<Vector3> lines)
        {
            StartCoroutine(CreatSVGDocument(lines));
        }

        private void CreateBaseSVG()
        {
            if(svgStringBuilder == null)
            {
                svgStringBuilder = new StringBuilder();
            }
        }

        private IEnumerator CreatSVGDocument(List<Vector3> lines)
        {
            var minX = lines.Min(vector => vector.x);
            var maxX = lines.Max(vector => vector.x);

            var minY = lines.Min(vector => vector.y);
            var maxY = lines.Max(vector => vector.y);

            var width = (maxX - minX);
            var height = (maxY - minY);

            svgStringBuilder.AppendLine($"<svg viewBox=\"0 0 {width} {height}\" xmlns=\"http://www.w3.org/2000/svg\">");
            for (int i = 0; i < lines.Count; i += 2)
            {
                if ((i % addLinesPerFrame) == 0)
                {
                    if (outputProgress && i > 0) outputProgress.Invoke((float)i / lines.Count);
                    yield return new WaitForEndOfFrame();
                }

                var lineStart = lines[i];
                var lineEnd = lines[i + 1];

                lineStart.x = (lineStart.x - minX);
                lineStart.y = (lineStart.y - minY);

                lineEnd.x = (lineEnd.x - minX);
                lineEnd.y = (lineEnd.y - minY);

                svgStringBuilder.AppendLine($"<line x1=\"{lineStart.x}\" y1=\"{height - lineStart.y}\" x2=\"{lineEnd.x}\" y2=\"{height - lineEnd.y}\" stroke=\"black\" stroke-width=\"{strokeWidth}\" />");
            }
            svgStringBuilder.AppendLine(" </svg>");

            if (outputProgress) outputProgress.Invoke(1.0f);
            yield return new WaitForEndOfFrame();
            SaveFile(svgStringBuilder);

        }
        public void SaveFile(StringBuilder stringBuilder)
        {
#if UNITY_EDITOR
            string path = EditorUtility.SaveFilePanel("Save profile SVG", "", outputFileName, "svg");
            if (path.Length != 0)
            {
                Debug.Log($"Saving svg: {path}");
                File.WriteAllText(path, stringBuilder.ToString());
            }
#elif !UNITY_EDITOR && UNITY_WEBGL
        byte[] buffer = Encoding.ASCII.GetBytes(stringBuilder.ToString());
        DownloadFile(buffer, buffer.Length, outputFileName);
#endif
        }
    }
}