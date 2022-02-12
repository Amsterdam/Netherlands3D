using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerMaterials : MonoBehaviour
{
    public Material BaseMaterial;
    public List<Color> Colors = new List<Color>();
    // Start is called before the first frame update
    void Start()
    {
        Texture2D texturemap = new Texture2D(16, 1,TextureFormat.RGBA32,false);
        for (int i = 0; i < Colors.Count; i++)
        {
            texturemap.SetPixel(i, 0, Colors[i]);
        }
        texturemap.Apply();
        BaseMaterial.SetTexture("_MappingTexture", texturemap);
    }

    
}
