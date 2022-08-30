using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;

namespace Netherlands3D.ModelParsing
{
    public class CreateMeshesFromOBJ : MonoBehaviour
    {
        const string glyphs = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"; //add the characters you want
        public bool createSingelMesh = true;

        public Vector3List vertices;
        public Vector3List normals;
        public List<Submesh> submeshes;

        public FloatEvent progressPercentage;
        int totalSubmeshCount = 0;
        int currentSubmeshindex = 0;

        Vector3List meshVertices;
        Vector3List meshNormals;
        intList meshIndices;

        System.DateTime time;

        int nextindex = 0;

        GameObjectDataSet container = new GameObjectDataSet();


        bool meshDataCreated = false;
        MeshData createdMeshData;
        bool submeshCreated = false;
        SubMeshData createdSubMeshData;
        System.Action<GameObjectDataSet> callback;

        public void CreateGameObjectDataSet(System.Action<GameObjectDataSet> callbacktoFunction,bool createMultipleObjects=false)
        {
            totalSubmeshCount = submeshes.Count;
            currentSubmeshindex = 0;
            callback = callbacktoFunction;
            if (createMultipleObjects)
            {
                StartCoroutine(CreateSingleSubmesh());
            }
            else
            {
                StartCoroutine(CreateMultiSubmesh());
            }
        }

        void Finish()
        {
            vertices.RemoveData();
            normals.RemoveData();
            callback(container);
        }

        IEnumerator CreateMultiSubmesh()
        {
            GameObjectData childObject = new GameObjectData();
            childObject.name = submeshes[0].name;
            meshDataCreated = false;
            StartCoroutine(CreateMeshData(submeshes));
            while (meshDataCreated == false)
            {
                yield return null;
            }
            childObject.meshdata = createdMeshData;
            container.gameObjects.Add(childObject);
            Finish();
        }

        IEnumerator CreateSingleSubmesh()
        {
            time = System.DateTime.Now;
            List<Submesh> submesheslist = new List<Submesh>();
            foreach (var submesh in submeshes)
            {
                submesheslist.Add(submesh);
                GameObjectData childObject = new GameObjectData();
                childObject.name = submesh.name;
                meshDataCreated = false;
                StartCoroutine(CreateMeshData(submesheslist));
                while (meshDataCreated==false)
                {
                    yield return null;
                }
                childObject.meshdata = createdMeshData;
                submesheslist.Clear();
                container.gameObjects.Add(childObject);
            }
            Finish();
        }

       IEnumerator CreateMeshData(List<Submesh> submeshes)
        {
            meshVertices = new Vector3List();
            string meshvertexname = randomString(20);
            meshNormals = new Vector3List();
            string meshnormalname = randomString(20);
            meshVertices.SetupWriting(meshvertexname);
            meshNormals.SetupWriting(meshnormalname);
            meshIndices = new intList();
            string meshindicesname = randomString(20);
            meshIndices.SetupWriting(meshindicesname);

            createdMeshData = new MeshData();
            createdMeshData.name = submeshes[0].name;
            createdMeshData.vertexFileName = meshvertexname;
            createdMeshData.normalsFileName = meshnormalname;
            createdMeshData.indicesFilename = meshindicesname;
            nextindex = 0;
            foreach (var submesh in submeshes)
            {
                submeshCreated = false;
                StartCoroutine(CreateSubMeshData(submesh));
                while (submeshCreated==false)
                {
                    yield return null;
                }
                createdMeshData.submeshes.Add(createdSubMeshData);
            }
            meshVertices.EndWriting();
            meshNormals.EndWriting();
            meshIndices.EndWriting();

            meshDataCreated = true;
        }
        IEnumerator CreateSubMeshData(Submesh submesh)
        {
            currentSubmeshindex++;
            createdSubMeshData = new SubMeshData();
            SubMeshRawData smrd = submesh.rawData;
            createdSubMeshData.materialname = submesh.name;
            smrd.SetupReading();
            vertices.SetupReading();
            normals.SetupReading();
            long indexcount = smrd.numberOfVertices();
            createdSubMeshData.startIndex = nextindex;
            for (int i = 0; i < indexcount; i++)
            {
                if ((System.DateTime.Now-time).TotalMilliseconds>400)
                {
                    if(progressPercentage!=null)
                    {
                        float size = 1f / totalSubmeshCount;
                        float progress = size* i / indexcount;
                        progressPercentage.started.Invoke(100 * (((currentSubmeshindex-1)*size)+progress));
                    }
                    yield return null;
                    time = System.DateTime.Now;
                }
                Vector3Int data = smrd.ReadNext();
                Vector3 vertex = vertices.ReadItem(data.x);
                Vector3 normal = normals.ReadItem(data.y);
                meshVertices.Add(vertex.x, vertex.y, vertex.z);
                meshNormals.Add(normal.x, normal.y, normal.z);
                meshIndices.Add(nextindex++);
            }
            createdSubMeshData.Indexcount = nextindex - createdSubMeshData.startIndex;
            
            smrd.EndReading();
            smrd.RemoveData();
            vertices.EndReading();
            normals.EndReading();

            submeshCreated = true;
        }

        string randomString(int length)
        {
            string returnstring = "";
            for (int i = 0; i < length; i++)
            {
                returnstring += glyphs[Random.Range(0, glyphs.Length)];
            }
            return returnstring;
        }

    }
}