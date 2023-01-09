using System.Collections.Generic;
using System;
using UnityEngine;
namespace Netherlands3D.ModelParsing
{
    [Serializable]
    public class MeshData
    {
        public string name;
        public string vertexFileName;
        public string normalsFileName;
        public string indicesFileName;
        public string uvFileName;
        public List<SubMeshData> submeshes = new List<SubMeshData>();

        public void Clear()
        {
            vertexFileName = Application.persistentDataPath + "/"+ vertexFileName + ".dat";
            if (System.IO.File.Exists(vertexFileName))
            {
                System.IO.File.Delete(vertexFileName);
            }
            normalsFileName = Application.persistentDataPath + "/" + normalsFileName+ ".dat";
            if (System.IO.File.Exists(normalsFileName))
            {
                System.IO.File.Delete(normalsFileName);
            }
            indicesFileName = Application.persistentDataPath + "/" + indicesFileName + ".dat";
            if (System.IO.File.Exists(indicesFileName))
            {
                System.IO.File.Delete(indicesFileName);
            }

            uvFileName = Application.persistentDataPath + "/" + uvFileName + ".dat";
            if (System.IO.File.Exists(uvFileName))
            {
                System.IO.File.Delete(uvFileName);
            }


            submeshes.Clear();

        }
    }

}