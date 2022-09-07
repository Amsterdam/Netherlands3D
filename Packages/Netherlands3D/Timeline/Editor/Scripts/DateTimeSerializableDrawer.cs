using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Netherlands3D.Timeline.Editor
{
    [CustomPropertyDrawer(typeof(DateTimeSerializable))]
    public class DateTimeSerializableDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            // Dont make child fields be indented
            int previousIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            // Calc rects
            Rect rect = new Rect(position.x, position.y, position.width, position.height);
            // Draw field
            EditorGUI.PropertyField(rect, property.FindPropertyRelative("dateTimeString"), GUIContent.none);
            // Set indent back
            EditorGUI.indentLevel = previousIndent;

            EditorGUI.EndProperty();
        }
    }
}
