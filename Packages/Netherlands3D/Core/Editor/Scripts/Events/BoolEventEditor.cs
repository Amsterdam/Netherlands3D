using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Netherlands3D.Events;

namespace Netherlands3D.Events
{
    [CustomEditor(typeof(BoolEvent))]
    public class BoolEventEditor : Editor
    {
        private bool testPayload = true;

        public override void OnInspectorGUI()
        {
            if (Application.isPlaying)
            {
                testPayload = EditorGUILayout.Toggle("Payload", testPayload);
                if (GUILayout.Button("Invoke"))
                {
                    var eventContainer = (BoolEvent)target;
                    eventContainer.InvokeStarted(testPayload);
                    Debug.Log($"Invoked {eventContainer.name}");
                }
            }
        }
    }
}