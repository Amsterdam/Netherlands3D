using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

namespace Netherlands3D.T3DPipeline
{
    public class Annotation
    {
        public int Id;
        public string Text { get; set; }
        public Vector3Double Position { get; private set; }

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
