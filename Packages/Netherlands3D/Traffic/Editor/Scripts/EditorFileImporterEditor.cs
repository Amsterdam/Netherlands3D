using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Netherlands3D.Traffic.Editor
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

            // Import .fpz
            if(GUILayout.Button(new GUIContent("Import .FZP", "Import a .FPZ file to test"), GUILayout.Height(32)))
            {
                selected.File.Open();
            }

            GUI.enabled = true;
            base.OnInspectorGUI();
        }
    }
}
