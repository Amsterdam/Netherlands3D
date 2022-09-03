using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Netherlands3D.ModelParsing
{
    public class objImporter : MonoBehaviour
    {
        StreamreadOBJ objreader;
        ReadMTL mtlreader;
        CreateGameObjectDataSetFromOBJ objectDataCreator;
        CreateGameObjects cgo;
        public GameObjectDataSet gameObjectData;
        [HideInInspector]
        public Material BaseMaterial;
        [HideInInspector]
        public string objfilename = "";
        
        [HideInInspector]
        public string mtlfilename = "";

        bool isbusy = false;
        [HideInInspector]
        public bool createSubMeshes = false;

        private bool needToCancel = false;

        public System.Action<string> currentActivity;
        public System.Action<string> currentAction;
        public System.Action<float> progressPercentage;

        public System.Action<string> alertmessage;
        public System.Action<string> errormessage;

        public System.Action<GameObject> returnObjectTo;

        #region send progress and errors
        void BroadcastCurrentActivity(string value)
        {
            if (currentActivity!=null) currentActivity(value);
        }
        void BroadcastCurrentAction(string value)
        {
            if (currentAction!=null) currentAction(value);
        }
       
        void BroadcastProgressPercentage(float value)
        {
            if (progressPercentage!=null) progressPercentage(value);
        }

        void BroadcastAlertmessage(string value)
        {
            if (alertmessage!=null) alertmessage(value);
        }
        void BroadcastErrormessage(string value)
        {
            if (errormessage!=null) errormessage(value);
        }


        #endregion
        //reading obj-file
        public void StartImporting(System.Action<GameObject> returnResultTo)
        {
            needToCancel = false;
            returnObjectTo = returnResultTo;
            if (isbusy)
            {
                return;
            }
            if (objreader is null)
            {
                objreader = gameObject.AddComponent<StreamreadOBJ>();
                objreader.broadcastProgressPercentage = BroadcastProgressPercentage;
                objreader.BroadcastErrorMessage = BroadcastErrormessage;

            }
            BroadcastCurrentActivity("obj-bestand inlezen");

            objreader.ReadOBJ(objfilename, OnOBJRead);
        }

        public void Cancel()
        {
            needToCancel = true;
            Debug.Log("cancelling StreamReadOBJ");
        }



        void OnOBJRead(bool succes)
        {
            if (!succes) //something went wrong
            {
                isbusy = false;
                objfilename = "";
                mtlfilename = "";
                returnObjectTo(null);
                return;
            }
            objfilename = "";
            if (mtlfilename!="")
            {
                if (mtlreader is null)
                {
                    mtlreader = gameObject.AddComponent<ReadMTL>();
                    mtlreader.broadcastProgressPercentage = BroadcastProgressPercentage;
                }
                BroadcastCurrentActivity("mtl-file lezen");
                mtlreader.StartMTLParse(System.IO.File.ReadAllText(mtlfilename),onMTLRead);
            }
            else
            {
                BroadcastAlertmessage("geen mtl-file, alle objecten krijgen de default-kleur");
                CreateGameObjectDataSet();
            }
        }

        void onMTLRead(bool succes)
        {
            if (!succes)
            {
                mtlreader = null;
            }
            System.IO.File.Delete(mtlfilename);
            mtlfilename = "";
            if (needToCancel)
            {
                Debug.Log("cancelled while reading materialFile");
                returnObjectTo(null);

                return;
            }
            CreateGameObjectDataSet();
        }


        //create the geometry
        void CreateGameObjectDataSet()
        {
            BroadcastCurrentActivity("geometrie samenstellen");

            if (objectDataCreator is null)
            {
                objectDataCreator = gameObject.AddComponent<CreateGameObjectDataSetFromOBJ>();
                objectDataCreator.broadcastProgressPercentage = BroadcastProgressPercentage;
                objectDataCreator.broadcastCurrentAction = BroadcastCurrentAction;
            }
            objectDataCreator.vertices = objreader.vertices;
            objectDataCreator.normals = objreader.normals;
            objectDataCreator.broadcastProgressPercentage = progressPercentage;
            
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
            if (needToCancel)
            {
                Debug.Log("cancelled while creating GameObjectDataSet");
                
                returnObjectTo(null);
                return;
            }
            if (gameObjectData is null)
            {
                //is een error
            }
            // add the materials to the dataset
            if (mtlreader != null)
            {
                gameObjectData.materials = mtlreader.GetMaterialData();
            }
            ////// just print the ouptut to json to look at it.
            //string json = JsonUtility.ToJson(gameObjectData, true);
            //var sr = System.IO.File.CreateText("D:/gebouwenoutput.json");
            //sr.Write(json);
            //sr.Close();

            CreateTheGameObject();
        }


        void CreateTheGameObject()
        {
            BroadcastCurrentActivity("objecten creeren");
            
            cgo = FindObjectOfType<CreateGameObjects>();
            if (cgo is null)
            {
                cgo = gameObject.AddComponent<CreateGameObjects>();
                cgo.BroadcastProgressPercentage = BroadcastProgressPercentage;
            }

            cgo.Create(gameObjectData,BaseMaterial, OnGameObjectCreated);
            
        }
        
        void OnGameObjectCreated(GameObject gameObject)
        {
            returnObjectTo(gameObject);
            

        }


    }
}
