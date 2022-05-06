using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Netherlands3D.Events;
using Netherlands3D.FileImporter;

namespace Netherlands3D.FileImporter.Editor
{
    /// <summary>
    /// For changing the default inspector view of class VISSIMEditorTesting for more user friendlyness
    /// </summary>
    [CustomEditor(typeof(FileImporterEditor))]
    public class EditorFileImporterEditor : UnityEditor.Editor
    {   
        private FileImporterEditor selected;

        private void OnEnable()
        {
            selected = (FileImporterEditor)target;
        }

        public override void OnInspectorGUI()
        {
            if(!Application.isPlaying)
            {
                GUILayout.Label("Only Allowed In Editor Runtime!");
                GUI.enabled = false;
            }

            // Import file
            if(GUILayout.Button(new GUIContent("Import File", "Import file to trigger the string event with"), GUILayout.Height(32)))
            {
                string filePath = UnityEditor.EditorUtility.OpenFilePanel("Select File To Load", "", "");
                if(filePath.Length != 0)
                {
                    UnityEngine.Debug.Log("[FileImporterEditor] Load file from: " + filePath);
                    selected.eventImportFiles.started.Invoke(filePath);
                }
            }

            GUI.enabled = true;
            base.OnInspectorGUI();
        }        
    }
}