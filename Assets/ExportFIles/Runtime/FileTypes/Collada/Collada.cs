using Netherlands3D.Core;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Collada : Geometryfile
{
    Vector3 origin = new Vector3();
    bool originisset = false;
    List<Vector3> Coordinates = new List<Vector3>();
    ColladaFile colladaFile;
    string outputfilename;
    public override string filetype
    {
        get { return "collada"; }
    }

    public override void AddMesh(string vertexfile)
    {

        long vertexcount;
        int SingelBytecount = 4;
        long counter = 0;
        Coordinates.Clear();
        string filename = Path.GetFileName(vertexfile);

        string objectname = filename.Replace(".bin", "");
        
        using (var stream = File.Open(vertexfile, FileMode.OpenOrCreate))
        {
            if (stream.Length == 0)
            {
                stream.Close();
                stream.Dispose();
                return;
            }
            vertexcount = (stream.Length / SingelBytecount);
            using (var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, false))
            {
                UnityEngine.Vector3 vert;
                while (counter < vertexcount)
                {
                    if (!originisset)
                    {
                        origin = new UnityEngine.Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        Coordinates.Add(Vector3.zero);
                        counter += 3;
                        originisset = true;
                        continue;

                    }
                    counter += 3;
                     vert = new UnityEngine.Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    Coordinates.Add(vert - origin);


                }
            }
        }
        colladaFile.AddObject(Coordinates, objectname, new Vector4(0.5f,0.5f,0.5f,1f));
    }

    public override void AddMesh(List<Vector3> vertices, string layername, Color color)
    {
        return;
    }

    public override void SaveFile()
    {
        colladaFile.CreateCollada(false, CoordConvert.UnitytoWGS84(origin));
        colladaFile.Save(outputfilename);
    }

    public override void SetupFile(string path, string filename)
    {
        colladaFile = new ColladaFile();
        outputfilename = filename + "dae";
    }

   
}
