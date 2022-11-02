using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Netherlands3D.Core;
using UnityEngine;

namespace Netherlands3D.T3DPipeline
{
    public static class HandleTextFile
    {
        //Write some text to a file
        public static void WriteString(string fileName, string content)
        {
            StreamWriter writer = new StreamWriter(fileName, false);
            writer.WriteLine(content);
            writer.Close();
            Debug.Log("saved file to: " + fileName);
        }
    }
}
