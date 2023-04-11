using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Netherlands3D
{
    [InitializeOnLoad]
    public class MultithreadingWebGL : MonoBehaviour
    {
        static MultithreadingWebGL()
        {
            PlayerSettings.WebGL.threadsSupport = false;
        }
    }
}
