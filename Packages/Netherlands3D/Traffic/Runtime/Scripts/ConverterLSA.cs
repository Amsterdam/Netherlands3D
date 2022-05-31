using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Traffic.VISSIM
{
    /// <summary>
    /// For converting vissim .lsa files
    /// </summary>
    public static class ConverterLSA
    {
        public static IEnumerator Convert(string filePath, int maxDataCount, Action<Dictionary<int, Data>> callback)
        {
            yield break;
        }
    }
}
