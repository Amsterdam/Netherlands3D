using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class ShowHideSamplesFolders : MonoBehaviour
{
    [MenuItem("Assets/Netherlands3D/Show samples folders")]
    static void ShowSamples()
    {
        Object selectedObject = Selection.activeObject;
        if (selectedObject == null)
        {
            Debug.Log("Please select a folder");
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(selectedObject);
        string directory = Path.GetDirectoryName(assetPath);
        if (Directory.Exists(directory))
        {
            RenameFolders(directory, "samples~", "samples");
        }
    }

    [MenuItem("Assets/Netherlands3D/Hide samples folders")]
    static void HideSamples()
    {
        Object selectedObject = Selection.activeObject;
        if (selectedObject == null)
        {
            Debug.Log("Please select a folder");
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(selectedObject);
        string directory = Path.GetDirectoryName(assetPath);
        if (Directory.Exists(directory))
        {
            RenameFolders(directory, "samples", "samples~");
        }

        var metaFile = directory + ".meta";
        Debug.Log("Removing meta file if generated: " + metaFile);
        if (File.Exists(directory + ".meta"))
        {

        }
    }


    static void RenameFolders(string folderPath, string from, string to)
    {
        foreach (string folder in Directory.GetDirectories(folderPath))
        {
            if (Path.GetFileName(folder) == from)
            {
                string newFolderPath = Path.Combine(Path.GetDirectoryName(folder), to);
                Directory.Move(folder, newFolderPath);
            }
            else
            {
                RenameFolders(folder, from, to);
            }
        }
    }
}
