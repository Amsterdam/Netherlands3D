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
        float[] BiglistVectorFloatArray = new float[1024*3];
        int baseindex;
        float[] vectorFloatArray = new float[3];
        byte[] vectorBinaryArray = new byte[12];
        
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
            writer = new BinaryWriter(File.Open(filepath, FileMode.Create,FileAccess.Write,FileShare.None));



        }
       
        public void Add(float v1, float v2, float v3)
        {
            
            vectorFloatArray[0] = v1;
            vectorFloatArray[1] = v2;
            vectorFloatArray[2] = v3;
            System.Buffer.BlockCopy(vectorFloatArray, 0, vectorBinaryArray, 0, 12);
            writer.Write(vectorBinaryArray);
            vectorCount += 1;
        }
        public void EndWriting()
        {
            
            writer.Close();
            writer = null;
        }
        public void SetupReading(string name = "")
        {
            readBytes = new byte[12*1024];
            if (name !="")
            {
                filepath = Application.persistentDataPath + "/" + name + ".dat";
            }
            reader = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.None, 12);
            //reader = File.OpenRead(filepath);
            baseindex = -1;

        }
        public Vector3 ReadItem(int index)
        {
            bool readNewBatch = false;
            if (baseindex == -1)
            {
                readNewBatch = true;
            }
            else if (index < baseindex)
            {
                readNewBatch = true;
            }
            else if (index >= baseindex + 1024)
            {
                readNewBatch = true;
            }

            if (readNewBatch)
            {
                reader.Position = index * 12;
                baseindex = index;
                int count = reader.Read(readBytes, 0, 1024 * 12);
                //System.Buffer.BlockCopy(readBytes, 0, BiglistVectorFloatArray, 0, 1024*12);
            }
            //reader.Position = index * 12;
            Vector3 ReturnItem = new Vector3();
            //reader.Read(readBytes, 0, 12);
            int startindex = index - baseindex;
            System.Buffer.BlockCopy(readBytes, startindex*12, BiglistVectorFloatArray, 0, 12);
            
            ReturnItem.x = BiglistVectorFloatArray[0];
            ReturnItem.y = BiglistVectorFloatArray[1];
            ReturnItem.z = BiglistVectorFloatArray[2];


            return ReturnItem;
        }
        public void EndReading()
        {
            reader.Close();
            reader = null;
            
        }
        public void RemoveData()
        {
            File.Delete(filepath);
        }
    }
}
