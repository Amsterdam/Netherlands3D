/*
*  Copyright (C) X Gemeente
*                X Amsterdam
*                X Economic Services Departments
*
*  Licensed under the EUPL, Version 1.2 or later (the "License");
*  You may not use this work except in compliance with the License.
*  You may obtain a copy of the License at:
*
*    https://github.com/Amsterdam/Netherlands3D/blob/main/LICENSE.txt
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" basis,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
*  implied. See the License for the specific language governing
*  permissions and limitations under the License.
*/
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
    /// This object reads planning .CSV files ( Based on Navisworks export )
    /// and can link child objects (with matching ID's) to the parsed DateTime's.
    /// This allows showing/hiding/coloring of the objects based on the chosen time in a Timeline.
    /// </summary>
    public class BimPlanningSetup : MonoBehaviour
    {
        public Material BuildMaterial;
        public Material DestroyMaterial;

        private TimelineUI timeline;

        private List<BIMPlanningData> BIMPlanningDataLine = new List<BIMPlanningData>();

        /// <summary>
        /// Data object for parsed Navisworks CSV lines
        /// </summary>
        private class BIMPlanningData
        {
            public string displayID;
            public string taskname;
            public string taskType;
            public string dateFrom;
            public string dateTo;

            public DateTime startDate;
            public DateTime endDate;
        }

        private void Start()
        {
            timeline = FindObjectOfType<TimelineUI>(true);
            if(timeline == null)
            {
                Debug.LogWarning("BimPlanningSetup requires a TimelineUI in your scene.",this.gameObject);
            }
        }

        /// <summary>
        /// Read all text from CSV planning file.
        /// A specific order is assumed based on Navisworks export: displayID,taskName,taskType,fromDate,toDate
        /// </summary>
        /// <param name="csvPath">Path to the CSV file</param>
        public void ReadPlanningFromCSV(string csvPath)
        {
            BIMPlanningDataLine.Clear();
            timeline.timelineData.timePeriods.Clear();

            var csvText = File.ReadAllText(csvPath);
            string[] lines = csvText.Split('\n');

            if (!VerifyNavisworksCSV(lines))
            {
                Debug.Log("Navisworks CSV could not be verified", this.gameObject);
                return;
            }

            for (int i = 1; i < lines.Length; i++)
            {
                ParseLine(lines[i]);
            }

            foreach (var child in transform.GetComponentsInChildren<BimPlanningItem>())
            {
                child.Initialize(timeline);
            }
            timeline.timelineData.OrderTimePeriods();

            FindPlanningGameObjects();
        }

        /// <summary>
        /// Check if lines appear to be a Navisworks CSV.
        /// </summary>
        /// <param name="csvLines">The array of csv lines</param>
        /// <returns></returns>
        private bool VerifyNavisworksCSV(string[] csvLines)
        {
            if (csvLines.Length < 2)
                return false;
            //TODO: We might want to add more checks here, probably on CSV header (csvLines[0])
            return true;
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

            var newPlanningData = new BIMPlanningData()
            {
                displayID = displayID,
                taskname = taskname,
                taskType = taskType,
                dateFrom = dateFrom,
                dateTo = dateTo,
                startDate = startDate,
                endDate = endDate,
            };
            BIMPlanningDataLine.Add(newPlanningData);

            FindPlanningGameObjects();
        }

        /// <summary>
        /// For all planning data lines, try to find a child gameobject with matching ID and connect it to the data.
        /// </summary>
        public void FindPlanningGameObjects()
        {
            foreach (var planningData in BIMPlanningDataLine)
            {
                bool childfound = GetChildWithID(planningData.displayID, out GameObject child);
                if (childfound)
                {
                    SetAsPlanningObject(child, planningData.taskname, planningData.taskType, planningData.startDate, planningData.endDate);
                }
            }
        }

        /// <summary>
        /// Add a planning item component on a corresponding GameObject
        /// </summary>
        private void SetAsPlanningObject(GameObject child, string taskname, string taskType, DateTime startDate, DateTime endDate)
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

        /// <summary>
        /// Find a child object of this transform which name matches the display ID
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
    }
}