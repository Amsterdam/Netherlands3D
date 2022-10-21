using System;
using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Events;
using SimpleJSON;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Netherlands3D.T3DPipeline
{
    /// <summary>
    /// Annotations are added to a CityObject's attribute node. All annotations of a CityObject are grouped under a single Attribute.
    /// </summary>
    public class AnnotationsAttribute : CityObjectAttribute
    {
        public List<Annotation> annotations { get; private set; } = new List<Annotation>(); // Annotations for this Attribute (belonging to a CityObject)
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

    /// <summary>
    /// Add this script to a CityObject to allow for the interaction to add an annotation.
    /// </summary>
    [RequireComponent(typeof(CityObject))]
    public class CityObjectAnnotations : ObjectClickHandler
    {
        private static int globalId = 0; //counts all annotations with unique ID regardless of to which CityObject it belongs (so adding a annotation to CityObject1 will start at 0, adding a second annotation to CityObject2 will be 1)
        private int localId = 0; // counts annotations with unique ids per CityObject (so adding a annotation to CityObject1 will start at 0, adding a second annotation to CityObject2 will be 0 as well)

        private CityObject parentObject; //CityObject to add annotations to
        private AnnotationsAttribute annotationsAttribute = new AnnotationsAttribute("annotations"); // All Annotations are added as a single JSONObject to the CityObject's attributes
        private static Annotation currentActiveAnnotation; //static so only 1 annotation can be active at any given time regardless of to which object it belongs
        private GameObject activeAnnotationMarker;

        [Tooltip("Global enable/disable of whether to create annotation when clicking on the mesh. Use the associated property to set this in code if needed")]
        [SerializeField]
        private bool annotationStateActive = true;
        public bool AnnotationStateActive { get => annotationStateActive; set => annotationStateActive = value; }

        [Tooltip("Listening for the event when annotation text is changed")]
        [SerializeField]
        private StringEvent onAnnotationTextChanged;
        [Tooltip("Listening for the event when annotation is submitted. This will add the annotation to the CityObject attributes")]
        [SerializeField]
        private TriggerEvent onNewAnnotationSumbmitted;
        [Tooltip("Should annotation ids be counted globally in the application or locally per CityObject?")]
        [SerializeField]
        private bool countAnnotationsGlobally;

        [Header("Optional")]
        [Tooltip("Optional GameObject to be instantiated on the annotation creation point")]
        [SerializeField]
        private GameObject activeAnnotationMarkerPrefab;
        [SerializeField]
        private GameObject completedAnnotationMarkerPrefab;
        [Tooltip("Optional events that are called when the creation of a new annotation has started.")]
        [SerializeField]
        private IntEvent newAnnotationStarted, newAnnotationWithLocalIDStarted, newAnnotationWithGlobalIDStarted;


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
            if (AnnotationStateActive && currentActiveAnnotation == null) //create new annotation if none is currently pending
            {
                var pos = eventData.pointerCurrentRaycast.worldPosition;
                StartAddNewAnnotation(pos);
            }
        }

        // Start adding a new annotation. It is not added to the attributes yet until it is completed.
        public void StartAddNewAnnotation(Vector3 position)
        {
            var doublePos = new Vector3Double(position.x, position.y, position.z);
            var id = countAnnotationsGlobally ? globalId : localId;
            currentActiveAnnotation = new Annotation(id, "", doublePos);

            CreateActiveAnnotationMarker(position);

            onAnnotationTextChanged.started.AddListener(OnActiveAnnotationTextChanged);
            onNewAnnotationSumbmitted.started.AddListener(OnAnnotationSubmitted);

            if (newAnnotationStarted)
                newAnnotationStarted.Invoke(id);
            if (newAnnotationWithLocalIDStarted)
                newAnnotationWithLocalIDStarted.Invoke(localId);
            if (newAnnotationWithGlobalIDStarted)
                newAnnotationWithGlobalIDStarted.Invoke(globalId);

            globalId++;
        }

        protected virtual void CreateActiveAnnotationMarker(Vector3 position)
        {
            if (activeAnnotationMarkerPrefab)
            {
                activeAnnotationMarker = Instantiate(activeAnnotationMarkerPrefab, position, Quaternion.identity);
            }
        }

        protected virtual void ConvertToAnnotationCompletedMarker(GameObject activeAnnotationMarker)
        {
            Instantiate(completedAnnotationMarkerPrefab, activeAnnotationMarker.transform.position, Quaternion.identity);
            Destroy(activeAnnotationMarker);
        }

        // Complete the annotation, add it to the attributes
        private void OnAnnotationSubmitted()
        {
            onAnnotationTextChanged.started.RemoveListener(OnActiveAnnotationTextChanged);
            onNewAnnotationSumbmitted.started.RemoveListener(OnAnnotationSubmitted);

            annotationsAttribute.AddAnnotation(currentActiveAnnotation);
            if (activeAnnotationMarker)
                ConvertToAnnotationCompletedMarker(activeAnnotationMarker);
            currentActiveAnnotation = null;
            localId++;
        }

        private void OnActiveAnnotationTextChanged(string newText)
        {
            currentActiveAnnotation.Text = newText;
        }
    }
}