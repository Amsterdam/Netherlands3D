using UnityEngine;
using System;
namespace Netherlands3D.ModelParsing
{
    [Serializable]
    public class MaterialData
    {
        public string Name;
        public string DisplayName;
        public Color Ambient;
        public Color Diffuse;
        public Color Specular;
        public float Shininess;
        public float Alpha;
        public int IllumType;
        public string DiffuseTexPath;
        public string BumpTexPath;
        [NonSerialized]
        public Texture2D DiffuseTex;
        [NonSerialized]
        public Texture2D BumpTex;

        public void Clear()
        {
            DiffuseTex = null;
            BumpTex = null;
        }
    }

}