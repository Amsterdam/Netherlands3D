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

        for (int i = 0; i < lines.Count; i += 2)
        {
            if ((i % addLinesPerFrame) == 0)
            {
                if (outputProgress && i > 0) outputProgress.Invoke((float)i / lines.Count);
                yield return new WaitForEndOfFrame();
            }

            var rdStart = CoordConvert.UnitytoRD(lines[i]);
            var rdEnd = CoordConvert.UnitytoRD(lines[i + 1]);

            netDxf.Vector3 lineStart = new netDxf.Vector3(rdStart.x, rdStart.y, rdStart.z);
            netDxf.Vector3 lineEnd = new netDxf.Vector3(rdEnd.x, rdEnd.y, rdEnd.z);

            Line entity = new Line(lineStart, lineEnd);
            dxfDocument.Entities.Add(entity);
        }
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
            if(dxfDocument.Save(path, binary))
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

