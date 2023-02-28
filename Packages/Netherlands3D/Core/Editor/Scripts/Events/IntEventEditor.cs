using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Netherlands3D.Events;

namespace Netherlands3D.Events
{
    [CustomEditor(typeof(IntEvent))]
    public class IntEventEditor : Editor
    {
        private int testPayload = 0;

        public override void OnInspectorGUI()
        {
            if (Application.isPlaying)
            {
                testPayload = EditorGUILayout.IntField(testPayload);
                if (GUILayout.Button("Invoke"))
                {
                    var eventContainer = (IntEvent)target;
                    eventContainer.InvokeStarted(testPayload);
                    Debug.Log($"Invoked {eventContainer.name}");
                }
            }
        }
    }
}