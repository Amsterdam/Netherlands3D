using Netherlands3D.Timeline;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Globalization;
using Netherlands3D.Events;
using System.IO;

namespace Netherlands3D.BIMPlanning
{
    /// <summary>
    /// This object reads Navisworks planning files (as .csv)
    /// And tries to link child objects (with matching ID's) to DateTime's.
    /// This allows showing/hiding/coloring of the objects based on the time.
    /// </summary>
    public class BimPlanningSetup : MonoBehaviour
    {
        public Material BuildMaterial;
        public Material DestroyMaterial;

        private TimelineUI timeline;

        private void Start()
        {
            timeline = FindObjectOfType<TimelineUI>(true);
            if(timeline == null)
            {
                Debug.LogWarning("BimPlanningSetup requires a TimelineUI in your scene.",this.gameObject);
            }
        }

        public void OnEnable()
        {
            
        }
        public void OnDisable()
        {

        }

        /// <summary>
        /// Read all text from CSV planning file.
        /// A specific order is assumed based on Navisworks export: displayID,taskName,taskType,fromDate,toDate
        /// </summary>
        /// <param name="csvPath">Path to the CSV file</param>
        public void ReadPlanningFromCSV(string csvPath)
        {
            timeline.timelineData.timePeriods.Clear();

            var csvText = File.ReadAllText(csvPath);
            string[] lines = csvText.Split('\n');

            for (int i = 1; i < lines.Length; i++)
            {
                ParseLine(lines[i]);
            }

            foreach (var child in transform.GetComponentsInChildren<BimPlanningItem>())
            {
                child.Initialize(timeline);
            }

            timeline.timelineData.OrderTimePeriods();
        }

        private void ParseLine(string line)
        {
            string[] items = line.Split(',');
            if (items.Length < 6)
            {
                Debug.Log("Could not parse Navisworks CSV. Not enough columns in CSV.", this.gameObject);
                return;
            }

            var displayID = items[0].Replace(' ', '_');
            var taskname = items[1];
            var taskType = items[2];
            var dateFrom = items[5].ToLower();
            var dateTo = items[6].ToLower();

            var startDate = new DateTime();
            var endDate = new DateTime();
            if (!DateTime.TryParse(dateFrom, out startDate) || !DateTime.TryParse(dateTo, out endDate))
            {
                Debug.Log("Could not parse DateTime from CSV line: {line}", this.gameObject);
                return;
            }

            bool childfound = GetChildWithID(displayID, out GameObject child);
            if (childfound)
            {
                AddPlanningItemToGameObject(child, taskname, taskType, startDate, endDate);
            }
        }

        /// <summary>
        /// Find a child object which name matches the display ID
        /// </summary>
        /// <param name="displayID">ID of the object</param>
        /// <param name="child">Output of the child if it is found</param>
        /// <returns></returns>
        private bool GetChildWithID(string displayID, out GameObject child)
        {
            child = null;
            var childcount = transform.childCount;
            for (int i = 0; i < childcount; i++)
            {
                if (transform.GetChild(i).gameObject.name == displayID)
                {
                    child = transform.GetChild(i).gameObject;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Add a planning item component on a corresponding GameObject
        /// </summary>
        private void AddPlanningItemToGameObject(GameObject child, string taskname, string taskType, DateTime startDate, DateTime endDate)
        {
            BimPlanningItem bimPlanningItem = child.GetComponent<BimPlanningItem>();
            if (bimPlanningItem is null)
            {
                bimPlanningItem = child.AddComponent<BimPlanningItem>();
                bimPlanningItem.TaskName = taskname;
            }
            if (taskType == "V")
            {
                bimPlanningItem.planningType = BimPlanningItem.PlanningType.REMOVED;
                bimPlanningItem.HighlightMaterialDestroy = DestroyMaterial;
                bimPlanningItem.DestroyStartDateTime = startDate;
                bimPlanningItem.DestroyEndDateTime = endDate;
            }
            else if (taskType == "N")
            {
                bimPlanningItem.planningType = BimPlanningItem.PlanningType.NEW;
                bimPlanningItem.HighlightMaterialBuild = BuildMaterial;
                bimPlanningItem.BuildStartDateTime = startDate;
                bimPlanningItem.BuildEndDateTime = endDate;
            }
        }
    }
}