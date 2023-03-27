using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.Core.Tiles
{
    /// <summary>
    /// Base class for adding/removing tiles to a prioritised list.
    /// Derived classes can contain specific logic to determine the priority based on platform.
    /// </summary>
    public abstract class TilePrioritiser : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern bool isMobile();

        [Header("Mobile limitations")]
        [SerializeField] private int overrideResolutionSSE = 540;
        private bool mobileMode = false;

        public int OverrideResolutionSSE { get => overrideResolutionSSE; set => overrideResolutionSSE = value; }
        public bool MobileMode { get => mobileMode; set => mobileMode = value; }
        public UnityEvent<bool> mobileModeEnabled;

        private void Awake()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            MobileMode = isMobile();
#endif
            mobileModeEnabled.Invoke(MobileMode);
        }

        public abstract void CalculatePriorities();
        public abstract void RequestUpdate(Tile tile);
        public abstract void RequestDispose(Tile tile);
        public abstract void SetCamera(Camera currentMainCamera);
    }
}