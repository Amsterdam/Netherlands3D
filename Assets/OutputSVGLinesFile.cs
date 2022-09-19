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

public class OutputSVGLinesFile : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void DownloadFile(byte[] array, int byteLength, string fileName);

    [SerializeField] Vector3ListEvent onReceiveLines;
    [SerializeField] private string outputFileName = "Profile_Netherlands3D.svg";
    [SerializeField] private int addLinesPerFrame = 1000;

    [SerializeField] private float multiplyCoordinates = 1f;


    void Awake()
    {
        onReceiveLines.started.AddListener(OutputAsSVG);
    }

    private void OutputAsSVG(List<Vector3> lines)
    {
        StartCoroutine(CreatSVGDocument(lines));
    }

    private IEnumerator CreatSVGDocument(List<Vector3> lines)
    {
        StringBuilder svgStringBuilder = new StringBuilder();

        var minX = lines.Min(vector => vector.x);
        var maxX = lines.Max(vector => vector.x);

        var minY = lines.Min(vector => vector.y);
        var maxY = lines.Max(vector => vector.y);

        var width = (maxX - minX) * multiplyCoordinates;
        var height = (maxY - minY) * multiplyCoordinates;

        svgStringBuilder.AppendLine($"<svg viewBox=\"{-(width/2.0f)} {-(height / 2.0f)} {width} {height}\" xmlns=\"http://www.w3.org/2000/svg\">");
        for (int i = 0; i < lines.Count; i += 2)
        {
            if ((i % addLinesPerFrame) == 0) yield return new WaitForEndOfFrame();

            var lineStart = lines[i] * multiplyCoordinates; 
            var lineEnd = lines[i+1] * multiplyCoordinates; 
            svgStringBuilder.AppendLine($"<line x1=\"{lineStart.x}\" y1=\"{height-lineStart.y}\" x2=\"{lineEnd.x}\" y2=\"{height-lineEnd.y}\" stroke=\"black\" />");
        }
        svgStringBuilder.AppendLine(" </svg>");
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
