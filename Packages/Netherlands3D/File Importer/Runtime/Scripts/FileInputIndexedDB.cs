/* Copyright(C)  X Gemeente
                 X Amsterdam
                 X Economic Services Departments
Licensed under the EUPL, Version 1.2 or later (the "License");
You may not use this work except in compliance with the License. You may obtain a copy of the License at:
https://joinup.ec.europa.eu/software/page/eupl
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" basis, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
implied. See the License for the specific language governing permissions and limitations under the License.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using UnityEngine.UI;
using System.Runtime.InteropServices;
//using Netherlands3D.ModelParsing;
//using Netherlands3D.Traffic.VISSIM;
//using Netherlands3D.Interface;
using Netherlands3D.Events;
#if UNITY_STANDALONE || UNITY_EDITOR
using Netherlands3D.FileImporter.SFB;
#endif


/// <summary>
/// This system handles the user file uploads.
/// They are moved into the IndexedDB so they can be streamread from Unity.
/// This avoids having to load the (large) amount of data in the Unity heap memory
/// </summary>
public class FileInputIndexedDB : MonoBehaviour
{

    #region Webgl config
    [DllImport("__Internal")]
    private static extern void InitializeIndexedDB(string dataPath);
    [DllImport("__Internal")]
    private static extern void SyncFilesFromIndexedDB();
    [DllImport("__Internal")]
    private static extern void SyncFilesToIndexedDB();
    [DllImport("__Internal")]
    private static extern void ClearFileInputFields();
    #endregion

    private List<string> filenames = new List<string>();
    private int numberOfFilesToLoad = 0;
    private int fileCount = 0;

    [SerializeField]
    private StringEvent filesImportedEvent;

    private BoolEvent receivingFiles;

    [SerializeField]
    //private BoolEvent clearDataBaseEvent;

	private void Awake()
	{
#if !UNITY_EDITOR && UNITY_WEBGL
        InitializeIndexedDB(Application.persistentDataPath);
#endif
    }

    #region general functions
    void ProcessAllFiles()
    {
        //LoadingScreen.Instance.Hide();
        if (receivingFiles) receivingFiles.started.Invoke(false);
        var files = string.Join(",", filenames);
        filenames.Clear();
        filesImportedEvent.started.Invoke(files);
    }

    public void SelectFiles(string fileExtention, bool multiselect)
    {
#if UNITY_EDITOR
        StartCoroutine(loadInUnityEditor(fileExtention, multiselect));
#elif UNITY_STANDALONE
StartCoroutine(LoadINUnityStandalone(fileExtention, multiselect));
#endif
    }
    #endregion
#if UNITY_WEBGL && !UNITY_EDITOR
    #region functions for webgl
    // Called from javascript, the total number of files that are being loaded.
    public void FileCount(int count)
    {
        numberOfFilesToLoad = count;
        fileCount = 0;
        filenames = new List<string>();
        //Debug.Log("expecting " + count + " files");
        if (receivingFiles) receivingFiles.started.Invoke(true);
        StartCoroutine(WaitForFilesToBeLoaded());
    }
    
    //called from javascript
    public void LoadFile(string filename)
    {
        filenames.Add(System.IO.Path.Combine(Application.persistentDataPath, filename));
        fileCount++;
        //Debug.Log("received: "+filename);        
    }

    // called from javascript
    public void LoadFileError(string name)
    {
        fileCount++;
        //LoadingScreen.Instance.Hide();
        //Debug.Log("unable to load " + name);
    }

    // runs while javascript is busy saving files to indexedDB.
    IEnumerator WaitForFilesToBeLoaded()
    {
        while (fileCount<numberOfFilesToLoad)
        {
            yield return null;
        }
        numberOfFilesToLoad = 0;
        fileCount = 0;
        ProcessFiles();
    }

    public void ProcessFiles()
    {
        // start js-function to update the contents of application.persistentdatapath to match the contents of indexedDB.
        SyncFilesFromIndexedDB();
    }

    public void IndexedDBUpdated() // called from SyncFilesFromIndexedDB
    {
        ProcessAllFiles();
    }

    void ClearDatabase(bool succes)
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        ClearFileInputFields();
        filenames.Clear();
        if (succes)
        {
            SyncFilesToIndexedDB();
        }
#endif
    }

    #endregion
#endif

#if UNITY_EDITOR
    #region functions for unity Editor
    IEnumerator loadInUnityEditor(string fileExtention, bool multiselect)
    {
        yield return null;
        string filePath = UnityEditor.EditorUtility.OpenFilePanel("Select File", "", fileExtention);
        if (receivingFiles) receivingFiles.started.Invoke(true);
        string baseFilename = System.IO.Path.GetFileName(filePath);
        string newFilename = System.IO.Path.Combine(Application.persistentDataPath, baseFilename);
        System.IO.File.Copy(filePath, newFilename,true);
        filenames.Add(newFilename);
        ProcessAllFiles();

        
        
    }

    #endregion
#endif
#if UNITY_STANDALONE
    #region functions for standalone
    IEnumerator LoadINUnityStandalone(string fileExtention, bool multiSelect)
    {
        yield return null;
        var result = StandaloneFileBrowser.OpenFilePanel("Select File", "", fileExtention, multiSelect);
        if (result.Length != 0)
        {
            if (receivingFiles) receivingFiles.started.Invoke(true);
            for (int i = 0; i < result.Length; i++)
            {
                string baseFilename = System.IO.Path.GetFileName(result[i]);
                string newFilename = System.IO.Path.Combine(Application.persistentDataPath, baseFilename);
                System.IO.File.Copy(result[i], newFilename,true);
                filenames.Add(newFilename);
            }
            ProcessAllFiles();
        }
    }
    #endregion
#endif




}
