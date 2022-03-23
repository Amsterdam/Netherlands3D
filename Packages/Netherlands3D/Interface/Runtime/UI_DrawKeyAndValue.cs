using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_DrawKeyAndValue : MonoBehaviour
{
    [SerializeField]
    private UI_KeyValuePair keyValuePairTemplate;

    [Header("Listen to")]
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
        var newKeyValuePair = Instantiate(keyValuePairTemplate, keyValuePairTemplate.transform.parent);
        newKeyValuePair.SetValues(keyValuePair[0], keyValuePair[1]);
        newKeyValuePair.gameObject.SetActive(true);
    }
}
