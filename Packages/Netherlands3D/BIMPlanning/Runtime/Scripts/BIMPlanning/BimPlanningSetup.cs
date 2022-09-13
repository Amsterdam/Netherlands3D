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
        private TimelineUI timeline;

        /// <summary>
        /// Data object for parsed Navisworks CSV lines
        /// </summary>
        public class BIMPlanningData
        {
            public string displayID;
            public string taskname;
            public string taskType;
            public string dateFrom;
            public string dateTo;

            public DateTime startDate;
            public DateTime endDate;
        }

        private List<BIMPlanningData> BIMPlanningDataList = new List<BIMPlanningData>();

        private string displayIDColumnHeader = "displayID";
        private string taskNameColumnHeader = "taskName";
        private string taskTypeColumnHeader = "taskType";
        private string fromDateColumnHeader = "fromDate";
        private string toDateColumnHeader = "toDate";

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
        public List<BIMPlanningData> ReadPlanningFromCSV(string csvPath, string displayIDColumnHeader, string taskNameColumnHeader, string taskTypeColumnHeader, string fromDateColumnHeader, string toDateColumnHeader)
        {
            this.displayIDColumnHeader = displayIDColumnHeader;
            this.taskNameColumnHeader = taskNameColumnHeader;
            this.taskTypeColumnHeader = taskTypeColumnHeader;
            this.fromDateColumnHeader = fromDateColumnHeader;
            this.toDateColumnHeader = toDateColumnHeader;

            BIMPlanningDataList = new List<BIMPlanningData>();
            BIMPlanningDataList.Clear();
            timeline.timelineData.timePeriods.Clear();

            var csvText = File.ReadAllText(csvPath);
            string[] lines = csvText.Split('\n');

            if (!VerifyCSVData(lines))
            {
                Debug.Log("Navisworks CSV could not be verified", this.gameObject);
                return null;
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

            return BIMPlanningDataList;
        }

        /// <summary>
        /// Check if lines appear to be a Navisworks CSV.
        /// </summary>
        /// <param name="csvLines">The array of csv lines</param>
        /// <returns></returns>
        private bool VerifyCSVData(string[] csvLines)
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
            BIMPlanningDataList.Add(newPlanningData);
        }
    }
}