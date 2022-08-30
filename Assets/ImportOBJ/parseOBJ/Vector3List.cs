using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
namespace Netherlands3D.ModelParsing
{
    public class Vector3List
{
       BinaryWriter writer;
       FileStream reader;
       
        byte[] readBytes;
        private int vectorCount = 0;
        string filepath;

        public int Count()
        {
            if (reader is null)
            {
                return vectorCount;
            }
            else
            {

           
            return  (int)reader.Length / 12;
            }
        }
        public void SetupWriting(string name)
        {
            filepath = Application.persistentDataPath + "/" + name + ".dat";
            writer = new BinaryWriter(File.Open(filepath, FileMode.Create));

        }
       
        public void Add(float v1, float v2, float v3)
        {
            //int value = (int)(1000 * v1);
            writer.Write(v1);
            //value = (int)(1000 * v2);
            writer.Write(v2);
            //value = (int)(1000 * v3);
            writer.Write(v3);
            vectorCount += 1;
        }
        public void EndWriting()
        {
            
            writer.Close();
        }
        public void SetupReading(string name = "")
        {
            readBytes = new byte[12];
            if (name !="")
            {
                filepath = Application.persistentDataPath + "/" + name + ".dat";
            }
            reader = File.OpenRead(filepath);

        }
        public Vector3 ReadItem(int index)
        {

            Vector3 ReturnItem = new Vector3();
            long startindex = (long)index * 12;
            //reader.Seek(startindex,SeekOrigin.Begin);
            //reead bytes
            reader.Position = startindex;
            int bytesRead = reader.Read(readBytes,0,12);
            
            ReturnItem.x = System.BitConverter.ToSingle(readBytes, 0);
            ReturnItem.y = System.BitConverter.ToSingle(readBytes, 4);
            ReturnItem.z = System.BitConverter.ToSingle(readBytes, 8);


            return ReturnItem;
        }
        public void EndReading()
        {
            reader.Close();
        }
        public void RemoveData()
        {
            File.Delete(filepath);
        }
    }
}
