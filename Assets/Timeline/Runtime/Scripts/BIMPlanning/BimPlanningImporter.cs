using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.BIMPlanning
{
    public class BimPlanningImporter : MonoBehaviour
    {
        [SerializeField] private StringEvent fileSelected;

        BimPlanningSetup csvImporter;
        BimPlanningSetup objImporter;

        void Start()
        {
            if(!csvImporter) csvImporter = gameObject.AddComponent<BimPlanningSetup>();
        }

        private void OnEnable()
        {
            fileSelected.started.AddListener(OnFileSelected);
        }
        private void OnDisable()
        {
            fileSelected.started.RemoveListener(OnFileSelected);
        }

        private void OnFileSelected(string value)
        {
            if (value == "") return;

            var files = value.Split(',');
            foreach(var filePath in files)
            {
                if(Path.GetExtension(filePath) == ".csv")
                {
                    //Read CSV using Navisworks .csv export based column headers
                    csvImporter.ReadPlanningFromCSV(filePath,
                        "displayID",
                        "taskName",
                        "taskType",
                        "fromDate",
                        "toDate"
                    );
                }
            }
        }

        private void Combine()
        {
            //got obj?

            //got csv?


            //Create layer event

        }
    }
}