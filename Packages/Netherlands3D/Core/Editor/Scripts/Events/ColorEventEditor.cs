using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Netherlands3D.Events;

namespace Netherlands3D.Events
{
    [CustomEditor(typeof(ColorEvent))]
    public class ColorEventEditor : Editor
    {
        private Color testPayload = Color.white;

        public override void OnInspectorGUI()
        {
            if (Application.isPlaying)
            {
                testPayload = EditorGUILayout.ColorField("Payload", testPayload);
                if (GUILayout.Button("Invoke"))
                {
                    var eventContainer = (ColorEvent)target;
                    eventContainer.InvokeStarted(testPayload);
                    Debug.Log($"Invoked {eventContainer.name}");
                }
            }
        }
    }
}