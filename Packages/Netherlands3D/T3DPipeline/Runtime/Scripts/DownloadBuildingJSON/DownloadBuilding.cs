using System;
using System.Collections;
using Netherlands3D.Events;
using UnityEngine;
using UnityEngine.Networking;

namespace Netherlands3D.T3DPipeline
{
    public class DownloadBuilding : MonoBehaviour
    {
        /// <summary>
        /// Subscribe to this event to receive the CityJSON after it was requested
        /// </summary>
        [SerializeField]
        private StringEvent BagIDInput;
        [SerializeField]
        private StringEvent CityJsonBagReceived;
        [SerializeField]
        private string url = @"https://tomcat.totaal3d.nl/happyflow-wfs/wfs?SERVICE=WFS&VERSION=2.0.0&REQUEST=GetFeature&TYPENAMES=bldg:Building&RESOURCEID=NL.IMBAG.Pand.{0}&OUTPUTFORMAT=application%2Fjson";
        private Coroutine requestCoroutine;

        private void OnEnable()
        {
            BagIDInput.started.AddListener(RequestCityJson);
        }

        private void OnDisable()
        {
            BagIDInput.started.RemoveAllListeners();
        }

        /// <summary>
        /// Send the request to receive a CityJSON of a given BAG ID
        /// </summary>
        private void RequestCityJson(string bagId)
        {
            if (requestCoroutine == null)
                requestCoroutine = StartCoroutine(GetCityJsonBag(bagId));
            else
                Debug.Log("Still waiting for coroutine to complete", gameObject);
        }

        public void RequestCityJson(string bagId, Action<string> callback = null)
        {
            if (requestCoroutine == null)
                requestCoroutine = StartCoroutine(GetCityJsonBag(bagId, callback));
            else
                Debug.Log("Still waiting for coroutine to complete", gameObject);
        }

        /// <summary>
        /// request coroutine
        /// </summary>
        private IEnumerator GetCityJsonBag(string bagId, Action<string> callback = null)
        {
            var url = string.Format(this.url, bagId);
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
                    var cityJsonBag = uwr.downloadHandler.text;
                    print(cityJsonBag);
                    CityJsonBagReceived.started.Invoke(cityJsonBag);
                    callback?.Invoke(cityJsonBag);
                }
            }
            requestCoroutine = null;
        }
    }
}