using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;
using Netherlands3D.ModelParsing;

public class ObjImportManager : MonoBehaviour
{
    [Header("Required input")]
    [SerializeField] Material baseMaterial;
    bool expectingObjFile = false;
    bool expectingMTLFile = false;
    bool expectingImageFile = false;
    [SerializeField] StringEvent receiveFileToLoad;
    [SerializeField] TriggerEvent cancelImporting;

    [Header("Optional triggers")]
    [SerializeField] TriggerEvent expectOBJFile;
    [SerializeField] TriggerEvent expectMTLFile;
    [SerializeField] TriggerEvent expectImageFile;
    [SerializeField, Tooltip("Use this trigger in combination with expectOBJFile and expectMTLFile to import OBJ and MTL files in specific steps")] TriggerEvent startImporting;

    [Header("Optional output")]
    [SerializeField] BoolEvent ReadyForImport;
    [SerializeField] StringEvent ReceivedOBJFilename;
    [SerializeField] StringEvent ReceivedMTLFilename;

    // We support one image right now.
    [SerializeField] StringEvent ReceivedImageFilename;

    [SerializeField] GameObjectEvent CreatedMoveableGameObject;
    [SerializeField] GameObjectEvent CreatedImmoveableGameObject;

    [Header("Settings")]
    [SerializeField] bool createSubMeshes = false;

    [Header("Progress")]
    [SerializeField] BoolEvent started;
    [SerializeField] StringEvent currentActivity;
    [SerializeField] StringEvent currentAction;
    [SerializeField] FloatEvent progressPercentage;

    [Header("Alerts and errors")]
    [SerializeField] StringEvent alertmessage;
    [SerializeField] StringEvent errormessage;

    string objFileName = "";
    string objfilename
    {
        get
        {
            return objFileName;
        }
        set
        {
            objFileName = value;

            Debug.Log("received objFile: " + objFileName);
            if (ReceivedOBJFilename) ReceivedOBJFilename.Invoke(System.IO.Path.GetFileName(objFileName));
        }
    }

    string mtlFileName = "";
    string mtlfilename
    {
        get
        {
            return mtlFileName;
        }
        set
        {
            mtlFileName = value;
#if UNITY_WEBGL && !UNITY_EDITOR
        if(mtlFileName!="")
            {
            mtlFileName = System.IO.Path.Combine(Application.persistentDataPath, mtlFileName);
            }
#endif
            if (ReceivedMTLFilename) ReceivedMTLFilename.Invoke(System.IO.Path.GetFileName(mtlFileName));
        }
    }



    string imgFileName = "";
    string imgfilename
    {
        get
        {
            return imgFileName;
        }
        set
        {
            imgFileName = value;

            if (ReceivedImageFilename) ReceivedImageFilename.Invoke(System.IO.Path.GetFileName(imgFileName));
        }
    }



    ObjImporter importer;
    private void Awake()
    {
        if (startImporting) startImporting.AddListenerStarted(OnStartImporting);
        if (expectOBJFile)
        {
            expectOBJFile.AddListenerStarted(OnExpectingOBJFile);
        }
        else
        {
            expectingObjFile = true;
            receiveFileToLoad.AddListenerStarted(OnFileNamesReceived);
        }
        if (expectMTLFile)
        {
            expectMTLFile.AddListenerStarted(OnExpectingMTLFile);
        }
        else
        {
            expectingMTLFile = true;
        }

        if (expectImageFile == true)
        {
            expectImageFile.AddListenerStarted(OnExpectingImageFile);
        }
        else
        {
            expectingImageFile = true;
        }



        if (cancelImporting) cancelImporting.AddListenerStarted(OnCancel);
        if (true)
        {

        }
    }

    #region collect input
    void OnExpectingOBJFile()
    {
        if (expectingMTLFile)
        {
            receiveFileToLoad.RemoveListenerStarted(OnMTLFileReceived);
        }
        receiveFileToLoad.AddListenerStarted(OnOBJFileReceived);
        expectingObjFile = true;
    }

    void OnExpectingMTLFile()
    {
        if (expectingObjFile)
        {
            receiveFileToLoad.RemoveListenerStarted(OnOBJFileReceived);
        }
        receiveFileToLoad.AddListenerStarted(OnMTLFileReceived);
    }


