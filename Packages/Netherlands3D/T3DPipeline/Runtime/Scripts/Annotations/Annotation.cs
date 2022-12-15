using SimpleJSON;
using UnityEngine;

namespace Netherlands3D.T3DPipeline
{
    /// <summary>
    /// Class to hold an annotation that can be added to a building.
    /// </summary>
    public class Annotation
    {
        public int LocalId; // local ID specific to the cityObject it belongs to
        public int GlobalId; // global ID among all annotations
        public string Text { get; set; } // The text of the annotation
        public Vector3Double Position { get; private set; } //the position of the Annotation on the building
        public AnnotationsAttribute ParentAttribute { get; private set; }
        public GameObject AnnotationMarker { get; private set; }

        public Annotation(int localId, int globalId, string text, Vector3Double position, AnnotationsAttribute parentAttribute, GameObject annotationMarker = null)
        {
            LocalId = localId;
            GlobalId = globalId;
            Text = text;
            Position = position;
            ParentAttribute = parentAttribute;
            AnnotationMarker = annotationMarker;
        }

        public JSONNode GetJSONNode()
        {
            var annotation = new JSONObject();
            var point = new JSONArray();
            point.Add("x", Position.x);
            point.Add("y", Position.y);
            point.Add("z", Position.z);
            annotation.Add("position", point);
            annotation.Add("text", Text);
            return annotation;
        }
    }
}
