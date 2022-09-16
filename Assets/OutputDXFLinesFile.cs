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

    [SerializeField] Vector3ListEvent onReceiveLines;
    [SerializeField] private bool binary = false;
    [SerializeField] private string outputFileName = "Profile_Netherlands3D.dxf";
    void Awake()
    {
        onReceiveLines.started.AddListener(OutputAsDXF);
    }

    private void OutputAsDXF(List<UnityEngine.Vector3> lines)
    {
        DxfDocument dxfDocument = new DxfDocument();
        dxfDocument.DrawingVariables.InsUnits = netDxf.Units.DrawingUnits.Meters;

        for (int i = 0; i < lines.Count; i += 2)
        {
            var rdStart = CoordConvert.UnitytoRD(lines[0]);
            var rdEnd = CoordConvert.UnitytoRD(lines[1]);

            netDxf.Vector3 lineStart = new netDxf.Vector3(rdStart.x, rdStart.y, rdStart.z);
            netDxf.Vector3 lineEnd = new netDxf.Vector3(rdEnd.x, rdEnd.y, rdEnd.z);

            Line entity = new Line(lineStart, lineEnd);
            dxfDocument.Entities.Add(entity);
        }

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
#else
            using (var stream = new MemoryStream())
            {
                if (dxfDocument.Save(stream, binary))
                {
                    DownloadFile(stream.ToArray(), stream.ToArray().Length, outputFileName);
                }
                else
                {
                    Debug.Log("cant write file");
                }
            }
#endif
    }
}

