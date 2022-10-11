using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Netherlands3D.T3DPipeline
{
    public class DownloadBuilding : MonoBehaviour
    {
        /// <summary>
        /// Saved BAG ID to reference after requesting
        /// </summary>
        public string BagId { get; private set; }
        /// <summary>
        /// Saved building CityJSON to reference after requesting
        /// </summary>
        public string CityJsonBag { get; private set; }

        public delegate void CityJsonBagEventHandler(string cityJson);
        /// <summary>
        /// Subscribe to this event to receive the CityJSON after it was requested
        /// </summary>
        public event CityJsonBagEventHandler CityJsonBagReceived;

        /// <summary>
        /// Send the request to receive a CityJSON of a given BAG ID
        /// </summary>
        public void RequestCityJson(string bagId)
        {
            BagId = bagId;
            StartCoroutine(GetCityJsonBag(bagId));
        }

        /// <summary>
        /// request coroutine
        /// </summary>
        private IEnumerator GetCityJsonBag(string bagId)
        {
            var url = $"https://tomcat.totaal3d.nl/happyflow-wfs/wfs?SERVICE=WFS&VERSION=2.0.0&REQUEST=GetFeature&TYPENAMES=bldg:Building&RESOURCEID=NL.IMBAG.Pand.{bagId}&OUTPUTFORMAT=application%2Fjson";
            var uwr = UnityWebRequest.Get(url);

            using (uwr)
            {
                yield return uwr.SendWebRequest();
                if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(uwr.error);
                }
                else
                {
                    CityJsonBag = uwr.downloadHandler.text;
                    CityJsonBagReceived?.Invoke(CityJsonBag);
                }
            }
        }
    }
}