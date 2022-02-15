using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Netherlands3D.VISSIM.Editor
{
    /// <summary>
    /// For changing the default inspector view of class VISSIMEditorTesting for more user friendlyness
    /// </summary>
    [CustomEditor(typeof(VISSIMTesting))]
    public class EditorVISSIMTesting : UnityEditor.Editor
    {
        private VISSIMTesting selected;

        private void OnEnable()
        {
            selected = (VISSIMTesting)target;
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
                string filePath = EditorUtility.OpenFilePanel("Select .FZP File", "", "fzp");
                if(filePath.Length != 0)
                {
                    if(VISSIMManager.ShowDebugLog) Debug.Log("[VISSIM Testing] Selected .fzp file from: " + filePath);
                    VISSIMManager.LoadFile(filePath);
                }
            }

            GUI.enabled = true;
            base.OnInspectorGUI();
        }
    }
}
