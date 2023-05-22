using Netherlands3D.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingOriginFollower : MonoBehaviour
{
    private Vector3ECEF ecefPosition;

    void Start()
    {
        StartCoroutine(DelayListeners());
    }

    private void SaveOrigin()
    {
        ecefPosition = CoordConvert.UnityToECEF(transform.position);
    }

    /// <summary>
    /// Store current Unity coordinate as late as possible.
    /// This way any other systems placing this object have finished their possible manipulations
    /// </summary>
    IEnumerator DelayListeners()
    {
        yield return new WaitForEndOfFrame();

        CoordConvert.prepareForOriginShift.AddListener(SaveOrigin);
        CoordConvert.relativeOriginChanged.AddListener(MoveToNewOrigin);
    }

    private void OnDestroy()
    {
        CoordConvert.relativeOriginChanged.RemoveListener(MoveToNewOrigin);
    }

    private void MoveToNewOrigin(Vector3 offset)
    {
        transform.SetPositionAndRotation(
            CoordConvert.ECEFToUnity(ecefPosition),
            CoordConvert.ecefRotionToUp()
        );
    }
}
