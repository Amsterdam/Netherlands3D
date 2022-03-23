using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_DestroyChildren : MonoBehaviour
{
	[Header("Listen to")]
	[SerializeField]
	private BoolEvent destroyChildren;

	[SerializeField]
	private bool inverseBoolean = false;

	private void Awake()
	{
		destroyChildren.started.AddListener(DestroyChildren);
	}

	private void DestroyChildren(bool destroy)
	{
		if (destroy || !destroy && inverseBoolean)
		{

			int children = transform.childCount;
			for (int i = children - 1; i > 0; i--)
			{
				Destroy(transform.GetChild(i).gameObject);
			}
		}
	}
}
