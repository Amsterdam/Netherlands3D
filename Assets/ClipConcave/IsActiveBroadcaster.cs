using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;

public class IsActiveBroadcaster : MonoBehaviour
{
    [SerializeField] BoolEvent boolEvent;
    [SerializeField] GameObjectEvent gameobjectEnabled;
    [SerializeField] GameObjectEvent gameobjectDisabled;
    // Start is called before the first frame update

    private void OnEnable()
    {
        if (gameobjectEnabled)
        {
            gameobjectEnabled.started.Invoke(this.gameObject);
        }
        if (boolEvent)
        {
            boolEvent.started.Invoke(true);
        }
    }
    private void OnDisable()
    {
        if (gameobjectDisabled)
        {
            gameobjectDisabled.started.Invoke(this.gameObject);
        }
        if (boolEvent)
        {
            boolEvent.started.Invoke(false);
        }
    }

    void Start()
    {
        if (boolEvent)
        {
            boolEvent.started.Invoke(isActiveAndEnabled);
        }
       
            if (isActiveAndEnabled)
            {
                if (gameobjectEnabled)
                    {
                        gameobjectEnabled.started.Invoke(this.gameObject);
                    }
            }
            else
        {
            if (gameobjectDisabled)
            {
                gameobjectDisabled.started.Invoke(this.gameObject);
            }
        }
           
        }
    }


