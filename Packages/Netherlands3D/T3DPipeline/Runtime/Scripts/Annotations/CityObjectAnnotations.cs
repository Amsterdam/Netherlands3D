using System;
using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Events;
using SimpleJSON;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Netherlands3D.T3DPipeline
{
    public class AnnotationsAttribute : CityObjectAttribute
    {
        public List<Annotation> annotations { get; private set; } = new List<Annotation>();
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
    public class CityObjectAnnotations : ObjectClickHandler
    {
        private static int globalId = 0;
        private int localId = 0;

        private CityObject parentObject;
        private AnnotationsAttribute annotationsAttribute = new AnnotationsAttribute("annotations");

        [SerializeField]
        private bool annotationStateActive = true;
        public bool AnnotationStateActive { get => annotationStateActive; set => annotationStateActive = value; }

        [SerializeField]
        private StringEvent onAnnotationTextChanged;
        [SerializeField]
        private TriggerEvent onAnnotationSumbmitted;


        private void Awake()
        {
            parentObject = GetComponent<CityObject>();
        }

        private void Start()
        {
            parentObject.AddAttribute(annotationsAttribute);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            if (AnnotationStateActive)
            {
                var pos = eventData.pointerCurrentRaycast.worldPosition;
                AddNewAnnotation(pos);
            }
        }

        public void AddNewAnnotation(Vector3 position)
        {
            var doublePos = new Vector3Double(position.x, position.y, position.z);
            var annotation = new Annotation(globalId, "", doublePos);
            annotationsAttribute.AddAnnotation(annotation);
            globalId++;

            onAnnotationTextChanged.started.AddListener(OnActiveAnnotationTextChanged);
            onAnnotationSumbmitted.started.AddListener(OnAnnotationSubmitted);
        }

        private void OnAnnotationSubmitted()
        {
            onAnnotationTextChanged.started.RemoveListener(OnActiveAnnotationTextChanged);
            onAnnotationSumbmitted.started.RemoveListener(OnAnnotationSubmitted);
            localId++;
        }

        private void OnActiveAnnotationTextChanged(string newText)
        {
            annotationsAttribute.annotations[localId].Text = newText;
        }
    }
}