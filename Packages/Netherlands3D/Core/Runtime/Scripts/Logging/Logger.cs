using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Logging
{
    public class Logger : MonoBehaviour
    {
        [SerializeField] private Color logMessageColor;
        public Color LogMessageColor { get => logMessageColor; set => logMessageColor = value; }

        public void Log(LogType logType, string logMessage)
        {
            var colorHex = ColorUtility.ToHtmlStringRGB(logMessageColor);
            var message = $"<color=#{colorHex}>{logMessage}</color>";

            switch (logType)
            {
                case LogType.Error:
                    Debug.LogError(message);
                    break;
                case LogType.Assert:
                    Debug.LogAssertion(message);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(message);
                    break;
                case LogType.Log:
                    Debug.Log(message);
                    break;
                case LogType.Exception:
                    Debug.LogException(new Exception(message));
                    break;
                default:
                    break;
            }
        }
    }
}
