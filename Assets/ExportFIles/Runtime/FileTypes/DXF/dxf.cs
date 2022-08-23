using Netherlands3D.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using netDxf;
using netDxf.Entities;
using netDxf.Tables;
using System.IO;

namespace Netherlands3D.FileExport.DXF
{
    public class dxf : Geometryfile
    {

        private DxfDocument dxfDocument;
        private string fullPath;
        List<Vector3RD> RDCoordinates = new List<Vector3RD>();

        public override string filetype
        {
            get { return "dxf"; }
        }

        public override void SetupFile(string path, string filename)
        {
            fullPath = Path.Combine(path, $"{filename}.dxf");
            dxfDocument = new DxfDocument();
            dxfDocument.DrawingVariables.InsUnits = netDxf.Units.DrawingUnits.Meters;
        }

        public override void AddMesh(List<UnityEngine.Vector3> vertices, string layername, Color color)
        {
            Layer dxfLayer = new Layer(layername);
            //translate the color
            byte r = (byte)(color.r * 255);
            byte g = (byte)(color.g * 255);
            byte b = (byte)(color.b * 255);
            dxfLayer.Color = new netDxf.AciColor(r, g, b);

            dxfDocument.Layers.Add(dxfLayer);

            //AddMesh(vertices, dxfLayer);
        }

        public override void SaveFile()
        {
            dxfDocument.Save(fullPath, true);
        }

        private void AddMesh(List<Vector3RD> triangleVertices, Layer dxfLayer)
        {
            PolyfaceMesh pfm;
            // create Mesh
            List<netDxf.Vector3> pfmVertices = new List<netDxf.Vector3>();
            pfmVertices.Capacity = triangleVertices.Count;
            List<PolyfaceMeshFace> pfmFaces = new List<PolyfaceMeshFace>();
            pfmFaces.Capacity = triangleVertices.Count / 3;
            int facecounter = 0;
            Debug.Log(triangleVertices.Count);
            int vertexIndex = 0;

            for (int i = 0; i < triangleVertices.Count-3; i += 3)
            {


                pfmVertices.Add(new netDxf.Vector3(triangleVertices[i].x, triangleVertices[i].y, triangleVertices[i].z));
                pfmVertices.Add(new netDxf.Vector3(triangleVertices[i + 2].x, triangleVertices[i + 2].y, triangleVertices[i + 2].z));
                pfmVertices.Add(new netDxf.Vector3(triangleVertices[i + 1].x, triangleVertices[i + 1].y, triangleVertices[i + 1].z));

                PolyfaceMeshFace pfmFace = new PolyfaceMeshFace(new List<short>() { (short)(vertexIndex + 1), (short)(vertexIndex + 2), (short)(vertexIndex + 3) });
                vertexIndex += 3;
                pfmFaces.Add(pfmFace);
                facecounter++;
                if (facecounter % 10000 == 0)
                {
                    pfm = new PolyfaceMesh(pfmVertices, pfmFaces);
                    pfm.Layer = dxfLayer;
                    dxfDocument.Entities.Add(pfm);
                    pfmVertices.Clear();
                    pfmFaces.Clear();
                    facecounter = 0;
                    vertexIndex = 0;
                }
            }
            if (pfmFaces.Count > 0)
            {
                pfm = new PolyfaceMesh(pfmVertices, pfmFaces);
                pfm.Layer = dxfLayer;
                dxfDocument.Entities.Add(pfm);
            }


        }

        /// <summary>
        /// reads the coordinate-list in the file and turns it into dxf-meshes
        /// </summary>
        /// <param name="vertexfile"></param>
        public override void AddMesh(string vertexfile)
        {

            long vertexcount;
            int SingelBytecount = 4;
            long counter = 0;
                RDCoordinates.Clear();
            string filename = Path.GetFileName(vertexfile);

                Layer dxfLayer = new Layer(filename.Replace(".bin", ""));
                dxfLayer.Color = netDxf.AciColor.LightGray;
                using (var stream = File.Open(vertexfile, FileMode.OpenOrCreate))
                {
                vertexcount = (stream.Length / SingelBytecount);
                    using (var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, false))
                    {
                    while (counter < vertexcount)
                    {

                        counter += 3;
                        UnityEngine.Vector3 vert = new UnityEngine.Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        Vector3RD RDvert = CoordConvert.UnitytoRD(vert);
                        RDCoordinates.Add(RDvert);
                    }
                    }
                }
                AddMesh(RDCoordinates, dxfLayer);

        }
    }
}