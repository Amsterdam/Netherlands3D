using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.BIMPlanning
{
    public class BimPlanningImporter : MonoBehaviour
    {
        [SerializeField] private StringEvent fileSelected;

        BimPlanningSetup csvImporter;
        void Start()
        {
            if(!csvImporter) csvImporter = gameObject.AddComponent<BimPlanningSetup>();
        }

        private void OnEnable()
        {
            fileSelected.started.AddListener(OnFileSelected);
        }

        private void OnFileSelected(string value)
        {
            var files = value.Split(',');
            foreach(var file in files)
            {
                //check csv extention
            }
        }

        void Combine()
        {
            //got obj?

            //got csv?


            //Create layer event

        }
    }
}