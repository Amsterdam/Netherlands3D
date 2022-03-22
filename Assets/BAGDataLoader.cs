using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BAGDataLoader : MonoBehaviour
{
    [SerializeField]
    private string bagWFSRequestURL = "";

    [SerializeField]
    private string xmlPostRequest = "";

    [Header("Listen to")]
    [SerializeField]
    private StringListEvent loadBagIDData;


    void Start()
    {
        loadBagIDData.started.AddListener(LoadBag);
    }

	private void LoadBag(List<string> bagIDs)
	{
		if(bagIDs.Count > 0)
        {
            var ID = bagIDs[0];

        }
	}
}
