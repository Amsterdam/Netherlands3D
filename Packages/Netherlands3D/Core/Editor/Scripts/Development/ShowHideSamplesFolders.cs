using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Netherlands3D.Development
{
    public class ShowHideSamplesFolders : MonoBehaviour
    {
        [MenuItem("Assets/Netherlands3D/Show Samples folders")]
        static void ShowSamples()
        {
            Object selectedObject = Selection.activeObject;
            if (selectedObject == null)
            {
                Debug.Log("Please select a folder");
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(selectedObject);
            if (Directory.Exists(assetPath))
            {
                Debug.Log($"Recursively renaming 'Samples~' to 'Samples' folders in {assetPath}");
                RenameFolders(assetPath, "Samples~", "Samples", true);
            }
            else
            {
                Debug.Log("Please select a folder");
            }

            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/Netherlands3D/Hide Samples folders")]
        static void HideSamples()
        {
            Object selectedObject = Selection.activeObject;
            if (selectedObject == null)
            {
                Debug.Log("Please select a folder");
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(selectedObject);
            if (Directory.Exists(assetPath))
            {
                Debug.Log($"Recursively renaming 'Samples' to 'Samples~' folders in {assetPath}");
                RenameFolders(assetPath, "Samples", "Samples~", true);
            }
            else
            {
                Debug.Log("Please select a folder");
            }

            

            AssetDatabase.Refresh();
        }

        static void RenameFolders(string folderPath, string from, string to, bool removeMeta = false)
        {
            foreach (string folder in Directory.GetDirectories(folderPath))
            {
                if (Path.GetFileName(folder) == from)
                {
                    if (removeMeta) { 
                        var metaFile = folder + ".meta";
                        if (File.Exists(metaFile))
                        {
                            Debug.Log($"Removing meta file: {metaFile}");
                            File.Delete(metaFile);
                        }
                    }
                    string newFolderPath = Path.Combine(Path.GetDirectoryName(folder), to);
                    var targetExists = Directory.Exists(newFolderPath);
                    if (targetExists && Directory.GetFiles(newFolderPath).Length < 1)
                    {
                        Debug.Log($"Empty {newFolderPath} found. Removing.");
                        Directory.Delete(newFolderPath);
                    }
                    else if (!targetExists)
                    {
                        Directory.Move(folder, newFolderPath);
                        Debug.Log($"{folder} -> {newFolderPath}");
                    }
                }
                else
                {
                    RenameFolders(folder, from, to, removeMeta);
                }
            }
        }
    }
}