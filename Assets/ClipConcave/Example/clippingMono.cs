using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class clippingMono : MonoBehaviour
{
    #region variqbles for testing
    [SerializeField] LineRenderer clipPolyLine; //linerenderer with the boundary
    [SerializeField] LineRenderer trianglePolyLine; //linerenderer with the triangle

    [SerializeField] GameObject resultPolyLine; //gameobject with linerenderer as template for resulting polygons

#endregion

    List<GameObject> resultPolygonGameObjects = new List<GameObject>();

    // persistent list that contains the triangle-Vertexlist
    // clear it before you fill it with a new triangle.
    // this way the garbage-collector doesn't have to remove the old list(which happens met you repace it ith a new list). 
    List<Vector3> triangle = new List<Vector3>();

    // persistent list that will receive the result form the clipper
    // clear it before you ask for a new result.
    List<Vector3> result = new List<Vector3>();

    //clipper-class
    //the clipperclass is re-usable. ie you can replace the boundary-polygon.
    public ClipConcave clipper;

    // Start is called before the first frame update
    void Start()
    {
        //create the clipper
         clipper = new ClipConcave();

        //create a Vector2-list for the boundary (last and first vertex should not be the same);
        List<Vector2> boundary = new List<Vector2>();
        ReadClipLineRenderer(clipPolyLine, boundary);
        //send the boundary to the clipper
        clipper.setBoundary(boundary); 
    }



    // Update is called once per frame
    void Update()
    {
        // remove the drawn result from the last run
        foreach (var item in resultPolygonGameObjects)
        {
            Destroy(item);
        }

        //clear the Vector3-list with triangle-coordinates.
        triangle.Clear();
        // fill the list with the coordiantes of the new triangle
        readTriangleLineRenderer(trianglePolyLine, triangle);

       
        // tell the clipper to prepare for clipping the triangle
        // it immediately tells you if there will be geometry left after clipping
       bool geometryAvailable = clipper.setTriangle(triangle);


            while (geometryAvailable) // keep requesting polygons while they are available
            {
                geometryAvailable = clipper.FindNextPolygon(result); //ask for the next polygon, while updating the availablity of even more geometry
                DrawPolygon(result); //do something with the received Polygon
            }

    }


    #region creating test output
    void DrawPolygon(List<Vector3> polygon)
    {
        GameObject resultObject = Instantiate(resultPolyLine);
        LineRenderer lr = resultObject.GetComponent<LineRenderer>();
        lr.positionCount = polygon.Count;
        lr.SetPositions(polygon.ToArray());
        resultPolygonGameObjects.Add(resultObject);
    }
    #endregion


    #region interfacing testinput
    void ReadClipLineRenderer(LineRenderer lr, List<Vector2> result)
    {
        Transform tr = lr.gameObject.transform;
        Vector2 worldposition = new Vector2(tr.position.x, tr.position.z);
        for (int i = 0; i < lr.positionCount; i++)
        {
            result.Add(worldposition+new Vector2(lr.GetPosition(i).x, lr.GetPosition(i).z));
        }
    }
    void readTriangleLineRenderer(LineRenderer lr, List<Vector3> result)
    {
        for (int i = 0; i < lr.positionCount; i++)
        {
            result.Add(lr.gameObject.transform.position+lr.GetPosition(i));
        }
    }
    #endregion




}



