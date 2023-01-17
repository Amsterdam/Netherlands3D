using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;


public class Polygon
{
    public string gmlid;
    public bool exterior;
    public List<Vector3> vertices;
    public Dictionary<string, string> attributes = new Dictionary<string, string>();
}

public class SurfaceMember
{
    public string type;
    public double offsetX = 0;
    public double offsetY = 0;
    public double offsetZ = 0;
    public double scaleX = 1;
    public double scaleY = 1;
    public double scaleZ = 1;
    public string nodename;
    public List<Polygon> polygons = new List<Polygon>();
    public Dictionary<string, string> attributes = new Dictionary<string, string>();
    public Dictionary<string, string> MultiSurfaceAttributes = new Dictionary<string, string>();
    public SurfaceMember(XmlNode node, string Type) //read bldg:multisurface
    {
                 
        foreach (XmlAttribute att in node.Attributes)
        {
            attributes.Add(att.LocalName, att.Value);
        }


type = Type;
        nodename = node.Name;
        foreach (XmlNode child in node.ChildNodes)
        {
            switch (child.LocalName)
            {
                case "MultiSurface":
                    ReadMultiSurface(child);
                    break;
                
                default:
                    break;
            }
        }
    
    }
    void ReadMultiSurface(XmlNode node) //read gml:multisurface
    {

        foreach (XmlAttribute att in node.Attributes)
        {
            MultiSurfaceAttributes.Add(att.LocalName, att.Value);
        }

        foreach (XmlNode child in node.ChildNodes)
        {
            switch (child.LocalName)
            {
                case "surfaceMember":
                    ReadSurfaceMember(child);
                    break;
                default:
                    break;
            }
        }
    } 
    void ReadSurfaceMember(XmlNode node) //read gml:surfacemember
    {
        foreach (XmlNode child in node.ChildNodes)
        {
            switch (child.LocalName)
            {
                case "Polygon":
                    ReadPolygon(child);
                    break;
                case "CompositeSurface":
                    ReadMultiSurface(child);
                    break;
                default:
                    break;
            }
        }
    }
    void ReadPolygon(XmlNode node) //read gml:polygon
    {

                 
        

        foreach (XmlNode child in node.ChildNodes)
        {
            switch (child.LocalName)
            {
                case "exterior":
                    Polygon pol = new Polygon();
                    polygons.Add(pol);
                    pol.exterior = true;
                    readGeometry(child,pol);
                    break;
                case "interior":
                    pol = new Polygon();
                    polygons.Add(pol);
                    pol.exterior = false;
                    readGeometry(child, pol);
                    break;
                default:
                    break;
            }
        }
    }
    void readGeometry(XmlNode node, Polygon pol) //read gml: interior and gml:exterior
    {
        foreach (XmlNode child in node.ChildNodes)
        {
            switch (child.LocalName)
            {
                case "LinearRing":
                    readLinearRing(child, pol);
                    break;

                default:
                    break;
            }
        }
    }
    void readLinearRing(XmlNode node, Polygon pol)
    {
        foreach (XmlAttribute att in node.Attributes)
        {
            pol.attributes.Add(att.LocalName, att.Value);
            if (att.LocalName == "id")
            {
                pol.gmlid = att.Value;
            }
        }

        foreach (XmlNode child in node.ChildNodes)
        {
            switch (child.LocalName)
            {
                case "posList":
                    readPosList(child, pol);
                    break;
                case "pos":
                    readPosList(child, pol);
                    break;
                default:
                    break;
            }
        }
    }
    void readPosList(XmlNode node, Polygon pol)
    {
        if (pol.vertices==null)
        {
            pol.vertices = new List<Vector3>();
        }

        Vector3 NieuweVertex = new Vector3();
        string posliststring = node.InnerText;
        double waarde = 0;
        int decimaalplaats = -1;
        int cijferwaarde;
        int aantaldigits = 0;
        int getalnummer = 0;
        bool readExponent = false;
        int exponent = 0;
        for (int i = 0; i < posliststring.Length; i++)
        {
            if (readExponent)
            {
                if (int.TryParse(posliststring[i].ToString(), out cijferwaarde))
                {
                    exponent = cijferwaarde;
                }

            }
            if (posliststring[i].ToString() == ".")
            {
                decimaalplaats = 0;
            }
            else if (posliststring[i].ToString() == "E")
            {
                readExponent = true;
            }
            else
            {
                if (int.TryParse(posliststring[i].ToString(), out cijferwaarde))
                {
                    waarde = (10 * waarde) + cijferwaarde;
                    if (decimaalplaats > -1) { decimaalplaats++; }
                    aantaldigits++;
                }
                else //geen cijfer of punt dus geen getal
                {
                    if (aantaldigits > 0)
                    {
                        if (decimaalplaats > -1)
                        {
                            waarde = waarde / Mathf.Pow(10, decimaalplaats);
                        }
                        if (readExponent)
                        {
                            waarde = waarde * Mathf.Pow(10, exponent);
                            readExponent = false;
                        }

                        switch (getalnummer % 3)
                        {
                            case 0:
                                NieuweVertex = new Vector3();
                                NieuweVertex.x = (float)((waarde*scaleX)-offsetX);
                                break;
                            case 1:
                                NieuweVertex.z = (float)((waarde*scaleY)-offsetY);
                                break;
                            case 2:
                                NieuweVertex.y = (float)((waarde*scaleZ)-offsetZ);
                                pol.vertices.Add(NieuweVertex);
                                break;
                            default:
                                break;
                        }
                        getalnummer++;
                        waarde = 0;
                        decimaalplaats = -1;
                        aantaldigits = 0;
                    }
                }
            }
            

        }

        //waarde berekenen
        if (waarde > 0)
        {
            if (aantaldigits > 0)
            {
                if (decimaalplaats > -1)
                {
                    waarde = waarde / Mathf.Pow(10, decimaalplaats);
                }
                if (readExponent)
                {
                    waarde = waarde * Mathf.Pow(10, exponent);
                    readExponent = false;
                }

                switch (getalnummer % 3)
                {
                    case 0:
                        NieuweVertex = new Vector3();
                        NieuweVertex.x = (float)((waarde * scaleX) - offsetX);
                        break;
                    case 1:
                        NieuweVertex.z = (float)((waarde * scaleY) - offsetY);
                        break;
                    case 2:
                        NieuweVertex.y = (float)((waarde * scaleZ) - offsetZ);
                        pol.vertices.Add(NieuweVertex);
                        break;
                    default:
                        break;
                }
                getalnummer++;
                waarde = 0;
                decimaalplaats = -1;
                aantaldigits = 0;
            }
        }
    }


