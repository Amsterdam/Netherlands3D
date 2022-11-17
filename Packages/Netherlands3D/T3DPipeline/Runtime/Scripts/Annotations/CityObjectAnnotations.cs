using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public List<Annotation> Annotations { get; private set; } = new List<Annotation>(); // Annotations for this Attribute (belonging to a CityObject)
        public AnnotationsAttribute(CityObject parentCityObject, string key) : base(parentCityObject ,key)
        {
        }

        public void AddAnnotation(Annotation annotation)
        {
            Annotations.Add(annotation);
        }

        public void RemoveAnnotation(int localId)
        {
            Annotations.RemoveAt(localId);
            RecalculateLocalIds();
        }

        private void RecalculateLocalIds()
        {
            for (int i = 0; i < Annotations.Count; i++)
            {
                Annotations[i].LocalId = i;
            }
        }

        public override JSONNode GetJSONValue()
        {
            Value = new JSONObject();
            foreach (var annotation in Annotations)
            {
                Value.Add(annotation.LocalId.ToString(), annotation.GetJSONNode());
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
        private static List<Annotation> allCompletedAnnotations = new List<Annotation>();
        private static List<GameObject> annotationMarkers = new List<GameObject>();
        private CityObject parentObject; //CityObject to add annotations to
        private AnnotationsAttribute annotationsAttribute; // All Annotations are added as a single JSONObject to the CityObject's attributes
        private Annotation currentActiveAnnotation; //static so only 1 annotation can be active at any given time regardless of to which object it belongs
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

        [Tooltip("GameObject to be instantiated on the annotation creation point")]
        [SerializeField]
        private GameObject activeAnnotationMarkerPrefab;

        [Header("Optional")]
        [Tooltip("Optional events that are called when the creation of a new annotation has started.")]
        [SerializeField]
        private IntEvent newAnnotationWithLocalIDStarted;
        [SerializeField]
        private IntEvent newAnnotationWithGlobalIDStarted;
        [SerializeField]
        private GameObjectEvent annotationMarkerSelected, annotationMarkerDeselected;
        [SerializeField]
        private IntEvent reselectAnnotationEvent;

        private void Awake()
        {
            parentObject = GetComponent<CityObject>();
            annotationsAttribute = new AnnotationsAttribute(parentObject, "annotations");
        }

        private void Start()
        {
            parentObject.AddAttribute(annotationsAttribute);
        }

        private void OnEnable()
        {
            objectClicked.started.AddListener(OnObjectClicked);
        }

        private void OnDisable()
        {
            objectClicked.started.RemoveListener(OnObjectClicked);
        }

        private void OnObjectClicked(Vector3 pos)
        {
            if (AnnotationStateActive && currentActiveAnnotation == null) //create new annotation if none is currently pending
            {
                StartAddNewAnnotation(pos);
            }
        }

        // Start adding a new annotation. It is not added to the attributes yet until it is completed.
        public void StartAddNewAnnotation(Vector3 position)
        {
            var doublePos = new Vector3Double(position.x, position.y, position.z);
            CreateActiveAnnotationMarker(position);
            currentActiveAnnotation = new Annotation(localId, globalId, "", doublePos, annotationsAttribute, activeAnnotationMarker);


            onAnnotationTextChanged.started.AddListener(OnActiveAnnotationTextChanged);
            onNewAnnotationSumbmitted.started.AddListener(OnAnnotationSubmitted);

            var id = countAnnotationsGlobally ? globalId : localId;
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
                annotationMarkers.Add(activeAnnotationMarker);
            }
        }

        protected virtual void DeselectCurrentAnnotation()
        {
            if (currentActiveAnnotation != null)
            {
                onAnnotationTextChanged.started.RemoveListener(OnActiveAnnotationTextChanged);
                if (activeAnnotationMarker && annotationMarkerDeselected != null)
                    annotationMarkerDeselected.Invoke(activeAnnotationMarker);
                currentActiveAnnotation = null;
                activeAnnotationMarker = null;
            }
        }

        // Complete the annotation, add it to the attributes
        private void OnAnnotationSubmitted()
        {
            onAnnotationTextChanged.started.RemoveListener(OnActiveAnnotationTextChanged);
            onNewAnnotationSumbmitted.started.RemoveListener(OnAnnotationSubmitted);

            annotationsAttribute.AddAnnotation(currentActiveAnnotation);
            allCompletedAnnotations.Add(currentActiveAnnotation);
            currentActiveAnnotation = null;
            activeAnnotationMarker = null;
            localId++;
        }

        private void OnActiveAnnotationTextChanged(string newText)
        {
            currentActiveAnnotation.Text = newText;
        }

        public static Annotation GetAnnotation(int globalId)
        {
            var annotation = allCompletedAnnotations.FirstOrDefault(ann => ann.GlobalId == globalId);
            return annotation;
        }

        public bool SelectAnnotation(int globalId, bool submitCurrentActiveAnnotation = true)
        {
            if (currentActiveAnnotation != null && submitCurrentActiveAnnotation)
            {
                OnAnnotationSubmitted();
            }
            DeselectCurrentAnnotation();

            var annotation = GetAnnotation(globalId);
            if (annotationsAttribute.Annotations.Contains(annotation))
            {
                currentActiveAnnotation = annotation;
                onAnnotationTextChanged.started.AddListener(OnActiveAnnotationTextChanged);
                return true;
            }
            return false;
        }

        public int GetLocalId(int globalId)
        {
            var ann = GetAnnotation(globalId);

            if (ann != null)
                return ann.LocalId;

            return -1;
        }

        private static void RecalculateGlobalIds()
        {
            for (int i = 0; i < allCompletedAnnotations.Count; i++)
            {
                allCompletedAnnotations[i].GlobalId = i;
            }
        }

        public void DeleteAnnotation(int localId)
        {
            var ann = annotationsAttribute.Annotations[localId];
            DeleteAnnotationWithGlobalId(ann.GlobalId);
        }

        public static void DeleteAnnotationWithGlobalId(int globalId)
        {
            var ann = GetAnnotation(globalId);
            if (ann.AnnotationMarker)
            {
                annotationMarkers.Remove(ann.AnnotationMarker);
                Destroy(ann.AnnotationMarker);
            }
            ann.ParentAttribute.RemoveAnnotation(ann.LocalId);
            RecalculateGlobalIds();
        }
    }
}
