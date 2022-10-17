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
            string path = "/Users/Tom/Documents/TSCD/T3D/CityJsonExports/";
            //Write some text to the test.txt file
            StreamWriter writer = new StreamWriter(path + fileName, false);
            writer.WriteLine(content);
            writer.Close();
            Debug.Log("saved file to: " + path + fileName);
        }
    }
}
