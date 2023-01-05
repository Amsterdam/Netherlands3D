using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Netherlands3D.Core;
using SimpleJSON;

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

        [SerializeField] private RoadsData roadsData;

        private Vector3WGS wgsBottomLeft;
        private Vector3WGS wgsTopRight;

        // Start is called before the first frame update
        void Start()
        {
            GetRoadData();
        }

        /// <summary>
        /// Generate roads from json data
        /// </summary>
        /// <param name="json"></param>
        /// <see cref="GetRoadData"/>
        private void GenerateRoadsFromData(string json)
        {
            JSONNode n = JSON.Parse(json);
            for(int i = 0; i < n["elements"].Count; i++)
            {
                roadsData.Add(new Road(n["elements"][i]));
            }
        }

        private void GetRoadData()
        {
            StartCoroutine(GetRoadDataAysnc());
        }

        /// <summary>
        /// Get road data from Open Street Maps
        /// </summary>
        /// <returns>yield break. On WebRequest success it generates roads</returns>
        private IEnumerator GetRoadDataAysnc()
        {
            string uri = string.Format("{0}{1}({2},{3},{4},{5});{6}",
                requestPrefix,
                requestParam,
                52.0899704821154, 5.12129160852116, 52.0908725582746, 5.12274540980247,
                //wgsBottomLeft.lat,
                //wgsBottomLeft.lon,
                //wgsTopRight.lat,
                //wgsTopRight.lon,
                requestSuffix);
            Debug.Log("[Simulator] UnityWebRequest: " + uri);
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
                        GenerateRoadsFromData(request.downloadHandler.text);
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

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Draw each road point
            Gizmos.color = Color.blue;
            foreach(var road in roadsData.Roads)
            {
                foreach(var point in road.points)
                {
                    Gizmos.DrawSphere(point.coordinateUnity, 1);
                }
            }
        }
#endif
    }
}
