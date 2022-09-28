using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;
using Netherlands3D.ModelParsing;

public class objImportManager : MonoBehaviour
{
    [Header("required input")]
    [SerializeField] Material baseMaterial;
    [SerializeField] TriggerEvent expectOBJFile;
    bool expectingObjFile = false;
    [SerializeField] TriggerEvent expectMTLFile;
    bool expectingMTLFile = false;
    [SerializeField] StringEvent receiveFileToLoad;
    [SerializeField] TriggerEvent startImporting;
    [SerializeField] TriggerEvent cancelImporting;

    [Header("optional output")]
    [SerializeField] BoolEvent ReadyForImport;
    [SerializeField] StringEvent ReceivedOBJFilename;
    [SerializeField] StringEvent ReceivedMTLFilename;
    [SerializeField] GameObjectEvent CreatedMoveableGameObject;
    [SerializeField] GameObjectEvent CreatedImmoveableGameObject;

    [Header("settings")]
    [SerializeField] bool createSubMeshes = false;

    [Header("progress")]
    [SerializeField] BoolEvent started;
    [SerializeField] StringEvent currentActivity;
    [SerializeField] StringEvent currentAction;
    [SerializeField] FloatEvent progressPercentage;

    [Header("error")]
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
            if (ReceivedOBJFilename) ReceivedOBJFilename.started.Invoke(System.IO.Path.GetFileName(objFileName));
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
            if (ReceivedMTLFilename) ReceivedMTLFilename.started.Invoke(System.IO.Path.GetFileName(mtlFileName));
        }
    }

    ObjImporter importer;
    private void Awake()
    {
        startImporting.started.AddListener(OnStartImporting);
        expectOBJFile.started.AddListener(OnExpectingOBJFile);
        expectMTLFile.started.AddListener(OnExpectingMTLFile);
        if (cancelImporting) cancelImporting.started.AddListener(onCancel);
    }

    #region collect input
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
        receiveFileToLoad.started.RemoveListener(OnOBJFileReceived);
        objfilename = value;
        Debug.Log(objfilename + "receiveid");
        if (ReadyForImport) ReadyForImport.started.Invoke(true);

    }

    void OnMTLFileReceived(string value)
    {
        receiveFileToLoad.started.RemoveListener(OnMTLFileReceived);
        mtlfilename = value;
    }
    #endregion


    public void onCancel()
    {
        BroadcastMessage("Cancel");
        if (currentActivity) currentActivity.started.Invoke("cancelling the import");
    }

    void OnStartImporting()
    {
        if (!importer) ConnectToImporter();
        importer.objFilePath = objfilename;
        importer.mtlFilePath = mtlfilename;
        importer.BaseMaterial = baseMaterial;
        importer.createSubMeshes = createSubMeshes;
        if(started)started.started.Invoke(true);
        importer.StartImporting(OnOBJImported);
    }

    void OnOBJImported(GameObject returnedGameObject)
    {
        bool canBemoved = importer.createdGameobjectIsMoveable;

        if (started) started.started.Invoke(false);
        if (canBemoved)
        {
            if (CreatedMoveableGameObject) CreatedMoveableGameObject.started.Invoke(returnedGameObject);
        }
        else
        {
            if (CreatedImmoveableGameObject) CreatedImmoveableGameObject.started.Invoke(returnedGameObject);
        }
        
        objfilename = "";
        mtlfilename = "";
    }

    void ConnectToImporter()
    {
        if (importer!=null) return;
        importer = gameObject.AddComponent<ObjImporter>();
        // give the importer handles for progress- and errormessaging
        importer.currentActivity = BroadcastCurrentActivity;
        importer.currentAction = BroadcastCurrentAction;
        importer.progressPercentage = BroadcastProgressPercentage;
        importer.alertmessage = BroadcastAlertmessage;
        importer.errormessage = BroadcastErrormessage;

    }
    void BroadcastCurrentActivity(string value)
    {
        if (currentActivity != null) currentActivity.started.Invoke(value);
    }
    void BroadcastCurrentAction(string value)
    {
        if (currentAction != null) currentAction.started.Invoke(value);
    }

    void BroadcastProgressPercentage(float value)
    {
        if (progressPercentage != null) progressPercentage.started.Invoke(value);
    }

    void BroadcastAlertmessage(string value)
    {
        if (alertmessage != null) alertmessage.started.Invoke(value);
    }
    void BroadcastErrormessage(string value)
    {
        if (errormessage != null) errormessage.started.Invoke(value);
    }


}
