using Netherlands3D.Geoservice;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Netherlands3D.Geoservice
{
    public class ShowGeoServiceInfo : MonoBehaviour
    {
        [HideInInspector]
        public ServerData serverData;

        [Header("serviceInformation")]
        public Text ServiceTitle;
        public Text ServiceAbstract;
        // Start is called before the first frame update
        public void Show()
        {
            if (ServiceTitle != null) ServiceTitle.text = serverData.ServiceTitle;
            if (ServiceAbstract != null) ServiceAbstract.text = serverData.ServiceAbstract;
        }


    }
}