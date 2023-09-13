using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Netherlands3D.WFSHandlers;
using UnityEngine.Events;

public class WFSInterface : MonoBehaviour
{
    [SerializeField] private Transform featureContentParent;
    [SerializeField] private Transform settingsParent;
    [SerializeField] private Transform filterParent;
    [SerializeField] private Button featureButtonPrefab;
    [SerializeField] private TextMeshProUGUI activeFeatureText;
    [SerializeField] private GameObject optionsContentPrefab;

    [Header("Invoked Events")]
    //[SerializeField] private StringEvent getFeatureEvent;
    //[SerializeField] private UnityEvent<List<WFSFeature>> wfsFeatureListEvent;
    [SerializeField] private ObjectEvent setActiveFeatureEvent;
    [Header("Listen Events")]
    [SerializeField] private ObjectEvent wfsDataEvent;
    [SerializeField] private ObjectEvent activateFeatureEvent;

    //private WFSFeature activeFeature;
    //private WFS2 activatedWFS;

    // Start is called before the first frame update
    void Start()
    {
        //wfsFeatureListEvent.AddListener(BuildWFSInterface);
        //wfsDataEvent.AddListenerStarted((object wfs) => BuildWFSInterface((WFS2)wfs));
        activateFeatureEvent.AddListenerStarted((object feature) => BuildFilterInterface((WFSFeature)feature));
    }

    //public void InvokeFeatureEvent()
    //{
    //    getFeatureEvent.Invoke(activeFeature.FeatureName);
    //}
    private void BuildFilterInterface(WFSFeature activeFeature)
    {
        print("should build interface for " + activeFeature.FeatureName);
        return;
        var startTime = Time.realtimeSinceStartupAsDouble;
        print("starting BuildINterface()" + startTime);
        for (int i = filterParent.childCount - 1; i >= 0; i--)
        {
            Destroy(filterParent.GetChild(i).gameObject);
        }
        Dictionary<string, Transform> uniqueFields = new();
        List<object> uniqueData = new();
        Debug.Log("Should build filter interface!");
        foreach(WFSFeatureData feature in activeFeature.GetFeatureDataList)
        {
            Debug.Log("Evaluating a feature for filtering!");
            foreach (KeyValuePair<string, object> kvp in feature.GetPropertyDictionary())
            {
                Debug.Log("Evaluating a KeyValuePair for filtering!");
                string key = kvp.Key;
                if (!uniqueFields.ContainsKey(key))
                {
                    Debug.Log("Found an unique key! Creating new field!");
                    GameObject optionsPanel = Instantiate(optionsContentPrefab, filterParent);
                    optionsPanel.GetComponentInChildren<TextMeshProUGUI>().text = key;
                    uniqueFields.Add(key, optionsPanel.GetComponentInChildren<ContentSizeFitter>().transform);
                }
                object kvpValue = kvp.Value;
                if (!uniqueData.Contains(kvpValue))
                {
                    Debug.Log("Found unfound filter data! Creating new filter button!");
                    Button filterButton = Instantiate(featureButtonPrefab, uniqueFields[key]);
                    filterButton.GetComponentInChildren<TextMeshProUGUI>().text = kvp.Value.ToString();
                    uniqueData.Add(kvpValue);
                }
            }
        }

        var endTime = Time.realtimeSinceStartupAsDouble;
        print("completed BuildINterface() in " + (endTime-startTime)*1000 + "ms");
    }

    private void BuildWFSInterface(List<WFSFeature> wfsFeatureData)
    {
        var startTime = Time.realtimeSinceStartupAsDouble;
        print("starting BuildWFSInterface()" + startTime);
        ResetInterface();
        foreach (WFSFeature feature in wfsFeatureData)
        {
            Button b = Instantiate(featureButtonPrefab, featureContentParent);
            b.GetComponentInChildren<TextMeshProUGUI>().text = feature.FeatureName;
            b.onClick.AddListener(() => 
                {
                    setActiveFeatureEvent.InvokeStarted(feature);
                    activeFeatureText.text = feature.FeatureName;
                    settingsParent.gameObject.SetActive(true);
                }
            );

            //b.onClick.AddListener(() => getFeatureEvent.Invoke(feature.FeatureName));
            //activatedWFS = wfs;
        }

        var endTime = Time.realtimeSinceStartupAsDouble;
        print("completed ResetInterface() in " + (endTime - startTime) * 1000 + "ms");
    }
    private void ResetInterface()
    {

        foreach (Transform child in featureContentParent)
        {
            child.GetComponent<Button>().onClick.RemoveAllListeners();
            Destroy(child.gameObject);
        }
    }


}
