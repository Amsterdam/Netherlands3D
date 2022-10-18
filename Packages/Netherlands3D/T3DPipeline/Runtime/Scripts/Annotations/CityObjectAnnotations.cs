using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Events;
using SimpleJSON;
using UnityEngine;

namespace Netherlands3D.T3DPipeline
{
    public class AnnotationsAttribute : CityObjectAttribute
    {
        private List<Annotation> annotations = new List<Annotation>();
        public AnnotationsAttribute(string key) : base(key)
        {
        }

        public void AddAnnotation(Annotation annotation)
        {
            annotations.Add(annotation);
        }

        public override JSONNode GetJSONValue()
        {
            Value = new JSONObject();
            foreach (var annotation in annotations)
            {
                Value.Add(annotation.Id.ToString(), annotation.GetJSONNode());
            }
            return Value;
        }
    }

    [RequireComponent(typeof(CityObject))]
    public class CityObjectAnnotations : MonoBehaviour
    {
        private static int id = 0;

        private CityObject parentObject;
        private AnnotationsAttribute annotationsAttribute = new AnnotationsAttribute("annotations");

        [SerializeField]
        private Vector3Event onObjectClicked;

        private void Awake()
        {
            parentObject = GetComponent<CityObject>();
        }

        private void Start()
        {
            parentObject.AddAttribute(annotationsAttribute);
        }

        private void OnEnable()
        {
            onObjectClicked.started.AddListener(AddAnnotation);
        }

        private void OnDisable()
        {
            onObjectClicked.started.RemoveAllListeners();
        }

        public void AddAnnotation(Vector3 position) //todo: type text
        {
            var doublePos = new Vector3Double(position.x, position.y, position.z);
            var annotation = new Annotation(id, id.ToString(), doublePos);
            annotationsAttribute.AddAnnotation(annotation);
            id++;
        }
    }
}