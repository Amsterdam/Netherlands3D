using System;
using System.Collections;
using Netherlands3D.Events;
using UnityEngine;
using UnityEngine.Networking;

namespace Netherlands3D.T3DPipeline
{
    public class UploadBuilding : MonoBehaviour
    {
        private Coroutine requestCoroutine;

        [Tooltip("URL to upload to")]
        [SerializeField]
        private string url = @"https://voorportaal.azurewebsites.net/api/uploadcityjson";
        [Tooltip("Username used to submit the request")]
        [SerializeField]
        private string userName;
        [Tooltip("Bag ID of the submission")]
        [SerializeField]
        private string bagId;
        [Tooltip("Authentication token for the request")]
        [SerializeField]
        private string submitToken;
        [Tooltip("Event that is called when the submission is completed. Returns true if upload is successful, and false if the upload failed")]
        [SerializeField]
        private BoolEvent uploadToEndpointSucceded;

        [Tooltip("Listening for the event when user name required for a submission is changed")]
        [SerializeField]
        private StringEvent onUserNameChanged;
        [Tooltip("Listening for the event when the BagID for a submission is changed")]
        [SerializeField]
        private StringEvent onBagIdChanged;

        private void OnEnable()
        {
            if (onUserNameChanged)
                onUserNameChanged.AddListenerStarted(SetUserName);
            if (onBagIdChanged)
                onBagIdChanged.AddListenerStarted(SetBagId);
        }

        private void OnDisable()
        {
            if (onUserNameChanged)
                onUserNameChanged.RemoveAllListenersStarted();
            if (onBagIdChanged)
                onBagIdChanged.RemoveAllListenersStarted();
        }

        public void SetUserName(string newName)
        {
            userName = newName;
        }

        public void SetBagId(string newId)
        {
            bagId = newId;
        }

        public void UploadCityJSONFileToEndpoint()
        {
            string data = CityJSONFormatter.GetCityJSON();
            print("uploading" + data);

            if (string.IsNullOrEmpty(submitToken))
                throw new Exception("no submission authentication token provided");

            if (requestCoroutine == null)
            {
                if (string.IsNullOrEmpty(bagId))
                    throw new Exception("Bag ID cannot be empty");

                requestCoroutine = StartCoroutine(UploadDataToEndpoint(data));
            }
            else
            {
                print("Still waiting for coroutine to return, not sending data");
                uploadToEndpointSucceded?.InvokeStarted(false);
            }
        }

        private IEnumerator UploadDataToEndpoint(string jsonData)
        {
            var token = "Bearer " + submitToken;

            var uwr = UnityWebRequest.Put(url, jsonData);
            uwr.SetRequestHeader("Content-Type", "application/json");
            uwr.SetRequestHeader("objectId", bagId);
            uwr.SetRequestHeader("initiatorPersoon", userName);
            uwr.SetRequestHeader("Authorization", token);

            using (uwr)
            {
                yield return uwr.SendWebRequest();
                if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(uwr.error);
                    uploadToEndpointSucceded?.InvokeStarted(false);
                }
                else
                {
                    print("Upload succeeded");
                    uploadToEndpointSucceded?.InvokeStarted(true);
                }
                requestCoroutine = null;
            }
        }
    }
}