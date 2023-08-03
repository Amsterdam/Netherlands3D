using Netherlands3D.Events;
using TMPro;
using UnityEngine;

namespace Netherlands3D.FileImporter.Samples
{
    [AddComponentMenu("File Importer/Samples/ShowFilePath")]
    public class ShowFilePath : MonoBehaviour
    {
        public StringEvent eventFileLoaderFileImported;
        public TextMeshProUGUI text;

        void Start()
        {
            eventFileLoaderFileImported.AddListenerStarted(x => text.text = x);
        }
    }
}
