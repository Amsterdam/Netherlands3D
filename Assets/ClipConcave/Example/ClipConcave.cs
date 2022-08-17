using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class ClipConcave
{
    #region private variables
    //reference to the input
    
    bool triangleIsFlipped = false; // if true, triangleList is reversed to be able to clip upSideDown-triangle. output will be flipped again to restore the original upsidedown-ness 
    bool GeometryAvailable = true;

    //objectpools
    SectionPool sectionpool = new SectionPool();
    IntersectionPool intersectionpool = new IntersectionPool();

    //definining properties of the plane of the triangle, to be able to calculate the elevation of points on the boundary
    planeVariables planeValues = new planeVariables();

    //Bounds of the boundary and Triangle, for a quick check if the triangle is outside the boundary
    Bounds BoundaryBounds;
    Bounds TriangleBounds;

    List<Section> BoundarySections = new List<Section>();
    List<Section> TriangleSections = new List<Section>();

    List<Intersection> intersections = new List<Intersection>();
    int UnusedIntersections;

    //pointlists with all the points (original and intersection) on boundary and triangle
    List<point> TrianglePoints = new List<point>();
    List<point> BoundaryPoints = new List<point>();
    #endregion

    #region structs
    struct planeVariables
    {
        public Vector3 normal;
        public Vector3 pointOnPlane;
    }
    struct point
    {
        public Vector2 position2D;
        public Vector3 position3D;
        public bool switchPoly;
        public int switchToIndex;
        public bool visited;
        public Intersection intersection;//used to find to which index on the other pointlist the switch has to be made.
                                         //needs to be a reference because the index might only become known after this point is created.
    }

    #endregion

    #region public functions
    /// <summary>
    /// sets the boundary with which the triangles should be clipped
    /// polygon MUST be CCW
    /// </summary>
    /// <param name="boundaryCoordinates"></param>
    public void setBoundary(List<Vector2> boundaryCoordinates)
    {

        if (boundaryCoordinates.Count < 3)
        {
            Debug.Log("no valid triangle set for ClipConcave.cs");
            
        }

        FillBoundarySectionList(boundaryCoordinates, BoundarySections);
        DetermineBoundaryBounds(boundaryCoordinates);
    }

    /// <summary>
    /// sets triangle that should be clipped.
    /// polygon MUST be convex.
    /// polygon must be CCW. polygon CAN be upside-down.
    /// </summary>
    /// <param name="triangleCoordinates"></param>
    /// <returns>true if triangle overlaps with Boundary</returns>
    public bool setTriangle(List<Vector3> triangleCoordinates) // returns true if the trianglecan be clipped
    {
        // if the Triangle consists of less than 3 coordinates, clipping is impossible
        if (triangleCoordinates.Count < 3)
        {
            Debug.Log("no valid triangle set for ClipConcave.cs");
            return false;
        }
        //check if the boundingBox of the triangle overlaps with the boundingbox of the boundary.
        //if they don't overlap there wont be anyting to clip, so we are basically finished.
        GeometryAvailable = DoesTriangleBoundsOverlapWithBoundaryBounds(triangleCoordinates);
        if (GeometryAvailable == false)
        {
            return false;
        }
        //calculate the location and orientation of the triangle
        calculatePlaneVariables(triangleCoordinates);

        //check triangle-normal and flip the listdirection if needed
        triangleIsFlipped = false;
        if (planeValues.normal.y<0f) // triangle is upside-Down
        {
            triangleCoordinates.Reverse();
            calculatePlaneVariables(triangleCoordinates);
            triangleIsFlipped = true;
        }

        //remove the intersections from the boundarysections and set the sectionDirection based on the triangleDirection
        ResetBoundarySections();

        FillTriangleSectionsList(triangleCoordinates, TriangleSections);
        FindIntersections();
        CreatePointLists();
        LinkPointLists();

        if (triangleIsFlipped)
        {
            //flip them back so the sender still has the unaltered input
            triangleCoordinates.Reverse();
        }

        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="resultPoints">List<Vector3>in wich to return the results</Vector3></param>
    /// <returns>true if there are more results available. returns result in resultPoints-list</returns>
    public bool FindNextPolygon(List<Vector3> resultPoints) //returns true if there are more polygons to be found
    {
      resultPoints.Clear();
        if(GeometryAvailable == false)
        {
            return false;
        }
        // if there are no intersections, the triangle is completely inside,completely outside or encompassing the boundary
        // so we can return the triangle or the boundary
        if (intersections.Count==0)
        {
             if (isTrianglePointInBoundary()) // if the triangle is completely inside the boundary, we just return the triangle
            {
                foreach (var item in TrianglePoints)
                {
                    resultPoints.Add(item.position3D);
                }
                if (triangleIsFlipped == true)//if the triangle was flipped, flip it back to its original orientation
                {
                    resultPoints.Reverse();
                }
                //we just returned the entire triangle, so there is nothing more to return
                GeometryAvailable = false;
                return false;
            }
            if (isBoundaryPointInsideTriangle()) //if the boundary is completely inside the triangle we return the boundary
            {
                foreach (var item in BoundaryPoints)
                {
                    resultPoints.Add(item.position3D);
                    
                }
                //we just returned the entire boundary, so there is nothing more to return
                GeometryAvailable = false;
                return false;
            }
        }

        // there are intersections, so we have to find a polygon


        int triangleStartIndex = 0;
        int currenttriangleIndex = -0;
        int currentBoundaryIndex = 0;
        bool loopingtriangle = false; //are we looping through the triangle or through te boundary?
        bool startpointFound = false;
        //find first unused intersection on triangle:
        for (int i = 0; i < TrianglePoints.Count; i++)
        {
            if (TrianglePoints[i].switchPoly)
            {
                if (!TrianglePoints[i].visited)
                {
                    triangleStartIndex = i;
                    startpointFound = true;
                }

            }
        }

        if (!startpointFound) return false;

        //mark the startingpoint as visited.
        point point = TrianglePoints[triangleStartIndex];
        point.visited = true;
        TrianglePoints[triangleStartIndex] = point;

        //the startingpoint tells us where to continue in the bounderyList
        currentBoundaryIndex = TrianglePoints[triangleStartIndex].switchToIndex;

        // we continue until we reach the triangleIndex we started from
        while (currenttriangleIndex != triangleStartIndex)
        {
            while (!loopingtriangle) // when in the boundaryList
            {
                // mark the current point as "visited"
                point = BoundaryPoints[currentBoundaryIndex];
                point.visited = true;
                BoundaryPoints[currentBoundaryIndex] = point;
                //check if the point tells us to switch to the triangleList
                if (BoundaryPoints[currentBoundaryIndex].switchPoly)
                {
                    // find the triangleList-index to jump to
                    currenttriangleIndex = BoundaryPoints[currentBoundaryIndex].switchToIndex;
                    //we move to the trianglelist now
                    loopingtriangle = true;
                    //we are moving throug an intersection, so ew decrease the number of unused intersections.
                    UnusedIntersections -= 1;
                }
                else
                {
                    //find the elevation of the boundarypoint
                    Vector2 position = BoundaryPoints[currentBoundaryIndex].position2D;
                    float elevation = FindElevationOnPlane(position);
                    //add the current position to the result.
                    resultPoints.Add(BoundaryPoints[currentBoundaryIndex].position3D);
                    //move to the next point in the list (loop back to 0 when we reached the end)
                    currentBoundaryIndex = nextBoundaryIndex(currentBoundaryIndex);
                }
            }
            while (loopingtriangle)
            {
                // mark the current point as "visited"
                point = TrianglePoints[currenttriangleIndex];
                point.visited = true;
                TrianglePoints[currenttriangleIndex] = point;
                //check if the point tells us to switch to the BoundaryList
                if (TrianglePoints[currenttriangleIndex].switchPoly)
                {
                    // find the BoundaryList-index to jump to
                    currentBoundaryIndex = TrianglePoints[currenttriangleIndex].switchToIndex;
                    //we leave the trinagleList now
                    loopingtriangle = false;
                    //we are moving throug an intersection, so ew decrease the number of unused intersections.
                    UnusedIntersections -= 1;
                }
                else
                {
                    //add the current position to the result.
                    resultPoints.Add(TrianglePoints[currenttriangleIndex].position3D);
                    //move to the next point in the list (loop back to 0 when we reached the end)
                    currenttriangleIndex = nextTriangleIndex(currenttriangleIndex);
                }
            }
        }

        // if the triangleList was flipped because the triangle was UpsideDown, flip it back to its original rotation;
        if (triangleIsFlipped == true)
        {
            resultPoints.Reverse();
        }

        //if there are no intersections left, we are finished.
        //If there is only 1 intersection left, something went wrong, you can't have 2 overlapping polygons with only one intersection. so we also stop.
        //
        if (UnusedIntersections <2)
        {
            GeometryAvailable = false;
            return false;
        }
        return true;
    }
    #endregion

    #region private functions

    #region preparation
    
    void calculatePlaneVariables(List<Vector3> TrianglePolygon)
    {
        //calculate the normal of the triangle
        Vector3 v1 = TrianglePolygon[2] - TrianglePolygon[0];
        Vector3 v2 = TrianglePolygon[1] - TrianglePolygon[0];
        Vector3 normal = Vector3.Cross(v1, v2).normalized;
        

        //store the values
        planeValues.normal = normal;
        planeValues.pointOnPlane = TrianglePolygon[0];

    }
    void DetermineBoundaryBounds(List<Vector2> BoundaryPolygon)
    {
        for (int i = 0; i < BoundaryPolygon.Count; i++)
        {
            BoundaryBounds.Encapsulate(new Vector3(BoundaryPolygon[i].x, 0, BoundaryPolygon[i].y));
        }
    }
    

    #endregion region

    #region analysing input
    bool DoesTriangleBoundsOverlapWithBoundaryBounds(List<Vector3> TrianglePolygon)
    {
        TriangleBounds = new Bounds();
        for (int i = 0; i < TrianglePolygon.Count; i++)
        {
            TriangleBounds.Encapsulate(TrianglePolygon[i]);
        }
        if (BoundaryBounds.Intersects(TriangleBounds))
            {
            return true;
            }
        return false;
        
    }

    bool isTrianglePointInBoundary()
    {
        //create a section that starts outside the Boundary and ends at a point on the triangle
        Section testSection = new Section();
        testSection.startpoint2D = new Vector2(BoundaryBounds.min.x-10, BoundaryBounds.min.z-10);
        testSection.endpoint2D = TrianglePoints[0].position2D;

        // count the intersections between the testsection and all the sections in the boundary
        // if there is an uneven number of intersections, the it lies inside the boundary.
        bool isinside = false;
        Intersection intersection;
        foreach (var item in BoundarySections)
        {
            intersection = FindIntersection(testSection, item);
            if (intersection!=null)
            {
                isinside = !isinside;
                intersectionpool.returnIntersection(intersection);
            }
        }
        return isinside;
    }
    bool isBoundaryPointInsideTriangle()
    {
        //create a section that starts outside the triangle and ends at a point on the boundary
        Section testSection = new Section();
        testSection.startpoint2D = new Vector2(TriangleBounds.min.x-10, TriangleBounds.min.z - 10);
        testSection.endpoint2D =BoundaryPoints[0].position2D;
        // count the intersections between the testsection and all the sections in the triangle
        // if there is an uneven number of intersections, the it lies inside the boundary.
        bool isinside = false;
        Intersection intersection;
        foreach (var item in TriangleSections)
        {
            intersection = FindIntersection(testSection, item);
            if (intersection != null)
            {
                isinside = !isinside;
                intersectionpool.returnIntersection(intersection);
            }
        }
        return isinside;
    }


    #endregion

    #region create sectionLists
    void FillBoundarySectionList(List<Vector2> polygon, List<Section> sectionList)
    {
        sectionList.Clear();
        Section newSection;
        
        for (int i = 0; i < polygon.Count - 1; i++)
        {
            newSection = sectionpool.getSection();
            newSection.startpoint2D = polygon[i];
            newSection.endpoint2D = polygon[i + 1];
            sectionList.Add(newSection);
        }
        newSection = sectionpool.getSection();
        newSection.startpoint2D = polygon[polygon.Count - 1];
        newSection.endpoint2D = polygon[0];
        sectionList.Add(newSection);
    }
    void ResetBoundarySections() //updates the sectiondirection
    {
        
        Vector2 triangleNormal2d = new Vector2(planeValues.normal.x, planeValues.normal.z);
        foreach (var section in BoundarySections)
        {
            section.ResetIntersections();
            if (planeValues.normal.y != 0) // if the trianglePlane is not vertical
            {
                
                section.sectionDirection = Section.SectionDirection.horizontal;
            }
            else 
            { 
                //looking form the top down:
                //if the line goes through the triangle form the front, the cuttingline points up
                // ontherwise the cuttingline points down.
                float dotProduct = Vector2.Dot((section.endpoint2D - section.startpoint2D), triangleNormal2d);
                if (dotProduct < 0)
                {
                    section.sectionDirection = Section.SectionDirection.up;
                }
                else
                {
                    section.sectionDirection = Section.SectionDirection.down;
                }
            }


        }
    }

    void FillTriangleSectionsList(List<Vector3> polygon, List<Section> sectionList)
    {
        clearTriangleSectionsList();
        Section newSection;

        for (int i = 0; i < polygon.Count - 1; i++)
        {
            newSection = sectionpool.getSection();
            newSection.startpoint2D = new Vector2(polygon[i].x,polygon[i].z);
            newSection.startelevation = polygon[i].y;
            newSection.endpoint2D = new Vector2(polygon[i+1].x, polygon[i+1].z);
            newSection.endelevation = polygon[i + 1].y;
            sectionList.Add(newSection);
        }
        newSection = sectionpool.getSection();
        newSection.startpoint2D = new Vector2(polygon[polygon.Count-1].x, polygon[polygon.Count - 1].z);
        newSection.startelevation = polygon[polygon.Count - 1].y;
        newSection.endpoint2D = new Vector2(polygon[0].x, polygon[0].z);
        newSection.endelevation = polygon[0].y;
        sectionList.Add(newSection);

    }
    void clearTriangleSectionsList()
    {
        for (int i = TriangleSections.Count - 1; i >= 0; i--)
        {
            sectionpool.returnSection(TriangleSections[i]);
        }
        TriangleSections.Clear();
    }
    #endregion

    #region find intersections
    void FindIntersections()
    {
        ClearIntersections();
        UnusedIntersections = 0;
        foreach (Section clipSection in BoundarySections)
        {
            foreach (Section triangleSection in TriangleSections)
            {
                Intersection newIntersection = FindIntersection(clipSection, triangleSection);
                if (newIntersection!=null)
                {
                    newIntersection.elevation = FindIntersectionElevation(newIntersection.point,triangleSection);
                    clipSection.AddIntersection(newIntersection);
                    triangleSection.AddIntersection(newIntersection);
                    intersections.Add(newIntersection);
                }
            }

        }
        UnusedIntersections = intersections.Count;
    }
    Intersection FindIntersection(Section boundarysection, Section trianglesection)
    {
        Vector2 intersectionPoint;
        // directions of the lines

        Vector2 boundaryDirection = (boundarysection.endpoint2D - boundarysection.startpoint2D).normalized;
        Vector2 triangleDirection = (trianglesection.endpoint2D - trianglesection.startpoint2D).normalized;

        // get the normals
        Vector2 boundaryNormal = new Vector2(-boundaryDirection.y, boundaryDirection.x);
        Vector2 triangleNormal = new Vector2(-triangleDirection.y, triangleDirection.x);




        
        //Step 1 Find IntersectionDirection
        // we want to find out which turn should be taken at this intersection provided we can only move towards the endpoint of the lines and we can only turn left.
        // to do this we calculate the determinant D=(1.x*2.y)-(1.y*2.x)
        // fact1: the determinant is positive if turning from vector1 to vector2 is CounterClockWise (=Left).
        // fact2: turning from vector2 to vector1 is allways the opposite of turning from vector1 to vector2
        // using the boundary as vector1 and the triangle as vector2 we find that:
        // fact1 tells us that, if the determinant is negative, moving from the boundary to the triangle involved making een right-hand turn. 
        // because this is not allowed, we conclude , using fact2, that the legal left-hand turn is form triangle to boundary.
        // if the determinant is 0, both lines are parallel.
        intersectionDirection direction= new intersectionDirection();
        float Determinant = 0;
            
            Determinant = (boundaryNormal.x * triangleNormal.y) - (triangleNormal.x * boundaryNormal.y);

            if (Determinant < 0)
            {
                direction = intersectionDirection.triangleToBoundary;
            }
            else if (Determinant > 0)
            {
             direction = intersectionDirection.boundaryToTriangle;
            }   
            else
                {
                direction = intersectionDirection.parallel;
                }
       


        //Step 2: are the lines parallel? -> no solutions
        if (direction == intersectionDirection.parallel)
        {
            return null;
        }

        //Step 3: calculate the intersection point
        float A = boundaryNormal.x;
        float B = boundaryNormal.y;

        float C = triangleNormal.x;
        float D = triangleNormal.y;

        //Rewrite the lines to a general form: Ax + By = k1 and Cx + Dy = k2
        float k1 = (boundaryNormal.x * boundarysection.startpoint2D.x) + (boundaryNormal.y * boundarysection.startpoint2D.y);
        float k2 = (triangleNormal.x * trianglesection.startpoint2D.x) + (triangleNormal.y * trianglesection.startpoint2D.y);

        float x_intersect = (D * k1 - B * k2) / (A * D - B * C);
        float y_intersect = (-C * k1 + A * k2) / (A * D - B * C);

        intersectionPoint = new Vector2(x_intersect, y_intersect);

        //Step 4: check if the intersectionpoint falls between the start-and endpoints of both segments.
        bool isIntersection = false;
        Vector2 ab = boundarysection.endpoint2D - boundarysection.startpoint2D;
        //The intersection and the first point
        Vector2 ac = intersectionPoint - boundarysection.startpoint2D;
        if (Vector2.Dot(ab, ac) > 0f && ab.sqrMagnitude >= ac.sqrMagnitude)
        {// intersectionpoint is on section1
            ab = trianglesection.endpoint2D - trianglesection.startpoint2D;
            ac = intersectionPoint - trianglesection.startpoint2D;
            if (Vector2.Dot(ab, ac) > 0f && ab.sqrMagnitude >= ac.sqrMagnitude)
            {
                isIntersection = true;
            }
        }
        if (!isIntersection)
        {
            return null;
        }
        // final step: collect all the data and return it.
        Intersection intersection = intersectionpool.getIntersection();
        intersection.point = intersectionPoint;
        intersection.direction = direction;
        return intersection;
    }
    float FindIntersectionElevation(Vector2 position, Section section)
    {
        float totalDistance = (section.endpoint2D - section.startpoint2D).magnitude;
        float totalHeight = section.endelevation - section.startelevation;
        float slope = totalHeight / totalDistance;
        float distanceToPoint = (position - section.startpoint2D).magnitude;
        float elevation = section.startelevation + (distanceToPoint * slope);
        return elevation;

    }

    float FindElevationOnPlane(Vector2 position)
    {
        // found the math on https://www.habrador.com/tutorials/math/4-plane-ray-intersection/
        // don't really understand how it works, but it works.
        float denominator = Vector3.Dot(Vector3.up, -planeValues.normal);
        if (denominator< 0.00001f)
        {
            return 0;
        }
        float distance = Vector3.Dot(planeValues.pointOnPlane - new Vector3(position.x, 0, position.y), -planeValues.normal) / denominator;
        return distance;
    }
    void ClearIntersections()
    {
        for (int i = 0; i < intersections.Count; i++)
        {
            intersectionpool.returnIntersection(intersections[i]);
        }
        intersections.Clear();
    }
    #endregion

    #region create pointlists
    void CreatePointLists()
    {
        makeBoundaryPointList(BoundaryPoints, BoundarySections);
        makeTrianglePointList(TrianglePoints, TriangleSections);
    }
    void makeTrianglePointList(List<point> pointlist, List<Section> sections)
    {
        pointlist.Clear();
        point Point;
        foreach (var section in sections)
        {
            Point = new point();
            Point.position2D = section.startpoint2D;
            Point.position3D = new Vector3(section.startpoint2D.x,section.startelevation,section.startpoint2D.y);
            pointlist.Add(Point);


            foreach (var intersection in section.Intersections)
            {
                Point = new point();
                Point.position2D = intersection.point;
                Point.position3D = new Vector3(intersection.point.x, intersection.elevation, intersection.point.y);
                Point.intersection = intersection;
                if (intersection.direction == intersectionDirection.triangleToBoundary)
                {
                    Point.switchPoly = true;
                }
                else
                {
                    Point.switchPoly = false;
                    intersection.pointIndexOnTriangle = pointlist.Count;
                }
                pointlist.Add(Point);

            }
        }

    }
    void makeBoundaryPointList(List<point> pointList, List<Section> sections)
    {
        pointList.Clear();
        point Point;
        foreach (var section in sections)
        {
            Point = new point();
            Point.position2D = section.startpoint2D;
            Point.position3D = new Vector3(section.startpoint2D.x, FindElevationOnPlane(section.startpoint2D), section.startpoint2D.y);
            pointList.Add(Point);
            foreach (var intersection in section.Intersections)
            {
                Point = new point();
                Point.position2D = intersection.point;
                Point.position3D = new Vector3(intersection.point.x, intersection.elevation, intersection.point.y);
                Point.intersection = intersection;
                if (intersection.direction == intersectionDirection.boundaryToTriangle)
                {
                    Point.switchPoly = true;
                }
                else
                {
                    Point.switchPoly = false;
                    intersection.pointIndexOnBoundary = pointList.Count;
                }
                pointList.Add(Point);

            }
        }
    }
    void LinkPointLists()
    {
        //connect crossings

        // first for the boundary
        for (int i = 0; i <BoundaryPoints.Count; i++)
        {

            if (BoundaryPoints[i].switchPoly)
            {
                point Point = BoundaryPoints[i];
                Point.switchToIndex = Point.intersection.pointIndexOnTriangle;
                BoundaryPoints[i] = Point;
            }
        }
        // newt for the triangle
        for (int i = 0; i < TrianglePoints.Count; i++)
        {
            if (TrianglePoints[i].switchPoly)
            {
                point Point = TrianglePoints[i];
                Point.switchToIndex = Point.intersection.pointIndexOnBoundary;
                TrianglePoints[i] = Point;
            }
        }
    }

    int nextTriangleIndex(int currentIndex)
    {
        return (currentIndex + 1) % TrianglePoints.Count;
    }
    int nextBoundaryIndex(int currentIndex)
    {
        return (currentIndex + 1) % BoundaryPoints.Count;
    }


    #endregion



    #endregion

}
public enum intersectionDirection
{
    boundaryToTriangle,
    triangleToBoundary,
    parallel
}




