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
using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using static Netherlands3D.BIMPlanning.BimPlanningSetup;

namespace Netherlands3D.BIMPlanning
{
    public class BimPlanningImporter : MonoBehaviour
    {
        [Header("Required input")]
        [SerializeField] Material baseMaterial;
        [SerializeField] private StringEvent objFileSelected;
        [SerializeField] private StringEvent mtlFileSelected;
        [SerializeField] private StringEvent csvFileSelected;

        [SerializeField] private Material BuildMaterial;
        [SerializeField] private Material DestroyMaterial;

        BimPlanningSetup csvImporter;
        ObjImporter.ObjImporter objImporter;

        [Header("Optional output")]
        [SerializeField] private BoolEvent onObjReady;
        [SerializeField] private BoolEvent onMtlReady;
        [SerializeField] private BoolEvent onCsvReady;

        [SerializeField] private BoolEvent onCsvVerified;

        [SerializeField] private BoolEvent onReadyForImports;

        private List<BIMPlanningData> BIMPlanningDataList = new List<BIMPlanningData>();

        //Requirements matched
        private bool objReady = false;
        private bool csvReady = false;

        //Optional
        private bool mtlReady = false;

        public bool ObjReady
        {
            get => objReady;
            set { objReady = value; if(onObjReady) onObjReady.InvokeStarted(objReady); }
        }
        public bool CsvReady
        {
            get => csvReady;
            set { csvReady = value; if (onCsvReady) onCsvReady.InvokeStarted(csvReady); }
        }
        public bool MtlReady {
            get => mtlReady;
            set { mtlReady = value; if (onMtlReady) onMtlReady.InvokeStarted(mtlReady); }
        }

        public void StartImport()
        {
            objImporter.StartImporting(OnOBJImported);

            ObjReady = false;
            MtlReady = false;
            onReadyForImports.InvokeStarted(false);
        }

        /// <summary>
        /// Add the imported object as child and try to link it to the parsed planning data
        /// </summary>
        private void OnOBJImported(GameObject returnedGameObject)
        {
            returnedGameObject.transform.SetParent(this.transform);

            FindPlanningGameObjects();
        }

        private void OnEnable()
        {
            objFileSelected.AddListenerStarted(OnOBJFileSelected);
            mtlFileSelected.AddListenerStarted(OnOBJFileSelected);
            csvFileSelected.AddListenerStarted(OnCSVileSelected);
        }
        private void OnDisable()
        {
            objFileSelected.RemoveListenerStarted(OnOBJFileSelected);
            mtlFileSelected.RemoveListenerStarted(OnOBJFileSelected);
            csvFileSelected.RemoveListenerStarted(OnCSVileSelected);
        }

        private void OnOBJFileSelected(string value)
        {
            AddImporters();

            if (value == "") return;

            var files = value.Split(',');
            foreach (var filePath in files)
            {
                var extention = Path.GetExtension(filePath);
                switch (extention)
                {
                    case ".obj":
                        ObjReady = true;
                        objImporter.objFilePath = filePath;
                        break;
                    case ".mtl":
                        MtlReady = true;
                        objImporter.mtlFilePath = filePath;
                        break;
                }
            }

            onReadyForImports.InvokeStarted(ObjReady && CsvReady);
        }

        private void OnCSVileSelected(string value)
        {
            AddImporters();

            if (value == "") return;

            var files = value.Split(',');
            foreach (var filePath in files)
            {
                var extention = Path.GetExtension(filePath);
                switch (extention)
                {
                    case ".csv":
                        ReadPlanning(filePath);
                        break;
                    default:
                        //Ignore other extentions
                        break;
                }
            }

            onReadyForImports.InvokeStarted(ObjReady && CsvReady);
        }

        private void AddImporters()
        {
            if (!csvImporter) csvImporter = gameObject.AddComponent<BimPlanningSetup>();
            if (!objImporter)
            {
                objImporter = gameObject.AddComponent<ObjImporter.ObjImporter>();
                objImporter.BaseMaterial = baseMaterial;
                objImporter.createSubMeshes = false;
            }
        }

        private void ReadPlanning(string filePath)
        {
            BIMPlanningDataList = csvImporter.ReadPlanningFromCSV(filePath,
                            "displayID",
                            "taskName",
                            "taskType",
                            "fromDate",
                            "toDate"
                           );
            if (BIMPlanningDataList != null && BIMPlanningDataList.Count > 0)
            {
                CsvReady = true;
            }

            onCsvVerified.InvokeStarted(csvReady);
        }

        /// <summary>
        /// For all planning data lines, try to find a child gameobject with matching ID and connect it to the data.
        /// </summary>
        public void FindPlanningGameObjects()
        {
            foreach (var planningData in BIMPlanningDataList)
            {
                SetPlanningObjects(planningData);
            }
        }

        private void SetPlanningObjects(BIMPlanningData planningData)
        {
            //All children that match ID get linked to planning data
            var childcount = transform.childCount;
            int matchedObjects = 0;
            for (int i = 0; i < childcount; i++)
            {
                var child = transform.GetChild(i);
                if (child.gameObject.name == planningData.displayID)
                {
                    SetAsPlanningObject(child.gameObject, planningData.taskname, planningData.taskType, planningData.startDate, planningData.endDate);
                    matchedObjects++;
                }
            }

            if (matchedObjects == 0) Debug.Log("");
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
}
