using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_EnableGameObject : MonoBehaviour
{
	[Header("Listen to")]
	[SerializeField]
	private BoolEvent enableGameObject;

	[SerializeField]
	private bool disableOnStart = false;

	private void Awake()
	{
		enableGameObject.started.AddListener(EnableGameObject);
	}

	private void EnableGameObject(bool enable)
	{
		this.gameObject.SetActive(enable);
	}

	private void Start()
	{
		if(disableOnStart){
			this.gameObject.SetActive(false);
		}
	}
}
