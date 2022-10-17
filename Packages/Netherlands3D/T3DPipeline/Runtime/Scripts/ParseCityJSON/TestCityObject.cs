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

    public class TestCityObject : MonoBehaviour
    {
        [SerializeField]
        private TextAsset testJson;
        protected void Start()
        {
            print(testJson.text);
            //var cityObjects = CityJSONParser.ParseCityJSON(testJson.text);
            var parsedJson = new CityJSON(testJson.text);

            SetRelativeCenter(parsedJson);

            foreach (var co in parsedJson.CityObjects)
            {
                co.gameObject.AddComponent<CityObjectVisualizer>();
            }
            string exportJson = CityJSONFormatter.GetCityJSON();
            print(exportJson);
            HandleTextFile.WriteString("export.json", exportJson);
        }

        private void SetRelativeCenter(CityJSON cityJson)
        {
            if (cityJson.CoordinateSystem == CoordinateSystem.RD)
            {
                var relativeCenterRD = (cityJson.MinExtent + cityJson.MaxExtent) / 2;
                print("Setting Relative RD Center to: x:" + relativeCenterRD.x + "\ty:" + relativeCenterRD.y + "\th:" + relativeCenterRD.z);
                CoordConvert.zeroGroundLevelY = (float)relativeCenterRD.z;
                CoordConvert.relativeCenterRD = new Vector2RD(relativeCenterRD.x, relativeCenterRD.y);
            }
        }
    }
}
