using System;
using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Events;
using UnityEngine;
using UnityEngine.Networking;

namespace Netherlands3D.Core
{
    public class WebRequest : MonoBehaviour
    {
        private static GameObject m_CoroutineObjectsParent;
        private static GameObject coroutineObjectsParent
        {
            get
            {
                if (!m_CoroutineObjectsParent)
                    m_CoroutineObjectsParent = new GameObject("Coroutine Objects");

                return m_CoroutineObjectsParent;
            }
        }

        public Coroutine Coroutine { get; private set; }
        public bool IsActive { get; private set; } = false;
        public bool HasFinished { get; private set; } = false;
        public bool IsSuccess { get; private set; } = false;


        public static WebRequest CreateWebRequest(string url, Action<string> callback, bool destroyObjectOnCompletion = true)
        {
            var coroutineObject = new GameObject(url);
            coroutineObject.transform.SetParent(coroutineObjectsParent.transform);
            var req = coroutineObject.AddComponent<WebRequest>();
            req.Coroutine = req.StartCoroutine(req.GetWebString(url, callback, destroyObjectOnCompletion));

            return req;
        }

        public static WebRequest CreateWebRequest(string url, StringEvent invokeEvent, bool destroyObjectOnCompletion = true)
        {
            var coroutineObject = new GameObject(url);
            coroutineObject.transform.SetParent(coroutineObjectsParent.transform);
            var req = coroutineObject.AddComponent<WebRequest>();
            req.Coroutine = req.StartCoroutine(req.GetWebString(url, invokeEvent, destroyObjectOnCompletion));

            return req;
        }

        public static WebRequest CreateWebRequest(string url, Action<byte[]> callback, bool destroyObjectOnCompletion = true)
        {
            var coroutineObject = new GameObject(url);
            coroutineObject.transform.SetParent(coroutineObjectsParent.transform);
            var req = coroutineObject.AddComponent<WebRequest>();
            req.Coroutine = req.StartCoroutine(req.GetWebByteArray(url, callback, destroyObjectOnCompletion));

            return req;
        }

        private IEnumerator GetWebString(string url, Action<string> action, bool destroyObjectOnCompletion)
        {
            IsActive = true;

            UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                var result = request.downloadHandler.text;
                action.Invoke(result);
                IsSuccess = true;
            }
            else
            {
                Debug.LogError(request.error);
                IsSuccess = false;
            }
            HasFinished = true;
            IsActive = false;

            if (destroyObjectOnCompletion)
            {
                print("destroying coroutineObject " + gameObject.name);
                Destroy(gameObject);
            }
        }

        private IEnumerator GetWebString(string url, StringEvent invokeEvent, bool destroyObjectOnCompletion)
        {
            IsActive = true;

            UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                var result = request.downloadHandler.text;
                invokeEvent.InvokeStarted(result);
                IsSuccess = true;
            }
            else
            {
                Debug.LogError(request.error);
                invokeEvent.InvokeCancelled(); //todo: is this the right way to use this?
                IsSuccess = false;
            }
            HasFinished = true;
            IsActive = false;

            if (destroyObjectOnCompletion)
                Destroy(gameObject);
        }

        private IEnumerator GetWebByteArray(string url, Action<byte[]> action, bool destroyObjectOnCompletion)
        {
            IsActive = true;

            UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                var result = request.downloadHandler.data;
                action.Invoke(result);
                IsSuccess = true;
            }
            else
            {
                Debug.LogError(request.error);
                IsSuccess = false;
            }
            HasFinished = true;
            IsActive = false;

            if (destroyObjectOnCompletion)
                Destroy(gameObject);
        }
    }
}
