using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;

public class ClickableElement : MonoBehaviour
{
    [SerializeField] private TriggerEvent onClicked;
    private void OnMouseDown()
    {
        if (onClicked)
            onClicked.InvokeStarted();
    }
}

