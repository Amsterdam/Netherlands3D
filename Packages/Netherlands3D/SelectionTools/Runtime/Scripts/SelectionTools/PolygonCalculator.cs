/*
*  Copyright (C) X Gemeente
*                X Amsterdam
*                X Economic Services Departments
*
*  Licensed under the EUPL, Version 1.2 or later (the "License");
*  You may not use this work except in compliance with the License.
*  You may obtain a copy of the License at:
*
*    https://github.com/Amsterdam/3DAmsterdam/blob/master/LICENSE.txt
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" basis,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
*  implied. See the License for the specific language governing
*  permissions and limitations under the License.
*/

using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.SelectionTools
{
    public static class PolygonCalculator
    {
        /// <summary>
        /// flatten a 3D polygon 
        /// </summary>
        /// <param name="polygon3D">A polygon defined in 3D space</param>
        /// <param name="plane"> The plane on which to flatten the 3D polygon</param>
        /// <returns>An array with the polygon projected on the plane</returns>
        public static Vector2[] FlattenPolygon(IList<Vector3> polygon3D, Plane plane)
        {
            Vector2[] polygon = new Vector2[polygon3D.Count];
            for (int i = 0; i < polygon3D.Count; i++)
            {
                Quaternion planeRotation = Quaternion.FromToRotation(-plane.normal, Vector3.forward); //use forward so that z component can be ignored
                polygon[i] = planeRotation * polygon3D[i];
            }
            return polygon;
        }

        /// <summary>
        /// Check if a 2d polygon contains point p
        /// </summary>
        /// <param name="polygon">array of points that define the polygon</param>
        /// <param name="p">point to test</param>
        /// <returns>true if point p is inside the polygon, otherwise false</returns>
        public static bool ContainsPoint(IList<Vector2> polygon, Vector2 p)
        {
            var j = polygon.Count - 1;
            var inside = false;
            for (int i = 0; i < polygon.Count; j = i++)
            {
                var pi = polygon[i];
                var pj = polygon[j];
                if (((pi.y <= p.y && p.y < pj.y) || (pj.y <= p.y && p.y < pi.y)) &&
                    (p.x < (pj.x - pi.x) * (p.y - pi.y) / (pj.y - pi.y) + pi.x))
                    inside = !inside;
            }
            return inside;
        }

        /// <summary>
        /// Calculate the area of a 2d Polygon
        /// </summary>
        /// <param name="polygon">array of points that define the polygon</param>
        /// <returns>the area of the polygon</returns>
        public static float Area(IList<Vector2> polygon)
        {
            int n = polygon.Count;
            float a = 0.0f;
            for (int p = n - 1, q = 0; q < n; p = q++)
            {
                Vector2 pval = polygon[p];
                Vector2 qval = polygon[q];
                a += pval.x * qval.y - qval.x * pval.y;
            }
            return Mathf.Abs(a * 0.5f);
        }

        /// <summary>
        /// Check if a point is inside or outside a triangle defined by points a, b, and c
        /// </summary>
        /// <param name="a">First point of the triangle</param>
        /// <param name="b">Second point of the triangle</param>
        /// <param name="c">Third point of the triangle</param>
        /// <param name="point">Point to evaluate</param>
        /// <returns>true if the point is inside triangle abc, otherwise false</returns>
        public static bool IsInsideTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 point)
        {
            float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
            float cCROSSap, bCROSScp, aCROSSbp;

            ax = c.x - b.x; ay = c.y - b.y;
            bx = a.x - c.x; by = a.y - c.y;
            cx = b.x - a.x; cy = b.y - a.y;
            apx = point.x - a.x; apy = point.y - a.y;
            bpx = point.x - b.x; bpy = point.y - b.y;
            cpx = point.x - c.x; cpy = point.y - c.y;

            aCROSSbp = ax * bpy - ay * bpx;
            cCROSSap = cx * apy - cy * apx;
            bCROSScp = bx * cpy - by * cpx;

            return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
        }

        /// <summary>
        /// Check if the provided list of points is a clockwise polygon
        /// </summary>
        /// <param name="points"></param>
        /// <returns>true if the polygon is clockwise; false if the polygon is counter-clockwise</returns>
        public static bool PolygonIsClockwise(IList<Vector2> points) //todo: make input parameter 2D since y component is not used
        {
            double sum = 0;
            for (int i = 0; i < points.Count - 1; i++)
            {
                sum += (points[i + 1].x - points[i].x) * (points[i + 1].y + points[i].y);
            }
            bool isClockwise = (sum > 0) ? true : false;
            return isClockwise;
        }

        /// <summary>
        /// Poly2Mesh has problems with polygons that have points in the same position.
        /// Lets move them a bit.
        /// This function modifies the provided source list
        /// </summary>
        /// <param name="contour"></param>
        public static void FixSequentialDoubles(List<Vector3> contour)
        {
            var removedSomeDoubles = false;
            for (int i = contour.Count - 2; i >= 0; i--)
            {
                if (contour[i] == contour[i + 1])
                {
                    contour.RemoveAt(i + 1);
                    removedSomeDoubles = true;
                }
            }
            if (removedSomeDoubles) Debug.Log("Removed some doubles");
        }

        /// <summary>
        /// Returns if lines intersect on a flat plane
        /// </summary>
        /// <returns></returns>
        public static bool LinesIntersectOnPlane(Vector3 lineOneA, Vector3 lineOneB, Vector3 lineTwoA, Vector3 lineTwoB)
        {
            return
                (((lineTwoB.z - lineOneA.z) * (lineTwoA.x - lineOneA.x) > (lineTwoA.z - lineOneA.z) * (lineTwoB.x - lineOneA.x)) !=
                ((lineTwoB.z - lineOneB.z) * (lineTwoA.x - lineOneB.x) > (lineTwoA.z - lineOneB.z) * (lineTwoB.x - lineOneB.x)) &&
                ((lineTwoA.z - lineOneA.z) * (lineOneB.x - lineOneA.x) > (lineOneB.z - lineOneA.z) * (lineTwoA.x - lineOneA.x)) !=
                ((lineTwoB.z - lineOneA.z) * (lineOneB.x - lineOneA.x) > (lineOneB.z - lineOneA.z) * (lineTwoB.x - lineOneA.x)));
        }

        /// <summary>
        /// Compare line with placed lines to check if they do not intersect.
        /// </summary>
        /// <param name="linePointA">Start point of the line we want to check</param>
        /// <param name="linePointB">End point of the line we want to check</param>
        /// <param name="existingLines">lines to check against. Each adjacent pair of Vector3 points is considered the start and end point of a line</param>
        /// <param name="skipFirst">Skip the first line in our chain</param>
        /// <param name="skipLast">Skip the last line in our chain</param>
        /// <returns>Returns true if an intersection was found</returns>
        public static bool LineCrossesOtherLine(Vector3 linePointA, Vector3 linePointB, IList<Vector3> existingLines, bool skipFirst = false, bool skipLast = false, bool ignoreConnected = false)
        {
            int startIndex = (skipFirst) ? 2 : 1;
            int endIndex = (skipLast) ? existingLines.Count - 1 : existingLines.Count;
            for (int i = startIndex; i < endIndex; i++)
            {
                var comparisonStart = existingLines[i - 1];
                var comparisonEnd = existingLines[i];
                if (PolygonCalculator.LinesIntersectOnPlane(linePointA, linePointB, comparisonStart, comparisonEnd))
                {
                    if (ignoreConnected)
                    {
                        if (linePointA.Equals(comparisonStart) || linePointA.Equals(comparisonEnd) || linePointB.Equals(comparisonStart) || linePointB.Equals(comparisonEnd))
                        {
                            //Debug.Log("Line is overlapping connected line! This is allowed.");
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        //Debug.Log("Line is crossing other line! This is not allowed.");
                        return true;
                    }
                }
            }
            return false;
        }
    }
}