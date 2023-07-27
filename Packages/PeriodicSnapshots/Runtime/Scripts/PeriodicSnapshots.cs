using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using Netherlands3D.Sun;
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.Snapshots
{
    public class PeriodicSnapshots : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern void DownloadSnapshot(byte[] array, int byteLength, string fileName);

        [Serializable]
        public class Moment
        {
            [HideInInspector]
            public string name = "";

            [Range(1, 31)]
            public int day;
            [Range(1, 12)]
            public int month;
            [Range(0, 23)]
            public int hour;

            public DateTime ToDateTime()
            {
                return new DateTime(DateTime.Now.Year, month, day, hour, 0, 0);
            }
        }

        [SerializeField] private Camera sourceCamera;
        [SerializeField] private SunTime sunTime;
        [SerializeField] private int snapshotWidth = 1024;
        [SerializeField] private int snapshotHeight = 768;
        [SerializeField] private LayerMask snapshotLayers;
        [SerializeField] private List<Moment> moments = new();

        [Tooltip("Generating can take a while, this event can be used to show a loader")]
        public UnityEvent onStartGenerating = new();

        [Tooltip("Generating can take a while, this event can be used to show the progress while generating")]
        public UnityEvent<float> onProgress = new();

        [Tooltip("Generating can take a while, this event can be used to hide a loader")]
        public UnityEvent onFinishedGenerating = new();

        private void Start()
        {
            if (!sourceCamera) sourceCamera = Camera.main;
        }

        private void OnValidate()
        {
            foreach (var moment in moments)
            {
                moment.name = $"{moment.day}-{moment.month} {moment.hour}:00";
            }
        }

        public void TakeSnapshots()
        {
            string timestamp = $"{DateTime.Now:yyyy-MM-ddTHH-mm}";
            var path = FetchPath(timestamp);

            StartCoroutine(TakeSnapshotsAcrossFrames(timestamp, path));
        }

        public void DownloadSnapshots()
        {
            string timestamp = $"{DateTime.Now:yyyy-MM-ddTHH-mm}";
            var path = FetchPath(timestamp);

            StartCoroutine(DownloadSnapshots(timestamp, path));
        }

        private IEnumerator DownloadSnapshots(string timestamp, string path)
        {
            yield return TakeSnapshotsAcrossFrames(timestamp, path);

#if UNITY_WEBGL && !UNITY_EDITOR
            var archivePath = FetchArchivePath(timestamp);
            var bytes = File.ReadAllBytes(archivePath);
            DownloadSnapshot(bytes, bytes.Length, Path.GetFileName(archivePath));
#endif
        }

        private IEnumerator TakeSnapshotsAcrossFrames(string timestamp, string path)
        {
            onStartGenerating.Invoke();

            var cachedTimeOfDay = sunTime.GetTime();
            for (var index = 0; index < moments.Count; index++)
            {
                onProgress.Invoke(1f / moments.Count * (index + 1));

                yield return TakeSnapshot(moments[index], path);
            }
            sunTime.SetTime(cachedTimeOfDay);

            var archiveFilePath = FetchArchivePath(timestamp);
            if (File.Exists(archiveFilePath)) File.Delete(archiveFilePath);

            ZipFile.CreateFromDirectory(path, archiveFilePath);
            Directory.Delete(path, true);

            // Make sure no rounding errors occur and set it to 1f
            onProgress.Invoke(1f);

            onFinishedGenerating.Invoke();
        }

        private static string FetchArchivePath(string timestamp)
        {
            return $"{Application.persistentDataPath}{Path.DirectorySeparatorChar}snapshot-series-{timestamp}.zip";
        }

        private static string FetchPath(string timestamp)
        {
            string path = $"{Application.persistentDataPath}{Path.DirectorySeparatorChar}snapshot-series-{timestamp}";
            if (Directory.Exists(path))
            {
                throw new Exception(
                    $"Path '{path}' should not exist, aborting taking a burst of snapshots to prevent inadvertent data loss"
                );
            }

            Directory.CreateDirectory(path);

            return path;
        }

        private IEnumerator TakeSnapshot(Moment moment, string path)
        {
            sunTime.SetDay(moment.day);
            sunTime.SetMonth(moment.month);
            sunTime.SetHour(moment.hour);
            sunTime.SetMinutes(0);
            sunTime.SetSeconds(0);

            // Skip frame to give rendering time to do its magic
            yield return null;

            byte[] bytes = Snapshot.ToImageBytes(
                snapshotWidth,
                snapshotHeight,
                sourceCamera,
                snapshotLayers,
                SnapshotFileType.png
            );

            DateTime dateTime = moment.ToDateTime();
            File.WriteAllBytes($"{path}{Path.DirectorySeparatorChar}{dateTime:yyyy-MM-ddTHH-mm}.png", bytes);
        }
    }
}
