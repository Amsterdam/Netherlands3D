using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class LayerMaterials : MonoBehaviour
{
    public MaterialSettings materialSettings;
   
    // Start is called before the first frame update
    void Start()
    {
        materialSettings.updateMaterial();
    }
#if UNITY_EDITOR
    private void Update()
    {
        materialSettings.updateMaterial();
    }
#endif
}

