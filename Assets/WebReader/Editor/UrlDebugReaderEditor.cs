using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DebugReader))]
public class UrlDebugReaderEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        DebugReader myReader = (DebugReader)target;
        if(GUILayout.Button("Get URL"))
        {
            myReader.ReadURLInEditor();
        }

    }

}
