using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Core.Tiles
{
    /// <summary>
    /// Base class for adding/removing tiles to a prioritised list.
    /// Derived classes can contain specific logic to determine the priority based on platform.
    /// </summary>
    public abstract class TilePrioritiser : MonoBehaviour
    {
        public abstract void CalculatePriorities();
        public abstract void Add(Tile tile);
        public abstract void Remove(Tile tile);
        public abstract void SetCamera(Camera currentMainCamera);
    }
}