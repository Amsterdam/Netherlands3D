using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;
using System.IO;
namespace Netherlands3D.ModelParsing
{
    public class LoadOBJ : MonoBehaviour
    {
        //[HideInInspector]
        public string objFilename;
        //[HideInInspector]
        public string mtlFilename;
        public Material defaultMaterial;
        public GameObject createdGameObject;

        [Header("receiving Events")]
        [SerializeField]
        StringEvent objFileSelected;
        [SerializeField]
        StringEvent mtlFileSelected;
        [Header("Sending Events")]
        [SerializeField]
        BoolEvent started;
        [SerializeField]
        StringEvent Errormessage;
        [SerializeField]
        StringEvent progressMessage;
        [SerializeField]
        FloatEvent progressPercentage;
        [SerializeField]
        BoolEvent finishedSuccesfull;


        private void Start()
        {
            if (objFileSelected)
            {
                objFileSelected.started.AddListener(getOBJFileName);
            }
            if (mtlFileSelected)
            {
                mtlFileSelected.started.AddListener(getmtlFileName);
            }
        }
        private void getOBJFileName(string filename)
        {
            objFilename = filename;
        }
        private void getmtlFileName(string filename)
        {
            mtlFilename = filename;
        }


        void sendErrorMessage(string message)
        {
            if (Errormessage != null)
            {
                Errormessage.started.Invoke(message);
            }
        }

        void sendFinishedEvent(bool succesfull)
        {
            if (finishedSuccesfull != null)
            {
                finishedSuccesfull.started.Invoke(succesfull);
            }
        }
        public void LoadModel()
        {
            if (testModel() == false)
            {
                return;
            }
            StartCoroutine(LoadingProcess());

        }

        bool testModel()
        {
            if (!File.Exists(objFilename))
            {
                sendErrorMessage("No obj-file found");
            }
            return true;
        }
        bool testMTLFile()
        {
            if (File.Exists(mtlFilename))
            {
                return true;
               
            }
            return false;
        }

        IEnumerator LoadingProcess()
        {
            bool isBusy = true;
            var objstreamReader = gameObject.AddComponent<StreamreadOBJ>();
            objstreamReader.ReadOBJ(objFilename);
            while (isBusy)
            {
                isBusy = !objstreamReader.isFinished;
                yield return null;
            }
            if(objstreamReader.succes==false)
            {
                sendErrorMessage("couldn't parse the obj-file");
                Destroy(gameObject.GetComponent<StreamreadOBJ>());
                sendFinishedEvent(false);
                yield break;
            }

            if (testMTLFile()==true)
            {
                ReadMTL mtlreader = gameObject.AddComponent<ReadMTL>();
                string mtldata = File.ReadAllText(mtlFilename);
                isBusy = true;
                mtlreader.StartMTLParse(ref mtldata);
                while (isBusy)
                {
                    isBusy = mtlreader.isBusy;
                    yield return null;
                }
                objstreamReader.materialDataSlots = mtlreader.GetMaterialData();
                Destroy(gameObject.GetComponent<ReadMTL>());
            }

            objstreamReader.CreateGameObject(defaultMaterial);
            isBusy = true;
            while (isBusy)
            {
                isBusy = !objstreamReader.isFinished;
                yield return null;
            }
            createdGameObject = objstreamReader.createdGameObject;

            Destroy(gameObject.GetComponent<StreamreadOBJ>());
            sendFinishedEvent(true);

        }

    }
}