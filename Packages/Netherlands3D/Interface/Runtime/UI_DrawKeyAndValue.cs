using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_DrawKeyAndValue : MonoBehaviour
{
    [SerializeField]
    private UI_KeyValuePair keyValuePair;

    [SerializeField]
    private StringListEvent onReceivedKeyValuePair;

    [SerializeField]
    //private 

    void Awake()
    {
        onReceivedKeyValuePair.started.AddListener(DrawKeyValuePair);
    }

	private void DrawKeyValuePair(List<string> keyValuePair)
	{
		
	}

	// Update is called once per frame
	void Update()
    {
        
    }
}
