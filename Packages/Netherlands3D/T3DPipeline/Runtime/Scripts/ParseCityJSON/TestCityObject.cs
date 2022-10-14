using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Netherlands3D.T3DPipeline
{
    public class TestCityObject : MonoBehaviour
    {
        [SerializeField]
        private TextAsset testJson;
        protected void Start()
        {
            print(testJson.text);
            CityJSONParser.ParseCityJSON(testJson.text);
            string exportJson = CityJSONFormatter.GetCityJSON();
            print(exportJson);
            //HandleTextFile.WriteString("export.json", exportJson);
            //print()
        }
    }
}