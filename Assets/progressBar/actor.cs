using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.ModelParsing;
public class actor : MonoBehaviour
{
    [SerializeField] OBJLoadConnector connector;
    // Start is called before the first frame update
    private void Awake()
    {
        
    }

    void StartImport()
    {
        connector.numberOfConnections += 1;
    }

    private void OnDisable()
    {
        connector.request.RequestTestConnection.RemoveListener(StartImport);
        

    }
    private void OnEnable()
    {
        connector.request.RequestTestConnection.AddListener(StartImport);
        

    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
