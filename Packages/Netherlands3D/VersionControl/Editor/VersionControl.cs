using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Networking;
using SimpleJSON;
using UnityEngine.SceneManagement;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

public class VersionControl : EditorWindow
{
    static bool importIsActive;
    static float symbolCounter = 0;
    static bool CurrentVersionIsDefined;
    static bool isProperVersion;
    static string currentVersion;
    static bool packageSwappable = true;
    static string mostRecentVersion;
    static List<string> availableVersions = new List<string>();
    static List<string> packagenames = new List<string>();
    static string packagename;
    static int index = 0;
    Vector2 usedNamespacesContainer;
    static AddRequest Request;
    static ListRequest listRequest;
    static VersionControl window;
    [MenuItem("Netherlands3D/versioncontrol")]
    static void Init()
    {
        importIsActive = false;
        CurrentVersionIsDefined = false;
        packagenames = new List<string>();

        // Get existing open window or if none, make a new one:
        window = (VersionControl)EditorWindow.GetWindow(typeof(VersionControl));
        window.Show();
        UpdatePackageInfo();
        
        //window.Repaint();

        
    }

    static void UpdatePackageInfo()
    {
        GetCurrentVersion();
        GetMostRecentVersion();
        
        
        //getUsedNamespaces();
        window.Repaint();
    }


    private void OnGUI()
    {
        
        GUILayout.Label("Netherlands3D Version Control", EditorStyles.boldLabel);

        if (importIsActive)
        {
            EditorGUILayout.BeginHorizontal();
            string labeltekst = "bezig met laden ";
            for (int i = 0; i < symbolCounter; i++)
            {
                labeltekst = $"{labeltekst}.";
            }
            symbolCounter++;
            if (symbolCounter>20)
            {
                symbolCounter = 0;
            }
            GUILayout.Label(labeltekst);
            EditorGUILayout.EndHorizontal();
            
            return;
        }

        if (packageSwappable == false)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("je kunt niet van versie wisselen als deze local is");
            EditorGUILayout.EndHorizontal();
            return;
        }

        EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Current Release: ", currentVersion);
            EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        if (CurrentVersionIsDefined == false)
        {
            return;
        }

        if (isProperVersion==false)
        {
            MenuItemifNotUsingRelease();
            return;
        }


       
        EditorGUILayout.BeginHorizontal();
        
                    GUILayout.Label("Available Releases:");
                    GUILayout.FlexibleSpace();
                    index = EditorGUILayout.Popup(index, availableVersions.ToArray());
                    GUILayout.FlexibleSpace();
        if (availableVersions[index]!=currentVersion)
        {
            if (GUILayout.Button("Update Package"))
                ImportPackage();

                
        }            
        
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("view ChangeLOG"))
            Application.OpenURL("https://github.com/Amsterdam/Netherlands3D/blob/main/CHANGELOG.md");
        EditorGUILayout.EndHorizontal();
        




    }

    private void MenuItemifNotUsingRelease()
    {
        GUILayout.Label("you are not working with a specific release of the package\nplease select a release to import");
        EditorGUILayout.BeginHorizontal();
        index = EditorGUILayout.Popup(index, availableVersions.ToArray());
        if (GUILayout.Button("Import Package"))
            ImportPackage();
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    private static void ImportPackage()
    {
        importIsActive = true;
        window.Repaint();
        
        string version = availableVersions[index];
        string packageURL = "https://github.com/Amsterdam/Netherlands3D.git?path=/Packages/Netherlands3D#" + version;
        Request = Client.Add(packageURL);
        EditorApplication.update += OnPackageInstalled;
    }
    static void OnPackageInstalled()
    {
        
        if (Request.IsCompleted)
        {
            
            Debug.Log("package received");
            if (Request.Status == StatusCode.Success)
            {
                Debug.Log("package installed");
                
                
            }
            else if (Request.Status == StatusCode.Failure)
            {
                Debug.Log("couldn't import the package");
            }
            EditorApplication.update -= OnPackageInstalled;
            
            importIsActive = false;
            UpdatePackageInfo();
        }
        
    }


    private static void GetCurrentVersion()
    {
        string path = Path.GetFullPath("Packages/nl.netherlands3d");
        
        listRequest = Client.List();    // List packages installed for the project
        EditorApplication.update += readPackageList;

    }

    private static void readPackageList()
    {
        if (listRequest.IsCompleted)
        {
            if (listRequest.Status == StatusCode.Success)
            {
                foreach (var package in listRequest.Result)
                    if (package.name == "nl.netherlands3d")
                    {
                        GitInfo gi = package.git;
                        if (gi==null)
                        {
                            packageSwappable = false;
                            return;
                        }
                        Debug.Log("packageversion: " + gi.revision);
                        if (gi.revision == "HEAD")
                        {
                            isProperVersion = false;
                        }
                        else
                        {
                            isProperVersion = true;
                            
                        }
                        currentVersion = gi.revision;
                        CurrentVersionIsDefined = true;
                        window.Repaint();
                    }

                    else if (listRequest.Status >= StatusCode.Failure)
                    {
                        Debug.Log("something went wrong");
                        Debug.Log(Request.Error.message);
                    }
            }
            else
            {
                Debug.Log("couldnt get al ist of the installed packages");
            }
            EditorApplication.update -= readPackageList;
            window.Repaint();
        }
    }

    private static void GetMostRecentVersion()
    {
        string path = "https://api.github.com/repos/Amsterdam/Netherlands3D/tags";
        ///path = "https://api.github.com/repos/Amsterdam/3DAmsterdam/tags";
        UnityWebRequest www = UnityWebRequest.Get(path);
        UnityWebRequestAsyncOperation asyncoperation = www.SendWebRequest();
        asyncoperation.completed += readGitPackageJSON;
    }
    private static void readGitPackageJSON(AsyncOperation obj)
    {
        obj.completed -= readGitPackageJSON;
        UnityWebRequestAsyncOperation asyncRequestObj = (UnityWebRequestAsyncOperation)obj;
        UnityWebRequest request = asyncRequestObj.webRequest;
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {
            // Show results as text
            //Debug.Log("managed to donwmoad something");
            string resulttext = request.downloadHandler.text;
            JSONNode releaseJSON = JSON.Parse(request.downloadHandler.text);

            mostRecentVersion = releaseJSON[0]["name"].Value;

            for (int i = 0; i < releaseJSON.Count; i++)
            {

                availableVersions.Add(releaseJSON[i]["name"].Value);
            }

            //Debug.Log(request.downloadHandler.text);

        }
    }

    private static void getUsedNamespaces()
    {
        //Scene activeScene = SceneManager.GetActiveScene();

        //GameObject[] rootGameObjects = activeScene.GetRootGameObjects();
        //foreach (GameObject item in rootGameObjects)
        //{
        //    MonoBehaviour[] monobehaviours = item.GetComponentsInChildren<MonoBehaviour>();
        //    foreach (MonoBehaviour behaviour in monobehaviours)
        //    {
        //        string scripttype = behaviour.GetType().FullName;
        //        if (scripttype.Contains("Netherlands3D"))
        //        {
        //            //int lastPoint = scripttype.LastIndexOf('.');
        //            //if (lastPoint > 1)
        //            //{
        //                //scripttype = scripttype.Substring(0, lastPoint);
        //                packagenames.Add(scripttype);
        //                packagename += scripttype;
        //            //}
        //        }
        //    }
        //}
    }
}
