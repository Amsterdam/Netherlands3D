using SimpleJSON;

namespace Netherlands3D.T3DPipeline
{
    /// <summary>
    /// Class to hold an annotation that can be added to a building.
    /// </summary>
    public class Annotation
    {
        public int Id; // An annotation has to have an id that is unique per CityObject. It may be unique globally for all CityObjects but this is not required.
        public string Text { get; set; } // The text of the annotation
        public Vector3Double Position { get; private set; } //the position of the Annotation on the building

        public Annotation(int id, string text, Vector3Double position)
        {
            Id = id;
            Text = text;
            Position = position;
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
