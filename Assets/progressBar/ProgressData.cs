using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Netherlands3D.ModelParsing;

[CreateAssetMenu(fileName = "ProgressData", menuName = "ScriptableObjects/ProgressData", order = 1)]
public class ProgressData : ScriptableObject
{
    [SerializeField] private bool _busy;
    public bool busy
    {
        set {
            _busy = value;
            statusChanged.Invoke(value);
        }
        get
        {
            return _busy;
        }
    }
    
    public string currentAction;
    public string currentActivity;
    public float progressPercentage;
    public bool showProgressPercentage;

    [HideInInspector]
    public UnityEvent<bool> statusChanged;



}
