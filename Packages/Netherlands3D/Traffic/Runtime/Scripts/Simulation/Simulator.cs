using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Netherlands3D.Core;

namespace Netherlands3D.Traffic.Simulation
{
    /// <summary>
    /// Handles traffic simulation
    /// </summary>
    /// <credit>OpenStreetMaps for providing street data</credit>
    public class Simulator : MonoBehaviour
    {
        private static readonly string requestPrefix = "https://overpass-api.de/api/interpreter?data=[out:json];";
        private static readonly string requestParam = "way[highway~\"motorway|trunk|primary|secondary|tertiary|motorway_link|trunk_link|primary_link|secondary_link|tertiary_link|unclassified|residential|living_street|track|road\"]";
        private static readonly string requestSuffix = "out geom;";

        public List<Road> roads = new List<Road>();

        private Vector3WGS wgsBottomLeft;
        private Vector3WGS wgsTopRight;

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        /// <summary>
        /// Get road data from Open Street Maps
        /// </summary>
        /// <returns>yield break. On WebRequest success it generates roads</returns>
        private IEnumerator GetRoadData()
        {
            string uri = string.Format("{0}{1}({2},{3},{4},{5},{6});",
                requestPrefix,
                requestParam,
                wgsBottomLeft.lat,
                wgsBottomLeft.lon,
                wgsTopRight.lat,
                wgsTopRight.lon,
                requestSuffix);
            Debug.Log("[Simulator] GetRoadData: " + uri);
            UnityWebRequest request = UnityWebRequest.Get(uri);                
            {
                yield return request.SendWebRequest();

                switch(request.result)
                {
                    case UnityWebRequest.Result.InProgress:
                        Debug.Log("[Simulator] InProgress...");
                        break;
                    case UnityWebRequest.Result.Success:
                        Debug.Log("[Simulator] UnityWebRequest Success");

                        break;
                    case UnityWebRequest.Result.ConnectionError:
                        Debug.LogWarning("[Simulator] ConnectionError: " + request.error);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogWarning("[Simulator] ProtocolError: " + request.error);
                        break;
                    case UnityWebRequest.Result.DataProcessingError: 
                        Debug.LogWarning("[Simulator] DataProcessingError: " + request.error);
                        break;
                    default:
                        break;
                }
            }

            yield break;
        }
    }
}
