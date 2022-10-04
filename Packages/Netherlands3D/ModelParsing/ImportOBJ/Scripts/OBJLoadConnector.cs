using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace Netherlands3D.ModelParsing
{
    [CreateAssetMenu(fileName = "ObjLoadConnector", menuName = "ScriptableObjects/Connectors/ObjLoadConnector", order = 1)]
    public class OBJLoadConnector : ScriptableObject
    {
        public void OnEnable()
        {
           request =  new Request();
           response = new Response();
        }
        public Request request;
        public Response response;
        
       


        [Header("input")]
        public Material baseMaterial;
        public string objFilePath;
        public string mtlFilePath;
        public bool ForceSingleGameobject=false;

        [Header("output")]
        public GameObject createdGameObject;
        public bool isRD;

        [Header("logging")]
        public bool isBusy=false;
        public int numberOfConnections = 0;


       
        public void Clear()
        {
            objFilePath = "";
            mtlFilePath = "";
            ForceSingleGameobject = false;
            createdGameObject = null;
        }
       
        
        
    }
    [System.Serializable]
    public class Request
    {
        public UnityEvent RequestImport;

        public UnityEvent RequestTestConnection;
    }

    [System.Serializable]
    public class Response
    {
        public UnityEvent RespondFinished;

        public UnityEvent RespondErrorOccured;

        public UnityEvent RespondAcknowlegde;

    }
}
