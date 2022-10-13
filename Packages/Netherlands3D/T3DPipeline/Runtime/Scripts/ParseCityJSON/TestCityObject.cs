using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.T3DPipeline
{
    public class TestCityObject : CityObject
    {
        protected override void Start()
        {
            base.Start();
            Type = CityObjectType.CityFurniture;
            geometries = new List<CityGeometry>();
            geometries.Add(new CityGeometry(Type, GeometryType.CompositeSolid, 1, false, false, false));

            print(GetJsonObject());
        }


        public override CitySurface[] GetSurfaces()
        {
            throw new System.NotImplementedException();
        }
    }
}