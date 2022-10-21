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
        public static void WriteString(string fileName, string content)
        {
            //Write some text to the test.txt file
            StreamWriter writer = new StreamWriter(fileName, false);
            writer.WriteLine(content);
            writer.Close();
            Debug.Log("saved file to: " + fileName);
        }
    }
}
