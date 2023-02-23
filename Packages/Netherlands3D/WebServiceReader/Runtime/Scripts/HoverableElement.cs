using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;
public class HoverableElement : MonoBehaviour
{
    [SerializeField] private BoolEvent onHover;

    private void OnMouseEnter()
    {
        if (onHover)
            onHover.Invoke(true);
    }

    private void OnMouseExit()
    {
        if (onHover)
            onHover.Invoke(false);
    }
}
