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
    [SerializeField] StringEvent receiveFileToLoad;
    [SerializeField] TriggerEvent cancelImporting;

    [Header("Optional triggers")]
    [SerializeField] TriggerEvent expectOBJFile;
    [SerializeField] TriggerEvent expectMTLFile;
    [SerializeField, Tooltip("Use this trigger in combination with expectOBJFile and expectMTLFile to import OBJ and MTL files in specific steps")] TriggerEvent startImporting;

    [Header("Optional output")]
    [SerializeField] BoolEvent ReadyForImport;
    [SerializeField] StringEvent ReceivedOBJFilename;
    [SerializeField] StringEvent ReceivedMTLFilename;
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

#if UNITY_WEBGL && !UNITY_EDITOR
        if (objFileName!="")
	        {
            objFileName = System.IO.Path.Combine(Application.persistentDataPath, objFileName);
	        }
        
#endif

            Debug.Log("received objFile: " + objFileName);
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
#if UNITY_WEBGL && !UNITY_EDITOR
        if(mtlFileName!="")
            {
            mtlFileName = System.IO.Path.Combine(Application.persistentDataPath, mtlFileName);
            }
#endif
            if (ReceivedMTLFilename) ReceivedMTLFilename.started.Invoke(System.IO.Path.GetFileName(mtlFileName));
        }
    }

    ObjImporter importer;
    private void Awake()
    {
        if(startImporting) startImporting.started.AddListener(OnStartImporting);
        if (expectOBJFile)
        {
            expectOBJFile.started.AddListener(OnExpectingOBJFile);
        }
        else
        {
            expectingObjFile = true;
            receiveFileToLoad.started.AddListener(OnFileneamesReceived);
        }
        if (expectMTLFile)
        {
            expectMTLFile.started.AddListener(OnExpectingMTLFile);
        }
        else
        {
            expectingMTLFile = true;
        }
        if (cancelImporting) cancelImporting.started.AddListener(OnCancel);
        if (true)
        {

        }
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

    void OnFileneamesReceived(string value)
    {
        Debug.Log("receiveid files: " + value);

        if (value=="")//empty string received, so no selection was made
        {
            return;
        }
        string[] filenames = value.Split(',');
        
        foreach (var file in filenames)
        {
            string fileextention = System.IO.Path.GetExtension(file);
            if (fileextention == ".obj")
            {
                objfilename = System.IO.Path.Combine(Application.persistentDataPath, file);
            }
            else if (fileextention == ".mtl")
            {
                mtlfilename = System.IO.Path.Combine(Application.persistentDataPath, file);
            }
        }
        if (objfilename!="")
        {
            if(startImporting==null)
            {
                OnStartImporting();
            }
            else
            {
                if (ReadyForImport) ReadyForImport.started.Invoke(true);
            }
        }

    }

    void OnOBJFileReceived(string value)
    {
        receiveFileToLoad.started.RemoveListener(OnOBJFileReceived);
        objfilename = value;
        if (ReadyForImport) ReadyForImport.started.Invoke(true);

    }

    void OnMTLFileReceived(string value)
    {
        receiveFileToLoad.started.RemoveListener(OnMTLFileReceived);
        mtlfilename = value;
    }
#endregion


    public void OnCancel()
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
