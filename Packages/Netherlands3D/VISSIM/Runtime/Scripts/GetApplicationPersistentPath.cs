using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.VISSIM
{
    // For editor purposes to get the path
    public class GetApplicationPersistentPath : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            print(Application.persistentDataPath);
        }
    }
}
