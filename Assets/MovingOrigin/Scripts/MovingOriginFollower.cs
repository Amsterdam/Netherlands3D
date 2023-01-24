using Netherlands3D.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingOriginFollower : MonoBehaviour
{
    private Vector3RD initialRDCoordinate = new Vector3RD();

    void Start()
    {
        StartCoroutine(StoreCurrentRD());
    }

    /// <summary>
    /// Store current Unity coordinate as late as possible.
    /// This way any other systems placing this object have finished their possible manipulations
    /// </summary>
    IEnumerator StoreCurrentRD()
    {
        yield return new WaitForEndOfFrame();

        //Store RD coordinate
        initialRDCoordinate = CoordConvert.UnitytoRD(this.transform.position);

        CoordConvert.relativeCenterChanged.AddListener(MoveToNewRD);
    }

    private void OnDestroy()
    {
        CoordConvert.relativeCenterChanged.RemoveListener(MoveToNewRD);
    }

    private void MoveToNewRD(Vector3 position, Quaternion rotation)
    {
        this.transform.position = CoordConvert.RDtoUnity(initialRDCoordinate);

        //Optional TODO: Hide/disable object if too far from RD coordinates
        //TODO: Rotation
    }
}
