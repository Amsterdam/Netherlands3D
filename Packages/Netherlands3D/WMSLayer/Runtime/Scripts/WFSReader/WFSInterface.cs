using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WFSInterface : MonoBehaviour
{
    [SerializeField] private Transform featureContentParent;
    [SerializeField] private Button featureButtonPrefab;

    [Header("Listen Events")]
    [SerializeField] private BoolEvent isWmsEvent;

    // Start is called before the first frame update
    void Start()
    {
        isWmsEvent.started.AddListener((bool isWms) =>
        {
            if (!isWms)
                BuildWFSInterface();
        }
        );
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void BuildWFSInterface()
    {
        ResetInterface();

        foreach (WFSFeature feature in WFS.ActiveInstance.features)
        {
            Button b = Instantiate(featureButtonPrefab, featureContentParent);
            b.GetComponentInChildren<Text>().text = feature.FeatureName;
            b.onClick.AddListener(() =>
            {
                WFS.ActiveInstance.TypeName = feature.FeatureName;
                WFS.ActiveInstance.GetFeature();
            }); 
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
