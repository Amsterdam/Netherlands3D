using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Netherlands3D.Timeline;

public class BimPlanningItem : MonoBehaviour
{

    
    public PlanningType soort = PlanningType.NIEUW;

    public TimelineUI timelineUI;
    public string TaskName;


    public DateTime BuildstartDateTime;
    public DateTime BuildeindDateTime;
    public DateTime DestroystartDateTime;
    public DateTime DestroyeindDateTime;
    bool highlighted = false;
    Material[] originalMaterials;
    public Material highlightMaterialBuild;
    Material[] highlightMaterialsBuild;
    public Material highlightMaterialDestroy;
    Material[] highlightMaterialsDestroy;

    MeshRenderer mr;
    // Start is called before the first frame update
    public void Initialize()
    {

        originalMaterials = this.gameObject.GetComponent<MeshRenderer>().materials;
        highlightMaterialsBuild = new Material[originalMaterials.Length];
        highlightMaterialsDestroy = new Material[originalMaterials.Length];
        for (int i = 0; i < highlightMaterialsBuild.Length; i++)
        {
            highlightMaterialsBuild[i] = highlightMaterialBuild;
            highlightMaterialsDestroy[i] = highlightMaterialDestroy;
        }
        mr = this.gameObject.GetComponent<MeshRenderer>();
        timelineUI.onCurrentDateChange.AddListener(OnDateChange);
        
        TimePeriod tp = new TimePeriod("", "", BuildstartDateTime, DestroyeindDateTime, TaskName);
        
        timelineUI.timelineData.timePeriods.Add(tp);
    }
       
     public void OnDateChange(DateTime date)
    {
        //enable or disable;
        mr.enabled = true;
        mr.materials = originalMaterials;

        if (BuildstartDateTime!=null)
        {
            if (date <= BuildstartDateTime)
            {
                mr.enabled = false;
                return;
            }
            if (date <= BuildeindDateTime)
            {

                mr.materials = highlightMaterialsBuild;
                highlighted = true;
                return;
            }
        }
        if (DestroystartDateTime!=null)
        {
            if (date > DestroyeindDateTime)
            {
                mr.enabled = false;
                return;
            }
            if (date >= DestroystartDateTime)
            {
                mr.enabled = true;
                mr.materials = highlightMaterialsDestroy;
                highlighted = true;
                return;
            }
        }
        
        
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public enum PlanningType
    {
        VERVALLEN,
        NIEUW
    }
}
