using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[CreateAssetMenu(fileName = "MaterialSettings", menuName = "ScriptableObjects/MaterialSettings", order = 1)]
public class MaterialSettings : ScriptableObject
{
    public Material BaseMaterial;
    public List<material> Colors = new List<material>();
    private Texture2D texturemap;
    // Start is called before the first frame update
   

    // Update is called once per frame


#if UNITY_EDITOR
    void Update()
    {
        bool rebuild = false;
        foreach (material mat in Colors)
        {
            if (mat.changed)
            {
                rebuild = true;
                mat.changed = false;
            }
        }
        if (rebuild)
        {
            updateMaterial();
        }
    }
#endif



    public void updateMaterial()
    {
        
        if (texturemap!=null)
        {
            DestroyImmediate(texturemap,true);
        }
            texturemap = new Texture2D(Colors.Count, 2, TextureFormat.RGBA32, false);

        for (int i = 0; i < Colors.Count; i++)
        {
            //set the color in pixel i,0
            texturemap.SetPixel(i, 0, Colors[i].color);

            //set smootness, metallic,noiseSize and noiseStrength in rgba-channels in pixel i,1
            Color settingspixel = new Color();
            settingspixel.r = Colors[i].smoothness;
            settingspixel.g = Colors[i].metallic;
            settingspixel.b = 1/Colors[i].noiseSize;
            settingspixel.a = Colors[i].noiseStrength;
            texturemap.SetPixel(i, 1, settingspixel);
        }
        texturemap.Apply();
        BaseMaterial.SetTexture("_MappingTexture", texturemap);
    }

    [Serializable]
    public class material
    {
        [HideInInspector]
        public bool changed = false;
        public string description;
        [Header("color Settings")]
        
        public Color color;
        
        [Range(0,1f)]
        public float smoothness;
        [Range(0, 1f)]
        public float metallic;
        [Header ("noise Settings")]
        [Range(1, 100)]
        public float noiseSize=25f;
        [Range(0, 1f)]
        public float noiseStrength=0.5f;
        public material()
        {
            description = "";
            color = new Color();
            smoothness = 0;
            metallic = 0;
            noiseSize = 25;
            noiseStrength = 0.25f;
        }
    }
}

