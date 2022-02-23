using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.VISSIM
{
    /// <summary>
    /// For testing/interacting with the VISSIM Manager within the Unity editor enviroment
    /// </summary>
    public class VISSIMTesting : MonoBehaviour
    {
        [Header("Values")]
        [Tooltip("Set a limit on how long the VISSIM.Datas list can be")]
        [SerializeField] private int maxVISSIMDatasLength = 1000;

        void Start()
        {
            if(!Application.isEditor) return;

            VISSIMManager.MaxDatasCount = maxVISSIMDatasLength;
        }
    }
}