    void OnExpectingImageFile()
    {
        if (expectingImageFile == true)
        {
            receiveFileToLoad.RemoveListenerStarted(OnImageFileReceived);
        }

        receiveFileToLoad.AddListenerStarted(OnImageFileReceived);
        expectingImageFile = true;
    }

    void OnFileNamesReceived(string value)
    {
        Debug.Log("receiveid files: " + value);

        if (value == "")//empty string received, so no selection was made
        {
            return;
        }
        string[] filenames = value.Split(',');

        foreach (var file in filenames)
        {
            string fileextention = System.IO.Path.GetExtension(file).ToLower();
            switch (fileextention)
            {
                case ".obj":
                    objfilename = System.IO.Path.Combine(Application.persistentDataPath, file);
                    break;

                case ".mtl":
                    mtlfilename = System.IO.Path.Combine(Application.persistentDataPath, file);
                    break;

                case ".jpg":
                case ".png":
                case ".jpeg":
                    imgfilename = System.IO.Path.Combine(Application.persistentDataPath, file);
                    break;
            }
        }

        if (objfilename != "")
        {
            if (startImporting == null)
            {
                OnStartImporting();
            }
            else
            {
                if (ReadyForImport) ReadyForImport.Invoke(true);
            }
        }
    }

    void OnOBJFileReceived(string value)
    {
        receiveFileToLoad.RemoveListenerStarted(OnOBJFileReceived);
        objfilename = value;
        if (ReadyForImport) ReadyForImport.Invoke(true);

    }

    void OnMTLFileReceived(string value)
    {
        receiveFileToLoad.RemoveListenerStarted(OnMTLFileReceived);
        mtlfilename = value;
    }

    void OnImageFileReceived(string value)
    {
        receiveFileToLoad.RemoveListenerStarted(OnImageFileReceived);
        imgfilename = value;
    }
    #endregion


    public void OnCancel()
    {
        BroadcastMessage("Cancel");
        if (currentActivity) currentActivity.Invoke("cancelling the import");
    }

    void OnStartImporting()
    {
        ConnectToImporter();

        importer.objFilePath = objfilename;
        importer.mtlFilePath = mtlfilename;
        importer.imgFilePath = imgfilename;

        importer.BaseMaterial = baseMaterial;
        importer.createSubMeshes = createSubMeshes;
        if (started) started.Invoke(true);
        importer.StartImporting(OnOBJImported);
    }

    void OnOBJImported(GameObject returnedGameObject)
    {
        bool canBemoved = importer.createdGameobjectIsMoveable;

        if (started) started.Invoke(false);
        if (canBemoved)
        {
            if (CreatedMoveableGameObject) CreatedMoveableGameObject.Invoke(returnedGameObject);
        }
        else
        {
            if (CreatedImmoveableGameObject) CreatedImmoveableGameObject.Invoke(returnedGameObject);
        }

        objfilename = string.Empty;
        mtlfilename = string.Empty;
        imgfilename = string.Empty;

        if (importer != null) Destroy(importer);
    }

    void ConnectToImporter()
    {
        if (importer != null) Destroy(importer);

        importer = gameObject.AddComponent<ObjImporter>();
        // give the importer handles for progress- and errormessaging
        importer.currentActivity = BroadcastCurrentActivity;
        importer.currentAction = BroadcastCurrentAction;
        importer.progressPercentage = BroadcastProgressPercentage;
        importer.alertmessage = BroadcastAlertmessage;
        importer.errormessage = BroadcastErrormessage;

        Debug.Log("Connected to new ObjImporter");
    }
    void BroadcastCurrentActivity(string value)
    {
        if (currentActivity != null) currentActivity.Invoke(value);
    }
    void BroadcastCurrentAction(string value)
    {
        if (currentAction != null) currentAction.Invoke(value);
    }

    void BroadcastProgressPercentage(float value)
    {
        if (progressPercentage != null) progressPercentage.Invoke(value);
    }
    void BroadcastAlertmessage(string value)
    {
        if (alertmessage != null) alertmessage.Invoke(value);
    }
    void BroadcastErrormessage(string value)
    {
        if (errormessage != null) errormessage.Invoke(value);
    }


}
