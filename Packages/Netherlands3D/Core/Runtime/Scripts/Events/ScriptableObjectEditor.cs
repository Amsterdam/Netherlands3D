#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using Netherlands3D.Events;
using UnityEngine.Events;

public class ScriptableObjectEditor : Editor
{
    [MenuItem("Assets/Netherlands3D/Log ScriptableObject references")]
    public static void CountReferences()
    {
        ScriptableObject[] instances = GetAllInstances<ScriptableObject>();
        foreach(var scriptableObject in instances)
        {
            if(scriptableObject == Selection.activeObject)
            {
                LogReferences(scriptableObject);
            }
        }
    }

    [MenuItem("Assets/Netherlands3D/Log ScriptableObject references", true)]
    public static bool ValidateCountReferences()
    {
        return Selection.activeObject != null;
    }

    public static T[] GetAllInstances<T>() where T : ScriptableObject
    {
        string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);  //FindAssets uses tags check documentation for more info
        T[] a = new T[guids.Length];
        for (int i = 0; i < guids.Length; i++)         //probably could get optimized 
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            a[i] = AssetDatabase.LoadAssetAtPath<T>(path);
        }

        return a;

    }

    public static void LogReferences(ScriptableObject scriptableObject)
    {
        Component[] allComponents = (Component[])Resources.FindObjectsOfTypeAll(typeof(Component));
        int references = 0;
        foreach (var component in allComponents)
        {
            references += ComponentReferences(component, scriptableObject);
        }
        Debug.Log($"References found: {references}.", scriptableObject);
    }

    private static int ComponentReferences(Component component, ScriptableObject scriptableObject)
    {
        SerializedObject serializedObject = new SerializedObject(component);
        SerializedProperty property = serializedObject.GetIterator();

        int count = 0;
        while (property.NextVisible(true))
        {
            bool isObjectField = property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue != null;
            if (isObjectField && property.objectReferenceValue == scriptableObject)
            {
                Debug.Log($"{scriptableObject.name} is referenced by:{component}", component);
                count++;
            }
        }
        return count;
    }
}
#endif