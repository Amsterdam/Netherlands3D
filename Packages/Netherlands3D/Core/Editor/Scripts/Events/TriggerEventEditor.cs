using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Netherlands3D.Events;

namespace Netherlands3D.Events
{
    [CustomEditor(typeof(TriggerEvent))]
    public class TriggerEventEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (Application.isPlaying)
            {
                if (GUILayout.Button("Invoke"))
                {
                    var eventContainer = (TriggerEvent)target;
                    eventContainer.InvokeStarted();
                    Debug.Log($"Invoked {eventContainer.name}");
                }
            }
        }
    }
}