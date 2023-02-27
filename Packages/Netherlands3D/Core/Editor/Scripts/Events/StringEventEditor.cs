using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Netherlands3D.Events;

namespace Netherlands3D.Events
{
    [CustomEditor(typeof(StringEvent))]
    public class StringEventEditor : Editor
    {
        private string testPayload = "";

        public override void OnInspectorGUI()
        {
            if (Application.isPlaying)
            {
                testPayload = EditorGUILayout.TextArea(testPayload);
                if (GUILayout.Button("Invoke"))
                {
                    var eventContainer = (StringEvent)target;
                    eventContainer.InvokeStarted(testPayload);
                    Debug.Log($"Invoked {eventContainer.name}");
                }
            }
        }
    }
}