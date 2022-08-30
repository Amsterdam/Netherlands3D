using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace Netherlands3D.ModelParsing
{
    [Serializable]
    public class GameObjectData
    {
        public string name;
        public MeshData meshdata;
        public bool positionIsDefined = true;
        public Vector3 localPosition = Vector3.zero;
        public Vector3 localRotation = Vector3.zero;
        public Vector3 localScale = Vector3.one;
    }

    [Serializable]
    public class GameObjectDataSet
    {
        public List<MaterialData> materials = new List<MaterialData>();
        public List<GameObjectData> gameObjects = new List<GameObjectData>();
    }



    [Serializable]
    public class MeshData
    {
        public string name;
        public string vertexFileName;
        public string normalsFileName;
        public string indicesFilename;
        public string uvFileName;
        public List<SubMeshData> submeshes = new List<SubMeshData>();
    }

    [Serializable]
    public class SubMeshData
    {
        public long vertexOffset = 0;
        public long startIndex;
        public long Indexcount;
        public string materialname;
    }

    [Serializable]
    public class MaterialData
    {
        public string Name;
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
    }

}