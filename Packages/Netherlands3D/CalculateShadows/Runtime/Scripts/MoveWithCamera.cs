using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveWithCamera : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!resultShown && moveWithCamera)
        {
            transform.position = new Vector3(mainCam.transform.position.x, 200, mainCam.transform.position.z);

            transform.localRotation = mainCam.transform.localRotation;
            transform.localRotation = Quaternion.Euler(new Vector3(90, transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z));

            transform.position = transform.position + transform.up * 800;

            areaFrame.transform.position = new Vector3(areaFrame.transform.position.x, 120, areaFrame.transform.position.z);
        }
    }
}
