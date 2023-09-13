using UnityEngine;
using System.Collections;
using Netherlands3D.Coordinates;
using Netherlands3D.Core;

public static class CameraExtensions
{
    public static Vector3[] corners = new Vector3[4];

    private static Vector3 unityMin;
    private static Vector3 unityMax;

    private static Vector2 topLeft = new Vector2(0, 1);
    private static Vector2 topRight = new Vector2(1, 1);
    private static Vector2 bottomRight = new Vector2(1, 0);
    private static Vector2 bottomLeft = new Vector2(0, 0);

    private static Plane[] cameraFrustumPlanes = new Plane[6]
	{
		new Plane(), //Left
		new Plane(), //Right
		new Plane(), //Down
		new Plane(), //Up
		new Plane(), //Near
		new Plane(), //Far
	};

    public static Extent GetExtent(this Camera camera, float maximumViewDistance = 0)
    {
        if (maximumViewDistance == 0) maximumViewDistance = camera.farClipPlane;
        CalculateCornerExtents(camera, maximumViewDistance);

        // Area that should be loaded
        var extent = new Extent(unityMin.x, unityMin.z, unityMax.x, unityMax.z);

        return extent;
    }

    public static Extent GetRDExtent(this Camera camera, float maximumViewDistance = 0)
    {
        if (maximumViewDistance == 0) maximumViewDistance = camera.farClipPlane;
        CalculateCornerExtents(camera, maximumViewDistance);

        // Convert min and max to WGS84 coordinates
        var rdMin = CoordinateConverter.UnitytoRD(unityMin);
        var rdMax = CoordinateConverter.UnitytoRD(unityMax);

        // Area that should be loaded
        var extent = new Extent(rdMin.x, rdMin.y, rdMax.x, rdMax.y);

        return extent;
    }

    private static void CalculateCornerExtents(Camera camera, float maximumViewDistance)
    {
        // Determine what world coordinates are in the corners of our view
        corners[0] = GetCornerPoint(camera, topLeft, maximumViewDistance);
        corners[1] = GetCornerPoint(camera, topRight, maximumViewDistance);
        corners[2] = GetCornerPoint(camera, bottomRight, maximumViewDistance);
        corners[3] = GetCornerPoint(camera, bottomLeft, maximumViewDistance);

        // Determine the min and max X- en Z-value of the visible coordinates
        unityMax = new Vector3(-float.MaxValue, -float.MaxValue, -float.MaxValue);
        unityMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        for (int i = 0; i < 4; i++)
        {
            unityMin.x = Mathf.Min(unityMin.x, corners[i].x);
            unityMin.z = Mathf.Min(unityMin.z, corners[i].z);
            unityMax.x = Mathf.Max(unityMax.x, corners[i].x);
            unityMax.z = Mathf.Max(unityMax.z, corners[i].z);
        }
    }

    private static Vector3 GetCornerPoint(this Camera camera, Vector2 screenPosition, float maximumViewDistance)
    {
        var output = new Vector3();

        var topScreenPointFar = Camera.main.ViewportToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10000));
        var topScreenPointNear = Camera.main.ViewportToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10));

        // Calculate direction vector
        Vector3 direction = topScreenPointNear - topScreenPointFar;
        float factor; //factor waarmee de Richtingvector vermenigvuldigd moet worden om op het maaiveld te stoppen
        if (direction.y < 0) //wanneer de Richtingvector omhooggaat deze factor op 1 instellen
        {
            factor = 1;
        }
        else
        {
            factor = ((topScreenPointNear.y) / direction.y); //factor bepalen t.o.v. maaiveld (aanname maaiveld op 0 NAP = ca 40 Unityeenheden in Y-richting)
        }

        // Determine the X, Y, en Z location where the viewline ends
        output.x = topScreenPointNear.x - Mathf.Clamp((factor * direction.x), -1 * maximumViewDistance, maximumViewDistance);
        output.y = topScreenPointNear.y - Mathf.Clamp((factor * direction.y), -1 * maximumViewDistance, maximumViewDistance);
        output.z = topScreenPointNear.z - Mathf.Clamp((factor * direction.z), -1 * maximumViewDistance, maximumViewDistance);

        return output;
    }

    public static Vector3[] GetWorldSpaceCorners(this Camera camera)
    {
        return corners;
    }

    /// <summary>
    /// Get the position of a screen point in world coordinates ( on a plane )
    /// </summary>
    /// <param name="screenPoint">The point in screenpoint coordinates</param>
    /// <returns></returns>
    public static Vector3 GetCoordinateInWorld(this Camera camera, Vector3 screenPoint, Plane worldPlane, float maxSelectionDistanceFromCamera = Mathf.Infinity)
    {
        var screenRay = camera.ScreenPointToRay(screenPoint);

        worldPlane.Raycast(screenRay, out float distance);
        var samplePoint = screenRay.GetPoint(Mathf.Min(maxSelectionDistanceFromCamera, distance));

        return samplePoint;
    }

    public static bool InView(this Camera camera, Bounds bounds)
    {
        //If camera is inside bounds we see it.
        if (bounds.Contains(camera.transform.position))
            return true;

        //Else check if frustum intersects
        GeometryUtility.CalculateFrustumPlanes(camera, cameraFrustumPlanes);
        return GeometryUtility.TestPlanesAABB(cameraFrustumPlanes, bounds);
    }
}
