using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
namespace Netherlands3D.ModelParsing
{
    public class SubMeshRawData
    {
        BinaryWriter writer;
        FileStream fs;
        BinaryReader bReader;

        int[] intArray = new int[3];
        byte[] intBinaryArray = new byte[12];

        string datapath = "";
        string filepath;
        string filename;
        public void SetupWriting(string name)
        {

            filepath = Application.persistentDataPath + "/" + name + ".dat";
            // create the file if it doesnt already exist
            var datafile = File.Exists(filepath) ? File.Open(filepath, FileMode.Append) : File.Open(filepath, FileMode.CreateNew);
            datafile.Close();
            writer = new BinaryWriter(File.Open(filepath, FileMode.Append,FileAccess.Write,FileShare.None));
        }
        public void Add(int vertexIndex, int normalIndex, int textureIndex)
        {
            intArray[0] = vertexIndex;
            intArray[1] = normalIndex;
            intArray[2] = textureIndex;

            System.Buffer.BlockCopy(intArray, 0, intBinaryArray, 0, 12);
            writer.Write(intBinaryArray);

           // writer.Write(vertexIndex);
           // writer.Write(normalIndex);
           // writer.Write(textureIndex);
        }
        public void EndWriting()
        {
            if (writer !=null)
            {
            writer.Flush();
            writer.Close();
            writer = null;
            }

        }
        public void SetupReading(string name)
        {
            if (datapath=="")
            {
                datapath = Application.persistentDataPath;
            }
          filepath = datapath + "/" + name + ".dat";

            fs = File.OpenRead(filepath);
            bReader = new BinaryReader(fs);
        }

        public void RemoveData(string filename)
        {
            filepath = Application.persistentDataPath + "/" + filename + ".dat";
            EndWriting();
            if (File.Exists(filepath))
            {
                File.Delete(filepath);
            }
            
        }

        public int numberOfVertices()
        {
            return (int)fs.Length / 12;
        }
        public Vector3Int ReadNext()
        {
            Vector3Int output = new Vector3Int();
            output.x = bReader.ReadInt32();
            output.y = bReader.ReadInt32();
            output.z = bReader.ReadInt32();
            return output;
        }
        public void EndReading()
        {
            bReader.Close();
            fs.Close();

        }
    }
}