    public void CreateGameObjects(GameObject parent)
    {
        
        //GameObject go = new GameObject(type);
        //go.transform.parent = parent.transform;
        //if (MultiSurfaceAttributes.ContainsKey("id"))
        //{
        //    ObjectProperties op = go.AddComponent<ObjectProperties>();
        //    op.gmlID = MultiSurfaceAttributes["id"];
        //}

        //foreach (Polygon bs in polygons)
        //{
        //    GameObject go2 = new GameObject();
        //    go2.transform.parent = go.transform;

        //    if (bs.attributes.ContainsKey("id"))
        //    {
        //        ObjectProperties op = go2.AddComponent<ObjectProperties>();
        //        op.gmlID = bs.attributes["id"];
        //    }

        //    MeshFilter mf = go2.AddComponent<MeshFilter>();
        //    mf.mesh = CreateMesh(bs.vertices);
        //    mf.mesh.name = bs.gmlid;
        //    MeshRenderer mr = go2.AddComponent<MeshRenderer>();
        //    mr.sharedMaterial = GameObject.Find("CityGMLLoader").GetComponent<MeshRenderer>().sharedMaterial;
        //}
    }

     Mesh CreateMesh(List<Vector3> vertices)
    {
        Mesh Outputmesh = new Mesh();

        if (vertices.Count > 3)
        {
            vertices.RemoveAt(vertices.Count - 1);
        }

        
        Vector3 offsetVector = new Vector3(0, 0, 0);
        //vertexoffset helsinki
        offsetVector = new Vector3(25490540, 0, 6665458);
        //vectoroffset amsterdam
        //Vector3 offsetVector = new Vector3(121000, 0, 487000);
        for (int i = 0; i < vertices.Count; i++)
        {

            Vector3 vert = vertices[i];

            vert = vert - offsetVector;
            vertices[i] = vert;
        }


        List<Vector2> v2s = RotateFace(vertices);

        
        Triangulator trimaker = new Triangulator(v2s.ToArray());
        int[] triangles = trimaker.Triangulate();


        int aantalTriangles = triangles.Length;
        List<int> triangleList = new List<int>() ;



        for (int i = 0; i < aantalTriangles; i += 3)
        {
            int vert2 = triangles[i + 1];
            triangleList.Add(triangles[i]);
            triangleList.Add(triangles[i + 2]);
            triangleList.Add(vert2);
        }


        Outputmesh.vertices = vertices.ToArray();
        Outputmesh.triangles = triangleList.ToArray();
        Outputmesh.RecalculateNormals();
        Outputmesh.Optimize();

        ////gespiegelde mesh maken
        //Mesh OppositeMesh = Outputmesh;
        //int[] Oppositetriangles = OppositeMesh.triangles;
        //for (int i = 0; i < Oppositetriangles.Length; i+=3)
        //{
        //    int n1 = Oppositetriangles[i + 1];
        //    Oppositetriangles[i + 1] = Oppositetriangles[i + 2];
        //    Oppositetriangles[i + 2] = n1;
        //}
        //OppositeMesh.triangles = Oppositetriangles;
        //OppositeMesh.RecalculateNormals();
        ////gespeigelde triangles tovoegen aan originele mesh;
        //List<int> alletriangles = new List<int>();
        //int[] origineletriangles = Outputmesh.triangles;
        //for (int i = 0; i < origineletriangles.Length; i++)
        //{
        //    alletriangles.Add(origineletriangles[i]);
        //}
        //for (int i = 0; i < Oppositetriangles.Length; i++)
        //{
        //    alletriangles.Add(Oppositetriangles[i]);
        //}
        //List<Vector3> normals = new List<Vector3>();
        //Vector3[] origineleNormals = Outputmesh.normals;
        //Vector3[] oppositeNormals = OppositeMesh.normals;
        //for (int i = 0; i < origineleNormals.Length; i++)
        //{
        //    normals.Add(origineleNormals[i]);
        //}
        //for (int i = 0; i < oppositeNormals.Length; i++)
        //{
        //    normals.Add(oppositeNormals[i]);
        //}
        //Outputmesh.triangles = alletriangles.ToArray();
        //Outputmesh.normals = normals.ToArray();


        return Outputmesh;

    }
    List<Vector2> RotateFace(List<Vector3> vertices)
    {
        List<Vector2> Vec2List = new List<Vector2>();

        //get Normal
        Vector3 Normal = new Vector3();

        Vector3 U = vertices[1] - vertices[0];
        Vector3 V = vertices[2] - vertices[0];

        Normal.x = (U.y * V.z) - (U.z * V.y);
        Normal.y = (U.z * V.x) - (U.x * V.z);
        Normal.z = (U.x * V.y) - (U.y * V.x);

        Vector3 verticaal = new Vector3(0, 1, 0);

        Quaternion rot = Quaternion.FromToRotation(Normal,verticaal);
        Matrix4x4 matrix4 = Matrix4x4.Rotate(rot);

        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 rotated = matrix4*vertices[i];
            Vec2List.Add(new Vector2(rotated.x, rotated.z));
        }
        

