using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.T3DPipeline
{
    public class CityJSONExportButton : MonoBehaviour
    {
        public void PrintCityJSON()
        {
            Debug.Log(CityJSONFormatter.GetCityJSON());
        }
    }
}
