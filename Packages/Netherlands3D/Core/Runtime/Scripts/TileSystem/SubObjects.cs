using Netherlands3D;
using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public partial class SubObjects : MonoBehaviour
{
    [SerializeField]
    private List<SubOjectData> subObjectsData;
    public List<SubOjectData> SubObjectsData { get => subObjectsData; private set => subObjectsData = value; }
    public bool Altered { get; private set; }
    public Vector2Int TileKey
    {
        get
        {
            var parts = gameObject.name.Split('-'); //todo: replace this with more elegant solution
            var x = int.Parse(parts[0]);
            var y = int.Parse(parts[1]);
            return new Vector2Int(x, y);
        }
    }
    public static string removeFromID = "NL.IMBAG.Pand.";
    public static string brotliExtention = ".br";

    private Mesh mesh;
    private Color[] vertexColors;

    private bool downloadingSubObjects = false;

    private Coroutine runningColoringProcess;

    private void Awake()
    {
        SubObjectsData = new List<SubOjectData>();
        mesh = this.GetComponent<MeshFilter>().sharedMesh;
        vertexColors = new Color[mesh.vertexCount];
        for (int i = 0; i < vertexColors.Length; i++)
        {
            vertexColors[i] = Color.white;
        }
    }

    public void Select(int selectedVert, System.Action<string> callback)
    {
        //Select using vertex index, or download metadata first
        if (SubObjectsData.Count > 0 && !downloadingSubObjects)
        {
            callback(GetIDByVertexIndex(selectedVert));
        }
        else
        {
            downloadingSubObjects = false;
            StartCoroutine(
                LoadMetaDataAndSelect(selectedVert, callback)
            );
        }
    }

    private void ReadMetaDataFile(byte[] results)
    {
        using (var stream = new MemoryStream(results))
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                var version = reader.ReadInt32();
                var subObjects = reader.ReadInt32();
                for (int i = 0; i < subObjects; i++)
                {
                    var id = reader.ReadString();
                    var firstIndex = reader.ReadInt32();
                    var indicesCount = reader.ReadInt32();
                    var firstVertex = reader.ReadInt32();
                    var vertexCount = reader.ReadInt32();
                    var subMeshID = reader.ReadInt32();

                    if (removeFromID.Length > 0)
                        id = id.Replace(removeFromID, "");

                    SubObjectsData.Add(new SubOjectData()
                    {
                        objectID = id,
                        firstIndex = firstIndex,
                        indicesCount = indicesCount,
                        firstVertex = firstVertex,
                        verticesLength = vertexCount,
                        subMeshID = subMeshID
                    });
                }
            }
        }
    }

    [ContextMenu("Copy ID's to clipboard")]
    private void CopyIDsToClipBoard()
    {
        string csvString = "";
        foreach (var subObject in SubObjectsData)
        {
            var randomData = ColorUtility.ToHtmlStringRGB(new Color(Random.value, Random.value, Random.value));
            csvString += subObject.objectID + ";#" + randomData + "\n";
        }

        GUIUtility.systemCopyBuffer = csvString;
        Debug.Log("Copied ID's");
    }

    private IEnumerator LoadMetaDataAndSelect(int selectedVertAfterLoading, System.Action<string> callback)
    {
        yield return LoadMetaData(mesh);

        var id = GetIDByVertexIndex(selectedVertAfterLoading);
        callback(id);
    }

    public IEnumerator LoadMetaDataAndApply(List<SubOjectData> prevousObjectDatas = null)
    {
        yield return LoadMetaData(mesh);

        if (prevousObjectDatas == null) yield break;

        //Sync parts of the data with our new objectdata 
        foreach (var subObject in SubObjectsData)
        {
            foreach (var previousSubObject in prevousObjectDatas)
            {
                if (subObject.objectID == previousSubObject.objectID)
                {
                    subObject.color = previousSubObject.color;
                    subObject.hidden = previousSubObject.hidden;
                }
            }
        }

        //Apply colors/hidden etc back to new geometry+data
        for (int i = 0; i < SubObjectsData.Count; i++)
        {
            var subObject = SubObjectsData[i];
            for (int j = 0; j < subObject.verticesLength; j++)
            {
                vertexColors[subObject.firstVertex + j] = subObject.color;
            }
        }
        mesh.colors = vertexColors;
    }

    private IEnumerator LoadMetaData(Mesh mesh)
    {
        if (!mesh) yield break;

        downloadingSubObjects = true;

        var metaDataName = mesh.name.Replace(".bin", "-data.bin");

        var webRequest = UnityWebRequest.Get(metaDataName);

        yield return webRequest.SendWebRequest();

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("No metadata on path: " + metaDataName);
            downloadingSubObjects = false;
        }
        else
        {
            if (gameObject == null) yield return null;

            byte[] results = webRequest.downloadHandler.data;
            ReadMetaDataFile(results);
            downloadingSubObjects = false;
            yield return null;
        }
        yield return null;
    }

    /// <summary>
    /// Get the ID of the object belonging to this vertex index
    /// </summary>
    /// <param name="vertexIndex">Vertex index</param>
    /// <returns></returns>
    public string GetIDByVertexIndex(int vertexIndex)
    {
        //Find all subobject ranges, and color the vertices at those indices
        for (int i = 0; i < SubObjectsData.Count; i++)
        {
            var subObject = SubObjectsData[i];
            if (vertexIndex >= subObject.firstVertex && vertexIndex < subObject.firstVertex + subObject.verticesLength)
            {
                return subObject.objectID;
            }
        }
        return "";
    }

    public void ColorAll(Color highlightColor)
    {
        Altered = true;

        for (int i = 0; i < SubObjectsData.Count; i++)
        {
            var subObject = SubObjectsData[i];
            subObject.color = highlightColor;
            for (int j = 0; j < subObject.verticesLength; j++)
            {
                vertexColors[subObject.firstVertex + j] = subObject.color;
            }
        }

        mesh.colors = vertexColors;
    }

    public void ColorObjectsByID(Dictionary<string, Color> idColors, Color defaultColor)
    {
        if (runningColoringProcess != null)
        {
            StopCoroutine(runningColoringProcess);
            runningColoringProcess = null;
        }

        runningColoringProcess = StartCoroutine(LoadAndColorByID(idColors, defaultColor));
    }

    private IEnumerator LoadAndColorByID(Dictionary<string, Color> idColors, Color defaultColor)
    {
        if (SubObjectsData.Count == 0)
            yield return LoadMetaData(mesh);

        if (this.gameObject == null || mesh == null) yield break;

        Altered = true;

        //Find all subobject ranges, and color the verts at those indices
        for (int i = 0; i < SubObjectsData.Count; i++)
        {
            var subObject = SubObjectsData[i];
            Color color;
            if (!idColors.ContainsKey(subObject.objectID))
            {
                color = defaultColor;
            }
            else
            {
                color = idColors[subObject.objectID];
            }
            subObject.color = color;

            for (int j = 0; j < subObject.verticesLength; j++)
            {
                vertexColors[subObject.firstVertex + j] = color;
            }
        }

        mesh.colors = vertexColors;
    }

    public void ColorWithIDs(List<string> ids, Color highlightColor)
    {
        Altered = true;

        //Find all subobject ranges, and color the verts at those indices
        for (int i = 0; i < SubObjectsData.Count; i++)
        {
            var subObject = SubObjectsData[i];
            if (ids.Contains(subObject.objectID))
            {
                subObject.color = highlightColor;
            }
            else if (!subObject.hidden)
            {
                subObject.color = Color.white;
            }
            for (int j = 0; j < subObject.verticesLength; j++)
            {
                vertexColors[subObject.firstVertex + j] = subObject.color;
            }
        }
        mesh.colors = vertexColors;
    }

    public void HideWithIDs(List<string> ids)
    {
        Altered = true;

        if (runningColoringProcess != null)
        {
            StopCoroutine(runningColoringProcess);
            runningColoringProcess = null;
        }

        runningColoringProcess = StartCoroutine(LoadAndHideWithIDs(ids));
    }

    private IEnumerator LoadAndHideWithIDs(List<string> ids)
    {
        Altered = true;

        if (SubObjectsData.Count == 0)
            yield return LoadMetaData(mesh);

        for (int i = 0; i < SubObjectsData.Count; i++)
        {
            var subObject = SubObjectsData[i];
            if (ids.Contains(subObject.objectID))
            {
                subObject.hidden = true;
                subObject.color = new Color(subObject.color.r, subObject.color.g, subObject.color.b, 0.0f);
            }
            else
            {
                subObject.hidden = false;
                subObject.color = new Color(subObject.color.r, subObject.color.g, subObject.color.b, 1.0f);
            }
            for (int j = 0; j < subObject.verticesLength; j++)
            {
                vertexColors[subObject.firstVertex + j] = subObject.color;
            }
        }
        mesh.colors = vertexColors;
    }

    public void ResetColors()
    {
        Altered = false;

        for (int i = 0; i < SubObjectsData.Count; i++)
        {
            var subObject = SubObjectsData[i];
            subObject.color = Color.white;
            subObject.hidden = false;
            for (int j = 0; j < subObject.verticesLength; j++)
            {
                vertexColors[subObject.firstVertex + j] = subObject.color;
            }
        }
        mesh.colors = vertexColors;
    }

    public void DrawVertexColorsAccordingToHeight(float max, Color minColor, Color maxColor)
    {
        Altered = true;

        Vector3[] vertices = mesh.vertices;

        //Find all subobject ranges, and color the verts at those indices
        for (int i = 0; i < SubObjectsData.Count; i++)
        {
            var subObject = SubObjectsData[i];
            Color randomColor;

            //determine height of vertex
            float height = 0.0f;
            for (int h = 0; h < subObject.indicesLength; h++)
            {
                if (vertices[h].y > height) height = vertices[h].y;
            }
            randomColor = Color.Lerp(minColor, maxColor, height / max);

            //apply color
            for (int j = 0; j < subObject.verticesLength; j++)
            {
                vertexColors[subObject.firstVertex + j] = randomColor;
            }
        }

        mesh.colors = vertexColors;
    }

    [ContextMenu("Generate random vertex colors")]
    public void DrawRandomVertexColors()
    {
        Altered = true;

        //Find all subobject ranges, and color the verts at those indices
        for (int i = 0; i < SubObjectsData.Count; i++)
        {
            var subObject = SubObjectsData[i];
            Color randomColor = new Color(Random.value, Random.value, Random.value, 1.0f);
            subObject.color = randomColor;

            for (int j = 0; j < subObject.verticesLength; j++)
            {
                vertexColors[subObject.firstVertex + j] = randomColor;
            }
        }

        mesh.colors = vertexColors;
    }
}
