using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;

namespace Netherlands3D.ModelParsing
{
    public class objImporter : MonoBehaviour
    {
        StreamreadOBJ objreader;
        ReadMTL mtlreader;
        CreateMeshesFromOBJ objectDataCreator;

        CreateGameobjectFromOBJ modelcreator;
        CreateGameObjects cgo;
        GameObjectDataSet gameObjectData;

        string objfilename="";
        string mtlfilename="";
        bool isbusy;
        [Header("input WEBGL")]
        [SerializeField] TriggerEvent expectOBJFile;
        bool expectingObjFile=false;
        [SerializeField] TriggerEvent expectMTLFile;
        bool expectingMTLFile = false;
        [SerializeField] StringEvent receiveFileToLoad;
        [Header ("input StandAlone")]
        [SerializeField] StringEvent receiveOBJFileDirect;
        [SerializeField] StringEvent receiveMTLFileDirect;

        [SerializeField] TriggerEvent startImporting;
        [SerializeField] BoolEvent ReadyForImport;

        [Header("settings")]
        [SerializeField] BoolEvent createSubmeshes;
        bool createSubMeshes = false;

        [Header("progress")]
        [SerializeField] BoolEvent started;
        [SerializeField] StringEvent currentActivity;
        [SerializeField] FloatEvent progressPercentage;

        [Header("error")]
        [SerializeField] StringEvent errormessage;

        [Header("result")]
        [SerializeField] GameObjectEvent createdGameObject;
        private void Awake()
        {
            isbusy = false;
            if (startImporting) startImporting.started.AddListener(OnStartImporting);
            expectOBJFile.started.AddListener(OnExpectingOBJFile);
            expectMTLFile.started.AddListener(OnExpectingMTLFile);
            if (receiveOBJFileDirect) receiveOBJFileDirect.started.AddListener(OnOBJFileReceived);
            if (receiveMTLFileDirect) receiveMTLFileDirect.started.AddListener(OnMTLFileReceived);

            
        }

        private void Start()
        {
            if (ReadyForImport) ReadyForImport.started.Invoke(false);
        }

        void OnExpectingOBJFile()
        {
            if (expectingMTLFile)
            {
                receiveFileToLoad.started.RemoveListener(OnMTLFileReceived);
                
            }
            receiveFileToLoad.started.AddListener(OnOBJFileReceived);
            expectingObjFile = true;
        }
        void OnExpectingMTLFile()
        {
            if (expectingObjFile)
            {
                receiveFileToLoad.started.RemoveListener(OnOBJFileReceived);
            }
            receiveFileToLoad.started.AddListener(OnMTLFileReceived);
        }


        void OnOBJFileReceived(string value)
        {
            objfilename = value;
            
#if UNITY_WEBGL && !UNITY_EDITOR
            objfilename = System.IO.Path.Combine(Application.persistentDataPath, value);
#endif
            Debug.Log(objfilename + "receiveid");
            if (ReadyForImport) ReadyForImport.started.Invoke(true);
        }

        void OnMTLFileReceived(string value)
        {
            mtlfilename = value;
#if UNITY_WEBGL && !UNITY_EDITOR
            objfilename = System.IO.Path.Combine(Application.persistentDataPath, value);
#endif
        }


        //reading obj-file
        void OnStartImporting()
        {

            Debug.Log("started importing obj");

            if (isbusy)
            {
                return;
            }
            isbusy = true;
            if (started) started.started.Invoke(true);

            if (objreader is null)
            {
                objreader = gameObject.AddComponent<StreamreadOBJ>();
            }
            if (currentActivity != null)
            {
                currentActivity.started.Invoke("obj-bestand importeren");
            }
            objreader.progressPercentage = progressPercentage;
            objreader.ReadOBJ(objfilename, OnOBJRead);
        }

        void OnOBJRead(bool succes)
        {
            if (!succes) //something went wrong
            {
                isbusy = false;
                if (started) started.started.Invoke(false);
                if (errormessage) errormessage.started.Invoke("could not read the obj-file");
                return;
            }
            objfilename = "";
            if (mtlfilename!="")
            {
                if (mtlreader is null)
                {
                    mtlreader = gameObject.AddComponent<ReadMTL>();
                }
                mtlreader.StartMTLParse(System.IO.File.ReadAllText(mtlfilename),onMTLRead);
            }
            else
            {
                CreateGameObjectDataSet();
            }
        }

        void onMTLRead(bool succes)
        {
            if (!succes)
            {
                isbusy = false;
                if (started) started.started.Invoke(false);
                if (errormessage) errormessage.started.Invoke("could not read the mtl-file");
                return;
            }
            mtlfilename = "";
            CreateGameObjectDataSet();
        }


        //create the geometry
        void CreateGameObjectDataSet()
        {
            if (objectDataCreator is null)
            {
                objectDataCreator = gameObject.AddComponent<CreateMeshesFromOBJ>();
            }
            objectDataCreator.vertices = objreader.vertices;
            objectDataCreator.normals = objreader.normals;
            objectDataCreator.progressPercentage = progressPercentage;
            if (currentActivity!=null)
            {
                currentActivity.started.Invoke("geometrie analyseren");
            }
            List<Submesh> submeshes = new List<Submesh>();
            foreach (KeyValuePair<string,Submesh> kvp in objreader.submeshes)
            {
                submeshes.Add(kvp.Value);
            }
            objectDataCreator.submeshes = submeshes;
            objectDataCreator.CreateGameObjectDataSet(OnGameObjectDataSetCreated,!createSubMeshes);

            

           
        }

        void OnGameObjectDataSetCreated(GameObjectDataSet gods)
        {
            gameObjectData = gods;
            // add the materials to the dataset
            if (mtlreader != null)
            {
                gameObjectData.materials = mtlreader.GetMaterialData();
            }
            //// just print the ouptut to json to look at it.
            string json = JsonUtility.ToJson(gameObjectData, true);
            var sr = System.IO.File.CreateText("D:/gebouwenoutput.json");
            sr.Write(json);
            sr.Close();

            CreateTheGameObject();
        }


        void CreateTheGameObject()
        {
            if(currentActivity)currentActivity.started.Invoke("geometrie maken");
            cgo = FindObjectOfType<CreateGameObjects>();
            if (cgo is null)
            {
                return;
            }
            cgo.gameObjectData = gameObjectData;
            cgo.createdGameObject.started.AddListener(OnGameObjectCreated);
            cgo.progressPercentage = progressPercentage;
            cgo.Create(OnGameObjectCreated);
            
        }
        
        void OnGameObjectCreated(GameObject gameObject)
        {
            //cgo.createdGameObject.started.RemoveListener(OnGameObjectCreated);
            if (started) started.started.Invoke(false);
        }


    }
}
