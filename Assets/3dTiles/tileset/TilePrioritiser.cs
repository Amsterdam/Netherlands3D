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

        [Header("SSE Screen height limitations (0 is disabled)")]
        [SerializeField] private int maxScreenHeightInPixels = 0;
        [SerializeField] private int maxScreenHeightInPixelsMobile = 0;

        private bool mobileMode = false;
        public bool MobileMode { get => mobileMode; set => mobileMode = value; }
        public int MaxScreenHeightInPixels {
            get
            {
                return (mobileMode) ? maxScreenHeightInPixelsMobile: maxScreenHeightInPixels;
            }
            set => maxScreenHeightInPixels = value; 
        }

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