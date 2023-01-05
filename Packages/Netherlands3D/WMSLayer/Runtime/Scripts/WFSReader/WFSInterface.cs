using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WFSInterface : MonoBehaviour
{
    [SerializeField] private Transform featureContentParent;
    [SerializeField] private Transform settingsParent;
    [SerializeField] private Button featureButtonPrefab;

    [Header("Invoked Events")]
    //[SerializeField] private StringEvent getFeatureEvent;
    [SerializeField] private ObjectEvent setActiveFeatureEvent;
    [Header("Listen Events")]
    [SerializeField] private ObjectEvent wfsDataEvent;

    private WFSFeature activeFeature;

    // Start is called before the first frame update
    void Start()
    {
        wfsDataEvent.started.AddListener((object wfs) => BuildWFSInterface((WFS)wfs));
    }

    //public void InvokeFeatureEvent()
    //{
    //    getFeatureEvent.Invoke(activeFeature.FeatureName);
    //}
    private void BuildWFSInterface(WFS wfs)
    {
        ResetInterface();
        foreach (WFSFeature feature in wfs.features)
        {
            Button b = Instantiate(featureButtonPrefab, featureContentParent);
            b.GetComponentInChildren<Text>().text = feature.FeatureName;
            b.onClick.AddListener(() => 
                {
                    setActiveFeatureEvent.Invoke(feature);
                    settingsParent.gameObject.SetActive(true);
                }
            );
            
            //b.onClick.AddListener(() => getFeatureEvent.Invoke(feature.FeatureName));
        }
    }
    private void ResetInterface()
    {
        foreach(Transform child in featureContentParent)
        {
            child.GetComponent<Button>().onClick.RemoveAllListeners();
            Destroy(child.gameObject);
        }
    }


}
