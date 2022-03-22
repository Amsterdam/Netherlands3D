using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputEvents : MonoBehaviour
{
    [Header("Invoke events")]
    [SerializeField]
    private Vector3Event clickOnScreenPosition;
    [SerializeField]
    private Vector3Event secondaryClickOnScreenPosition;

    void Start()
    {
        
    }

    void Update()
    {
        if (!IsOverInterface())
        {
            if (Input.GetMouseButtonDown(0))
            {
                clickOnScreenPosition.Invoke(Input.mousePosition);
            }
            else if (Input.GetMouseButtonDown(1))
            {
                secondaryClickOnScreenPosition.Invoke(Input.mousePosition);
            }
        }
    }

    private bool IsOverInterface()
    {
        if (!EventSystem.current) return false;
        if (EventSystem.current.IsPointerOverGameObject()) return true;
        return false;
    }
}
