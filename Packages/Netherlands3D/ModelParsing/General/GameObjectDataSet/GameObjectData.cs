using System.Collections;
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

        public void Clear()
        {
            meshdata.Clear();
        }
    }

}