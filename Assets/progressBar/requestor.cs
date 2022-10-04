using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.ModelParsing;
public class requestor : MonoBehaviour
{
    [SerializeField] OBJLoadConnector connector;
    // Start is called before the first frame update

    bool acknowledged = false;

    void OnEnable()
    {
        if (connector.isBusy)
        {
            Debug.Log("tool is Busy, please wait for it to finish");
            return;
        }
        connector.request.RequestTestConnection.Invoke();
        
        if (connector.numberOfConnections==0)
        { 
            Debug.LogError("Nobody available to de the work");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
