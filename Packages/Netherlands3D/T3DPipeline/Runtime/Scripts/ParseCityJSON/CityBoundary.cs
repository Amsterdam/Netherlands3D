using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Netherlands3D.T3DPipeline
{
    public class CityBoundary
    {
        public List<CityBoundary> Boundaries { get; private set; } = new List<CityBoundary>(1); //depth is dependant on GeometryType
        public CityBoundary OuterBoundary=> Boundaries[0];
        public CityBoundary[] InteriorBoundaries => Boundaries.Skip(1).ToArray();

        public List<CityPolygon> Polygons; //only valid when depth is reached
    }
}
