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
    public static class GeometryCalculator
    {
        public static bool ContainsPoint(Vector2[] polygon, Vector2 p)
        {
            var j = polygon.Length - 1;
            var inside = false;
            for (int i = 0; i < polygon.Length; j = i++)
            {
                var pi = polygon[i];
                var pj = polygon[j];
                if (((pi.y <= p.y && p.y < pj.y) || (pj.y <= p.y && p.y < pi.y)) &&
                    (p.x < (pj.x - pi.x) * (p.y - pi.y) / (pj.y - pi.y) + pi.x))
                    inside = !inside;
            }
            return inside;
        }

        public static float Area(Vector2[] polygon)
        {
            int n = polygon.Length;
            float a = 0.0f;
            for (int p = n - 1, q = 0; q < n; p = q++)
            {
                Vector2 pval = polygon[p];
                Vector2 qval = polygon[q];
                a += pval.x * qval.y - qval.x * pval.y;
            }
            return Mathf.Abs(a * 0.5f);
        }

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
    }
}