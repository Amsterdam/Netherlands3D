using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WFSFeatureDataHandler : MonoBehaviour
{
    public GameObject DataObject { get; private set; }
    public WFSFeatureData FeatureData { get; set; }

    private void Awake()
    {
        DataObject = gameObject;
    }

    public void ShowFeatureData()
    {

    }
    
    public void HideFeatureData()
    {

    }


}
