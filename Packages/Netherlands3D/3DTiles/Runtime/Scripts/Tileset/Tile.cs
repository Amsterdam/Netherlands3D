using Netherlands3D.Coordinates;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Tiles3D
{
   [System.Serializable]
    public class Tile : IDisposable
    {
        public bool isLoading = false;
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

        public Content content; //Gltf content





        public int CountLoadingChildren()
        {
            int result = 0;
            foreach (var childTile in children)
            {
                if (childTile.content != null)
                {
                    if (childTile.contentUri.Contains(".json") == false)
                    {
                        if (childTile.content.State != Content.ContentLoadState.DOWNLOADED)
                        {
                            result += 1;
                        }

                    }
                }
            }
            foreach (var childTile in children)
                {
                    result += childTile.CountLoadingChildren();
                }
            
            return result;
        }
        public int loadedChildren;
        public int CountLoadedChildren()
        {
            int result = 0;
            foreach (var childTile in children)
            {
                if (childTile.content != null)
                {
                    if (childTile.contentUri.Contains(".json") == false)
                    {

                        if (childTile.content.State != Content.ContentLoadState.DOWNLOADING)
                        {
                            result++;
                        }

                    }
                }
            }
                foreach (var childTile in children)
                {
                    result += childTile.CountLoadedChildren();
                }
            loadedChildren = result;
            return result;
        }

        public int CountLoadedParents()
        {
            int result = 0;
            if (parent !=null)
            {
                if (parent.content != null)
                {
                    if (parent.contentUri.Contains(".json") == false)
                    {
                        if (parent.content.State == Content.ContentLoadState.DOWNLOADED)
                        {
                            result = 1;
                        }
                    }
                }
            }
           
            if (parent !=null)
            {
                return result + parent.CountLoadedParents();
            }
            return result;
        }

        public int CountLoadingParents()
        {
            int result = 0;
            if (parent != null)
            {
                if (parent.isLoading)
                {
                    if (parent.contentUri.Contains(".json") == false)
                    {
                        if (parent.content != null)
                        {
                            if (parent.content.State != Content.ContentLoadState.DOWNLOADED)
                            {
                                result = 1;
                            }
                        }
                    }
                }
            }
            if (parent != null)
            {
                return result + parent.CountLoadingParents();
            }
            return result;
        }
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
                    var boxCenter = CoordinateConverter.ECEFToUnity(new Vector3ECEF(boundingVolume.values[0], boundingVolume.values[1], boundingVolume.values[2]));
                    var xAxis = CoordinateConverter.ECEFToUnity(new Vector3ECEF(boundingVolume.values[3], boundingVolume.values[4], boundingVolume.values[5]));
                    var yAxis = CoordinateConverter.ECEFToUnity(new Vector3ECEF(boundingVolume.values[6], boundingVolume.values[7], boundingVolume.values[8]));
                    var zAxis = CoordinateConverter.ECEFToUnity(new Vector3ECEF(boundingVolume.values[9], boundingVolume.values[10], boundingVolume.values[11]));

                    var xAxisExt = CoordinateConverter.ECEFToUnity(new Vector3ECEF(boundingVolume.values[0] + boundingVolume.values[3], boundingVolume.values[1] + boundingVolume.values[4], boundingVolume.values[2] + boundingVolume.values[5]));
                    var yAxisExt = CoordinateConverter.ECEFToUnity(new Vector3ECEF(boundingVolume.values[0] + boundingVolume.values[6], boundingVolume.values[1] + boundingVolume.values[7], boundingVolume.values[2] + boundingVolume.values[8]));
                    var zAxisExt = CoordinateConverter.ECEFToUnity(new Vector3ECEF(boundingVolume.values[0] + boundingVolume.values[9], boundingVolume.values[1] + boundingVolume.values[10], boundingVolume.values[2] + boundingVolume.values[11]));

                    var xAxisExtInv = CoordinateConverter.ECEFToUnity(new Vector3ECEF(boundingVolume.values[0] - boundingVolume.values[3], boundingVolume.values[1] - boundingVolume.values[4], boundingVolume.values[2] - boundingVolume.values[5]));
                    var yAxisExtInv = CoordinateConverter.ECEFToUnity(new Vector3ECEF(boundingVolume.values[0] - boundingVolume.values[6], boundingVolume.values[1] - boundingVolume.values[7], boundingVolume.values[2] - boundingVolume.values[8]));
                    var zAxisExtInv = CoordinateConverter.ECEFToUnity(new Vector3ECEF(boundingVolume.values[0] - boundingVolume.values[9], boundingVolume.values[1] - boundingVolume.values[10], boundingVolume.values[2] - boundingVolume.values[11]));

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
                    var sphereCentre = CoordinateConverter.ECEFToUnity(new Vector3ECEF(boundingVolume.values[0], boundingVolume.values[1], boundingVolume.values[2]));
                    var sphereMin = CoordinateConverter.ECEFToUnity(new Vector3ECEF(boundingVolume.values[0]- sphereRadius, boundingVolume.values[1] - sphereRadius, boundingVolume.values[2] - sphereRadius));
                    var sphereMax = CoordinateConverter.ECEFToUnity(new Vector3ECEF(boundingVolume.values[0]+ sphereRadius, boundingVolume.values[1]+ sphereRadius, boundingVolume.values[2]+ sphereRadius));
                    bounds.size = Vector3.zero;
                    bounds.center = sphereCentre;
                    bounds.Encapsulate(sphereMin);
                    bounds.Encapsulate(sphereMax);
                    break;
                case BoundingVolumeType.Region:
                    //Array order: west, south, east, north, minimum height, maximum height
                    var ecefMin = CoordinateConverter.WGS84toECEF(new Vector3WGS((boundingVolume.values[0] * 180.0f) / Mathf.PI, (boundingVolume.values[1] * 180.0f) / Mathf.PI, boundingVolume.values[4]));
                    var ecefMax = CoordinateConverter.WGS84toECEF(new Vector3WGS((boundingVolume.values[2] * 180.0f) / Mathf.PI, (boundingVolume.values[3] * 180.0f) / Mathf.PI, boundingVolume.values[5]));

                    var unityMin = CoordinateConverter.ECEFToUnity(ecefMin);
                    var unityMax = CoordinateConverter.ECEFToUnity(ecefMax);

                    bounds.size = Vector3.zero;
                    bounds.center = unityMin;
                    bounds.Encapsulate(unityMax);
                    break;
                default:
                    break;
            }

            boundsAvailable = true;
        }

        public float getParentSSE()
        {
            float result = 0;
            if (parent!=null)
            {

            
            
            if (parent.content!=null)
            {
                if (parent.content.State==Content.ContentLoadState.DOWNLOADED)
                {
                    result = parent.screenSpaceError;
                }
            }
            if (result==0)
            {
                    result = parent.getParentSSE();
            }
            }
            return result;
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
