using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Netherlands3D.Timeline;

namespace Netherlands3D.BIMPlanning
{
    public class BimPlanningItem : MonoBehaviour
    {
        public PlanningType planningType = PlanningType.NEW;

        public string TaskName;

        public DateTime BuildStartDateTime;
        public DateTime BuildEndDateTime;
        public DateTime DestroyStartDateTime;
        public DateTime DestroyEndDateTime;

        public Material HighlightMaterialBuild;
        public Material HighlightMaterialDestroy;

        private Material[] originalMaterials;
        private Material[] highlightMaterialsBuild;
        private Material[] highlightMaterialsDestroy;

        private MeshRenderer meshRenderer;

        public enum PlanningType
        {
            REMOVED,
            NEW
        }

        public void Initialize(TimelineUI timeline)
        {
            originalMaterials = this.gameObject.GetComponent<MeshRenderer>().materials;
            highlightMaterialsBuild = new Material[originalMaterials.Length];
            highlightMaterialsDestroy = new Material[originalMaterials.Length];
            for (int i = 0; i < highlightMaterialsBuild.Length; i++)
            {
                highlightMaterialsBuild[i] = HighlightMaterialBuild;
                highlightMaterialsDestroy[i] = HighlightMaterialDestroy;
            }
            meshRenderer = this.gameObject.GetComponent<MeshRenderer>();
            timeline.onCurrentDateChange.AddListener(this.OnDateChange);

            TimePeriod timePeriod = new TimePeriod("", "", BuildStartDateTime, DestroyEndDateTime, TaskName);
            timeline.timelineData.AddTimePeriod(timePeriod, false);
        }

        public void OnDateChange(DateTime date)
        {
            meshRenderer.enabled = true;
            meshRenderer.materials = originalMaterials;

            if (BuildStartDateTime != null)
            {
                if (date <= BuildStartDateTime)
                {
                    meshRenderer.enabled = false;
                    return;
                }
                if (date <= BuildEndDateTime)
                {

                    meshRenderer.materials = highlightMaterialsBuild;
                    return;
                }
            }
            if (DestroyStartDateTime != null)
            {
                if (date > DestroyEndDateTime)
                {
                    meshRenderer.enabled = false;
                    return;
                }
                if (date >= DestroyStartDateTime)
                {
                    meshRenderer.enabled = true;
                    meshRenderer.materials = highlightMaterialsDestroy;
                    return;
                }
            }
        }
    }
}