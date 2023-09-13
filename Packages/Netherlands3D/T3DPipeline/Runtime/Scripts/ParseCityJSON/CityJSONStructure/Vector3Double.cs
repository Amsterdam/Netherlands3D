using Netherlands3D.Core;
using SimpleJSON;
using System;
using Netherlands3D.Coordinates;
using UnityEngine;

/// <summary>
/// Vertices in a CityJSON require more precision than floats. This class mimics the basic functionality of Unity's Vector3 but with doubles instead of floats.
/// </summary>
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

    public static Vector3Double operator +(Vector3Double lhs, Vector3Double rhs)
    {
        return new Vector3Double(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z);
    }

    public static Vector3Double operator -(Vector3Double lhs, Vector3Double rhs)
    {
        return new Vector3Double(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z);
    }

    public static Vector3Double operator *(Vector3Double lhs, double rhs)
    {
        return new Vector3Double(lhs.x * rhs, lhs.y * rhs, lhs.z * rhs);
    }

    public static Vector3Double operator /(Vector3Double lhs, double rhs)
    {
        return new Vector3Double(lhs.x / rhs, lhs.y / rhs, lhs.z / rhs);
    }

    public static Vector3Double operator *(Vector3Double lhs, Vector3Double rhs) // returns an element wise multiplication, not matrix multiplication
    {
        return new Vector3Double(lhs.x * rhs.x, lhs.y * rhs.y, lhs.z * rhs.z);
    }

    public override bool Equals(object other)
    {
        if (!(other is Vector3Double)) return false;

        return Equals((Vector3Double)other);
    }

    public bool Equals(Vector3Double other)
    {
        return x == other.x && y == other.y && z == other.z;
    }

    public override string ToString()
    {
        return "(" + x + ", " + y + ", " + z + ")";
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(x, y, z);
    }

    public static explicit operator Vector3(Vector3Double v) => new Vector3((float)v.x, (float)v.y, (float)v.z);

    public static implicit operator Vector3RD(Vector3Double v) => new Vector3RD(v.x, v.y, v.z);
    public static implicit operator Vector3Double(Vector3RD v) => new Vector3Double(v.x, v.y, v.z);
    public static implicit operator Vector3Double(Vector3 v) => new Vector3Double(v.x, v.y, v.z);
}