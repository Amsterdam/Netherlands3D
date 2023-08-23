using Netherlands3D.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Tiles3D
{
    [System.Serializable]
    public class Tile : IDisposable
    {
        public int X;
        public int Y;
        public int Z;
        public bool hascontent;

        public int childrenCountDelayingDispose = 0;
        public Tile parent;

        [SerializeField] public List<Tile> children = new List<Tile>();

        public double[] transform = new double[16] { 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0 };
        public double geometricError;
        public float screenSpaceError = float.MaxValue;

        public string refine;
        public BoundingVolume boundingVolume;

        public bool inView = false;
        public bool canRefine = false;

        public string contentUri = "";
        public Content content;

        public int parentsLoadingCount;
        public int parentsLoadedCount;
        public int loadingChildrenCount;
        public int loadedChildrenCount;


        public int priority = 0;

        private bool boundsAvailable = false;
        private Bounds bounds = new Bounds();

        public bool requestedDispose = false;
        public bool requestedUpdate = false;
        internal bool nestedTilesLoaded = false;

        public Bounds ContentBounds
        {
            get
            {
                return bounds;
            }
            set => bounds = value;
        }

        public Vector3 EulerRotationToVertical()
        {
            float posX = (float)(transform[12] / 1000); // measured for earth-center to prime meridian (greenwich)
            float posY = (float)(transform[13] / 1000); // measured from earth-center to 90degrees east at equator
            float posZ = (float)(transform[14] / 1000); // measured from earth-center to nothpole

            float angleX = -Mathf.Rad2Deg * Mathf.Atan(posY / posZ);
            float angleY = -Mathf.Rad2Deg * Mathf.Atan(posX / posZ);
            float angleZ = -Mathf.Rad2Deg * Mathf.Atan(posY / posX);
            Vector3 result = new Vector3(angleX, angleY, angleZ);
            return result;
        }

        public Quaternion RotationToVertical()
        {
            float posX = (float)(transform[12] / 1000000); // measured for earth-center to prime meridian (greenwich)
            float posY = (float)(transform[13] / 1000000); // measured from earth-center to 90degrees east at equator
            float posZ = (float)(transform[14] / 1000000); // measured from earth-center to nothpole

            Quaternion rotation = Quaternion.FromToRotation(new Vector3(posX, posY, posZ), new Vector3(0, 0, 1));

            return rotation;
        }

        public int GetChildCount()
        {
            int childcount = 1;
            foreach (var child in children)
            {
                childcount += child.GetChildCount();
            }
            return childcount;
        }

        public bool ChildrenHaveContent()
        {
            if (children.Count > 0) { 
                foreach (var child in children)
                {
                    if (!child.content || child.content.State != Content.ContentLoadState.DOWNLOADED) return false;
                    break;
                }
            }
            return true;
        }

        public int GetNestingDepth()
        {
            int maxDepth = 1;
            foreach (var child in children)
            {
                int depth = child.GetNestingDepth() + 1;
                if (depth > maxDepth) maxDepth = depth;

            }
            return maxDepth;
        }

        public enum TileStatus
        {
            unloaded,
            loaded
        }

        public void IncrementLoadingChildren()
        {
            loadingChildrenCount++;
            if (parent!=null)
            {
                parent.IncrementLoadingChildren();
            }
        }
        public void IncrementLoadingParents()
        {
            parentsLoadingCount++;
            foreach (var child in children)
            {
                child.IncrementLoadingParents();
            }
        }

        public void IncrementLoadedChildren()
        {
            loadedChildrenCount++;
            if (parent != null)
            {
                parent.IncrementLoadedChildren();
            }
        }

        public void IncrementLoadedParents()
        {
            parentsLoadedCount++;
            foreach (var child in children)
            {
                child.IncrementLoadedParents();
            }
        }
        public void DecrementLoadingParents()
        {
            parentsLoadingCount--;
            foreach (var child in children)
            {
                child.DecrementLoadingParents();
            }
        }
        public void DecrementLoadingChildren()
        {
            loadingChildrenCount--;
            if (parent != null)
            {
                parent.DecrementLoadingChildren();
            }
        }
        public void DecrementLoadedChildren()
        {
            loadedChildrenCount--;
            if (parent != null)
            {
                parent.DecrementLoadedChildren();
            }
        }
        public void DecrementLoadedParents()
        {
            parentsLoadedCount--;
            foreach (var child in children)
            {
                child.DecrementLoadedParents();
            }
        }

        public bool IsInViewFrustrum(Camera ofCamera)
        {
            if (!boundsAvailable) CalculateBounds();

            inView = ofCamera.InView(ContentBounds);

            return inView;
        }

        public void CalculateBounds()
        {
            switch (boundingVolume.boundingVolumeType)
            {
                case BoundingVolumeType.Box:
                    //TODO: proper Box bounding calculation
                    var boxCenter = CoordConvert.ECEFToUnity(new Vector3ECEF(boundingVolume.values[0], boundingVolume.values[1], boundingVolume.values[2]));
                    var xAxis = CoordConvert.ECEFToUnity(new Vector3ECEF(boundingVolume.values[3], boundingVolume.values[4], boundingVolume.values[5]));
                    var yAxis = CoordConvert.ECEFToUnity(new Vector3ECEF(boundingVolume.values[6], boundingVolume.values[7], boundingVolume.values[8]));
                    var zAxis = CoordConvert.ECEFToUnity(new Vector3ECEF(boundingVolume.values[9], boundingVolume.values[10], boundingVolume.values[11]));

                    var xAxisExt = CoordConvert.ECEFToUnity(new Vector3ECEF(boundingVolume.values[0] + boundingVolume.values[3], boundingVolume.values[1] + boundingVolume.values[4], boundingVolume.values[2] + boundingVolume.values[5]));
                    var yAxisExt = CoordConvert.ECEFToUnity(new Vector3ECEF(boundingVolume.values[0] + boundingVolume.values[6], boundingVolume.values[1] + boundingVolume.values[7], boundingVolume.values[2] + boundingVolume.values[8]));
                    var zAxisExt = CoordConvert.ECEFToUnity(new Vector3ECEF(boundingVolume.values[0] + boundingVolume.values[9], boundingVolume.values[1] + boundingVolume.values[10], boundingVolume.values[2] + boundingVolume.values[11]));

                    var xAxisExtInv = CoordConvert.ECEFToUnity(new Vector3ECEF(boundingVolume.values[0] - boundingVolume.values[3], boundingVolume.values[1] - boundingVolume.values[4], boundingVolume.values[2] - boundingVolume.values[5]));
                    var yAxisExtInv = CoordConvert.ECEFToUnity(new Vector3ECEF(boundingVolume.values[0] - boundingVolume.values[6], boundingVolume.values[1] - boundingVolume.values[7], boundingVolume.values[2] - boundingVolume.values[8]));
                    var zAxisExtInv = CoordConvert.ECEFToUnity(new Vector3ECEF(boundingVolume.values[0] - boundingVolume.values[9], boundingVolume.values[1] - boundingVolume.values[10], boundingVolume.values[2] - boundingVolume.values[11]));

                    bounds.size = Vector3.zero;
                    bounds.center = boxCenter;
                    bounds.Encapsulate(xAxisExt);
                    bounds.Encapsulate(yAxisExt);
                    bounds.Encapsulate(zAxisExt);
                    bounds.Encapsulate(xAxisExtInv);
                    bounds.Encapsulate(yAxisExtInv);
                    bounds.Encapsulate(zAxisExtInv);

                    break;
                case BoundingVolumeType.Sphere:
                    var sphereRadius = boundingVolume.values[0];
                    var sphereCentre = CoordConvert.ECEFToUnity(new Vector3ECEF(boundingVolume.values[0], boundingVolume.values[1], boundingVolume.values[2]));
                    var sphereMin = CoordConvert.ECEFToUnity(new Vector3ECEF(boundingVolume.values[0]- sphereRadius, boundingVolume.values[1] - sphereRadius, boundingVolume.values[2] - sphereRadius));
                    var sphereMax = CoordConvert.ECEFToUnity(new Vector3ECEF(boundingVolume.values[0]+ sphereRadius, boundingVolume.values[1]+ sphereRadius, boundingVolume.values[2]+ sphereRadius));
                    bounds.size = Vector3.zero;
                    bounds.center = sphereCentre;
                    bounds.Encapsulate(sphereMin);
                    bounds.Encapsulate(sphereMax);
                    break;
                case BoundingVolumeType.Region:
                    //Array order: west, south, east, north, minimum height, maximum height
                    var ecefMin = CoordConvert.WGS84toECEF(new Vector3WGS((boundingVolume.values[0] * 180.0f) / Mathf.PI, (boundingVolume.values[1] * 180.0f) / Mathf.PI, boundingVolume.values[4]));
                    var ecefMax = CoordConvert.WGS84toECEF(new Vector3WGS((boundingVolume.values[2] * 180.0f) / Mathf.PI, (boundingVolume.values[3] * 180.0f) / Mathf.PI, boundingVolume.values[5]));

                    var unityMin = CoordConvert.ECEFToUnity(ecefMin);
                    var unityMax = CoordConvert.ECEFToUnity(ecefMax);

                    bounds.size = Vector3.zero;
                    bounds.center = unityMin;
                    bounds.Encapsulate(unityMax);
                    break;
                default:
                    break;
            }

            boundsAvailable = true;
        }

        public void Dispose()
        {
            if (content != null)
            {
                content.Dispose();
                content = null;
            }
        }
    }
}
