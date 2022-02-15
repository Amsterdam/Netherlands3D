using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//[CustomEditor(typeof(LayerMaterials))]
//[CanEditMultipleObjects]
//public class CustomInspector : Editor
//{
//    SerializedProperty materialsList;

//    void OnEnable()
//    {
//        materialsList = serializedObject.FindProperty("Colors");
//    }

//    public override void OnInspectorGUI()
//    {
//        serializedObject.Update();
//        EditorGUILayout.PropertyField(materialsList);
//        if (GUILayout.Button("Apply"))
//            ApplyColors();
//        serializedObject.ApplyModifiedProperties();
//    }

//    private void ApplyColors()
//    {
//        Debug.Log("applying Colors");
//    }

//}

