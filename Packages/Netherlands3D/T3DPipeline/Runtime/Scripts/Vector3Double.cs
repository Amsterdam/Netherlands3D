using SimpleJSON;
using System;
using UnityEngine;

[System.Serializable]
public struct Vector3Double
{
    public double x;
    public double y;
    public double z;
    public Vector3Double(double x, double y, double z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3Double(JSONArray jsonArray)
    {
        if (jsonArray.Count != 3)
            throw new ArgumentException("Invalid JSONArray. JSONArray must have length of 3");

        x = jsonArray[0].AsDouble;
        y = jsonArray[1].AsDouble;
        z = jsonArray[2].AsDouble;
    }

    public Vector3 AsVector3()
    {
        return new Vector3((float)x, (float)y, (float)z);
    }

    public JSONArray AsJSONArray()
    {
        var array = new JSONArray();
        array.Add(x);
        array.Add(y);
        array.Add(z);
        return array;
    }

}