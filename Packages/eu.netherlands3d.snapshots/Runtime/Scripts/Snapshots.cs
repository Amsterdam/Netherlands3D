/*
*  Copyright (C) X Gemeente
*                X Amsterdam
*                X Economic Services Departments
*
*  Licensed under the EUPL, Version 1.2 or later (the "License");
*  You may not use this work except in compliance with the License.
*  You may obtain a copy of the License at:
*
*    https://github.com/Amsterdam/3DAmsterdam/blob/master/LICENSE.txt
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" basis,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
*  implied. See the License for the specific language governing
*  permissions and limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.Snapshots
{
    public class Snapshots : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern void DownloadSnapshot(byte[] array, int byteLength, string fileName);

        [Tooltip("Optional source camera (Defaults to Camera.main)")]
        [SerializeField] private Camera sourceCamera;

        [SerializeField] private bool useViewSize = true;
        [SerializeField] private int width = 1920;
        [SerializeField] private int height = 1080;
        [SerializeField] private string targetPath = "screenshots";
        [SerializeField] private string fileName = "Snapshot";
        [SerializeField] private SnapshotFileType fileType = SnapshotFileType.png;
        [SerializeField] private LayerMask snapshotLayers;

        public int Width
        {
            get => width;
            set
            {
                useViewSize = false;
                width = value;
            }
        }

        public int Height
        {
            get => height;
            set
            {
                useViewSize = false;
                height = value;
            }
        }

        public string FileName { get => fileName; set => fileName = value; }
        public string TargetPath { get => targetPath; set => targetPath = value; }

        public string FileType
        {
            get => fileType.ToString();
            set
            {
                if (Enum.TryParse(value, out fileType) == false)
                {
                    fileType = SnapshotFileType.png;
                }
            }
        }

        public LayerMask SnapshotLayers { get => snapshotLayers; set => snapshotLayers = value;}

        private void Start()
        {
            if (!sourceCamera) sourceCamera = Camera.main;
        }

        [Obsolete("Use the Width property instead")]
        public void SetImageWidth(string width) => Width = int.Parse(width);

        [Obsolete("Use the Width property instead")]
        public void SetImageWidth(int width) => Width = width;

        [Obsolete("Use the Height property instead")]
        public void SetImageHeight(string height) => Height = int.Parse(height);

        [Obsolete("Use the Height property instead")]
        public void SetImageHeight(int height) => Height = height;

        public void UseViewSize(bool useViewSize) => this.useViewSize = useViewSize;

        /// <summary>
        /// Set the layermask for the snapshot camera ( if you want to exclude, include specific layers in the image )
        /// </summary>
        /// <param name="snapshotLayers">Include these layers in the snapshot</param>
        [Obsolete("Use the SnapshotLayers property instead")]
        public void SetLayerMask(LayerMask snapshotLayers) => SnapshotLayers = snapshotLayers;

        public void TakeSnapshot()
        {
            var snapshotWidth = (useViewSize) ? Screen.width : width;
            var snapshotHeight = (useViewSize) ? Screen.height : height;

            byte[] bytes = Snapshot.ToImageBytes(snapshotWidth, snapshotHeight, sourceCamera, snapshotLayers, fileType);

            var path = DetermineSaveLocation();

            //Use the jslib DownloadSnapshot to download the bytes as a file in WebGL/Browser
#if UNITY_WEBGL && !UNITY_EDITOR
            DownloadSnapshot(bytes, bytes.Length, Path.GetFileName(path));
#else
            File.WriteAllBytes(path, bytes);
#endif
        }

        [Obsolete("Use Snapshot.ToImageBytes instead")]
        public static byte[] SnapshotToImageBytes(
            int imageWidth,
            int imageHeight,
            string fileType = "png",
            Camera sourceCamera = null,
            LayerMask snapshotLayers = default
        ) {
            if (!sourceCamera) sourceCamera = Camera.main;

            if (Enum.TryParse(fileType, out SnapshotFileType fileTypeAsEnum) == false)
            {
                fileTypeAsEnum = SnapshotFileType.png;
            }

            return Snapshot.ToImageBytes(imageWidth, imageHeight, sourceCamera, snapshotLayers, fileTypeAsEnum);
        }

        private string DetermineSaveLocation()
        {
            var outputFileName = fileName;
            if (string.IsNullOrEmpty(outputFileName))
            {
                outputFileName = $"Snapshot_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}";
            }
            outputFileName = $"{outputFileName}.{FileType}";

            string location = Application.persistentDataPath
                + Path.DirectorySeparatorChar
                + targetPath
                + Path.DirectorySeparatorChar
                + outputFileName;

#if UNITY_EDITOR
            // Window for user to input desired path/name/filetype
            location = EditorUtility.SaveFilePanel(
                "Save texture as file",
                "",
                outputFileName,
                fileType.ToString()
            );
#endif

            return location;
        }
    }
}
