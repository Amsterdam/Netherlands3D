using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.Coordinates
{
    public static class MovingOrigin
    {
        public static UnityEvent prepareForOriginShift = new();
        public class CenterChangedEvent : UnityEvent<Vector3> { }
        public static CenterChangedEvent relativeOriginChanged = new();

        public static void MoveAndRotateWorld(Vector3 cameraPosition)
        {
            prepareForOriginShift.Invoke();

            var flatCameraPosition = new Coordinate(CoordinateSystem.Unity, cameraPosition.x, 0d, cameraPosition.z);
            var wgsCoordinate = CoordinateConverter.ConvertTo(flatCameraPosition, CoordinateSystem.WGS84);
            EPSG4936.relativeCenter = CoordinateConverter.ConvertTo(wgsCoordinate, CoordinateSystem.EPSG_4936).ToVector3ECEF();

            var offset = new Vector3(-cameraPosition.x, 0, -cameraPosition.z);

            relativeOriginChanged.Invoke(offset);
        }

    }
}
