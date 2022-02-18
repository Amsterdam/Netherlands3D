using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Netherlands3D.VISSIM.Editor
{
    /// <summary>
    /// Custom editor for the script Entity
    /// </summary>
    [CustomEditor(typeof(Entity))]
    public class EditorEntity : UnityEditor.Editor
    {
        private Entity selected;

        private void OnEnable()
        {
            selected = (Entity)target;
        }

        public override void OnInspectorGUI()
        {
            if(!Application.isPlaying)
            {
                base.OnInspectorGUI();
                return;
            }

            // Display entity data
            GUILayout.Label("Data Values", EditorStyles.boldLabel);
            if(selected.Data == null) return;
            GUILayout.Label(string.Format("ID: {0}", selected.Data.id));
            GUILayout.Label(string.Format("Name: '{0}' With Type Index: {1}", TypeIndexToName(selected.Data.entityTypeIndex), selected.Data.entityTypeIndex));
            GUILayout.Label(string.Format("Bounds: {0}, 1, {1}", selected.Data.width, selected.Data.length));
            if(selected.Data.coordinates != null) GUILayout.Label(string.Format("Coordinates Count: {0}", selected.Data.coordinates.Count));
        }

        private string TypeIndexToName(int index)
        {
            switch(index)
            {
                case 100: return "Car";
                case 200: return "Truck";
                case 300: return "Bus";
                case 400: return "Tram";
                case 500: return "Pedestrian";
                case 600: return "Cyclist";
                case 700: return "Van";
                default: return "Not Defined";
            }
        }
    }
}
