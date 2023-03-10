using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Netherlands3D.Events;

namespace Netherlands3D.Events
{
    [CustomEditor(typeof(FloatEvent))]
    public class FloatEventEditor : Editor
    {
        private float testPayload = 0;

        public override void OnInspectorGUI()
        {
            if (Application.isPlaying)
            {
                testPayload = EditorGUILayout.FloatField(testPayload);
                if (GUILayout.Button("Invoke"))
                {
                    var eventContainer = (FloatEvent)target;
                    eventContainer.InvokeStarted(testPayload);
                    Debug.Log($"Invoked {eventContainer.name}");
                }
            }
        }
    }
}
