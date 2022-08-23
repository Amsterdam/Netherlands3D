using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Core;

abstract public class Geometryfile
{
    public abstract void SetupFile(string path, string filename);
    public abstract void AddMesh(string vertexfile);
    public abstract void AddMesh(List<Vector3> vertices, string layername, Color color);
    public abstract void SaveFile();
    public abstract string filetype
    { get; }
    
}
