using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using System.IO;

static class determineUsedPackages
{

    private static  HashSet<string> packages = new HashSet<string>();
    // Start is called before the first frame update

    [MenuItem("enableMenuItem", menuItem = "Netherlands3D/test", priority = 1000, validate = true)]
    static bool enableMenuItem()
    {
        return true;
    }

    [MenuItem("enableMenuItem", menuItem = "Netherlands3D/test", priority = 1000)]


    static void getUsedNameSpaces()
    {
        Scene activeScene = SceneManager.GetActiveScene();

        GameObject[] rootGameObjects = activeScene.GetRootGameObjects();
        foreach (GameObject item in rootGameObjects)
        {
            MonoBehaviour[] monobehaviours = item.GetComponentsInChildren<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in monobehaviours)
            {
                string scripttype = behaviour.GetType().FullName;
                if (scripttype.Contains("Netherlands3D"))
                {
                    int lastPoint = scripttype.LastIndexOf('.');
                    if (lastPoint>1)
                    {
                        scripttype = scripttype.Substring(0, lastPoint);
                        packages.Add(scripttype);
                    }
                    
                }
                
            }
        }
        foreach (var item in packages)
        {
            Debug.Log(item);
        }
        string filename = "LICENSE.txt";
        string path = "https://raw.githubusercontent.com/Amsterdam/Netherlands3D/v0.0.1/"+filename;
        Debug.Log(path);
        UnityWebRequest www = UnityWebRequest.Get(path);
        UnityWebRequestAsyncOperation asyncoperation = www.SendWebRequest();
        asyncoperation.completed += readTextfile;
        

       
    }
    static void readTextfile(AsyncOperation obj)
    {
        obj.completed -= readTextfile;
        UnityWebRequestAsyncOperation asyncRequestObj = (UnityWebRequestAsyncOperation)obj;
        UnityWebRequest request = asyncRequestObj.webRequest;
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {
            // Show results as text
            Debug.Log("managed to donwmoad something");
           
            Debug.Log(request.downloadHandler.text);

        }

    }

    [MenuItem("getCurrentVersion", menuItem = "Netherlands3D/getCurrentVersion", priority = 1)]
    static void GetCurrentPackageVersion()
    {
        string path = Path.GetFullPath("Packages/com.unity.timeline/package.json");
        Debug.Log(path);
        StreamReader reader = new StreamReader(path);
        Debug.Log(reader.ReadToEnd());
        reader.Close();
    }
}
