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

            var flatCameraPosition = new Vector3(cameraPosition.x, 0, cameraPosition.z);
            EPSG4936.relativeCenter = CoordinateConverter.WGS84toECEF(CoordinateConverter.UnitytoWGS84(flatCameraPosition));

            var offset = new Vector3(-cameraPosition.x, 0, -cameraPosition.z);

            relativeOriginChanged.Invoke(offset);
        }

    }
}
