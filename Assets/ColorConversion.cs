using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorConversion : MonoBehaviour
{
    public static Color32 IntToColor(int aCol)
    {
        Color32 c = new Color32();
        c.b = (byte)((aCol) & 0xFF);
        c.g = (byte)((aCol >> 8) & 0xFF);
        c.r = (byte)((aCol >> 16) & 0xFF);
        c.a = (byte)((aCol >> 24) & 0xFF);
        return c;
    }
    
    [ContextMenu("Apply color gradient on X axis")]
    public void ApplyVertexColor()
    {
        var mesh = this.GetComponent<MeshFilter>().mesh;
        var vertexCount = mesh.vertexCount;
        var vertices = mesh.vertices;

        Color32[] colors = new Color32[vertexCount];
		for (int i = 0; i < vertexCount; i++)
		{
            colors[i] = IntToColor(Mathf.RoundToInt(vertices[i].x+ vertices[i].y+ vertices[i].z * 1000.0f));
        }

        mesh.colors32 = colors;
	}
}
