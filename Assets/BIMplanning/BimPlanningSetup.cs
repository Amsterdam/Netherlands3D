using Netherlands3D.Timeline;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Globalization;

public class BimPlanningSetup : MonoBehaviour
{

    public Material BuildMaterial;
    public Material DestroyMaterial;
    public TimelineUI timelineUI;

    public TextAsset planningdata;
    // Start is called before the first frame update
    void Start()
    {
        timelineUI.timelineData.timePeriods.Clear();
        string tekst = planningdata.ToString();
        string[] regels = tekst.Split('\n');

        for (int i = 1; i < regels.Length; i++)
        {
            readLine(regels[i]);
        }
        foreach (var child in transform.GetComponentsInChildren<BimPlanningItem>())
        {
            child.Initialize();
        }
        timelineUI.timelineData.OrderTimePeriods();
    }


    void readLine(string regel)
    {
        string[] items = regel.Split(',');
        if (items.Length<6)
        {
            return;
        }
        string displayID = items[0].Replace(' ','_');
        string taskname = items[1];
        string taskType = items[2];
        string datumVan = items[5].ToLower(); ;
        DateTime startdate = new DateTime();
        if (!DateTime.TryParse(datumVan, out startdate))
        {
            return;
        }
        ;
        string datumTot = items[6].ToLower(); ;


        DateTime einddate = new DateTime();
        if (!DateTime.TryParse(datumTot, out einddate))
        {
            return;
        }
        GameObject child = null;
        bool childfound = false;
        int childcount = transform.childCount;
        for (int i = 0; i < childcount; i++)
        {
            if (transform.GetChild(i).gameObject.name == displayID)
            {
                child = transform.GetChild(i).gameObject;
                childfound = true;
                break;
            }
        }
        if (childfound)
        {
            BimPlanningItem bpi = child.GetComponent<BimPlanningItem>();
            if (bpi is null)
            {
                bpi =child.AddComponent<BimPlanningItem>();
                bpi.timelineUI = timelineUI;
                bpi.TaskName = taskname;
            }

            if (taskType=="V")
            {
                bpi.soort = BimPlanningItem.PlanningType.VERVALLEN;
                bpi.highlightMaterialDestroy = DestroyMaterial;
                bpi.DestroystartDateTime = startdate;
                bpi.DestroyeindDateTime = einddate;
            }
            else if (taskType == "N")
            {
                bpi.soort = BimPlanningItem.PlanningType.NIEUW;
                bpi.highlightMaterialBuild = BuildMaterial;
                bpi.BuildstartDateTime = startdate;
                bpi.BuildeindDateTime = einddate;
            }
            
        }
    }

    
    // Update is called once per frame
    void Update()
    {
        
    }
}