        return Vec2List;
    }

}


public class Triangulator
{
    private List<Vector2> m_points = new List<Vector2>();

    public Triangulator(Vector2[] points)
    {
        m_points = new List<Vector2>(points);
    }

    public int[] Triangulate()
    {
        List<int> indices = new List<int>();

        int n = m_points.Count;
        if (n < 3)
            return indices.ToArray();

        int[] V = new int[n];
        if (Area() > 0)
        {
            for (int v = 0; v < n; v++)
                V[v] = v;
        }
        else
        {
            for (int v = 0; v < n; v++)
                V[v] = (n - 1) - v;
        }

        int nv = n;
        int count = 2 * nv;
        for (int v = nv - 1; nv > 2;)
        {
            if ((count--) <= 0)
                return indices.ToArray();

            int u = v;
            if (nv <= u)
                u = 0;
            v = u + 1;
            if (nv <= v)
                v = 0;
            int w = v + 1;
            if (nv <= w)
                w = 0;

            if (Snip(u, v, w, nv, V))
            {
                int a, b, c, s, t;
                a = V[u];
                b = V[v];
                c = V[w];
                indices.Add(a);
                indices.Add(b);
                indices.Add(c);
                for (s = v, t = v + 1; t < nv; s++, t++)
                    V[s] = V[t];
                nv--;
                count = 2 * nv;
            }
        }

        indices.Reverse();
        return indices.ToArray();
    }

    private float Area()
    {
        int n = m_points.Count;
        float A = 0.0f;
        for (int p = n - 1, q = 0; q < n; p = q++)
        {
            Vector2 pval = m_points[p];
            Vector2 qval = m_points[q];
            A += pval.x * qval.y - qval.x * pval.y;
        }
        return (A * 0.5f);
    }

    private bool Snip(int u, int v, int w, int n, int[] V)
    {
        int p;
        Vector2 A = m_points[V[u]];
        Vector2 B = m_points[V[v]];
        Vector2 C = m_points[V[w]];
        if (UnityEngine.Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
            return false;
        for (p = 0; p < n; p++)
        {
            if ((p == u) || (p == v) || (p == w))
                continue;
            Vector2 P = m_points[V[p]];
            if (InsideTriangle(A, B, C, P))
                return false;
        }
        return true;
    }

    private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
    {
        float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
        float cCROSSap, bCROSScp, aCROSSbp;

        ax = C.x - B.x; ay = C.y - B.y;
        bx = A.x - C.x; by = A.y - C.y;
        cx = B.x - A.x; cy = B.y - A.y;
        apx = P.x - A.x; apy = P.y - A.y;
        bpx = P.x - B.x; bpy = P.y - B.y;
        cpx = P.x - C.x; cpy = P.y - C.y;

        aCROSSbp = ax * bpy - ay * bpx;
        cCROSSap = cx * apy - cy * apx;
        bCROSScp = bx * cpy - by * cpx;

        return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
    }
}
