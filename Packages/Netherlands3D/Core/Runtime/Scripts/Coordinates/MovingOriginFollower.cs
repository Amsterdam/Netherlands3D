using Netherlands3D.Core;
using System.Collections;
using UnityEngine;

public class MovingOriginFollower : MonoBehaviour
{
    
    public Vector3ECEF ecefPosition;
    private Quaternion rotation;
    SetGlobalRDOrigin globalOrigin;
    Camera connectedCamera;

    void Start()
    {
        globalOrigin = FindObjectOfType<SetGlobalRDOrigin>();
        if (globalOrigin!=null)
        {
            globalOrigin.prepareForOriginShift.AddListener(SaveOrigin);
            connectedCamera = GetComponent<Camera>();
            if (connectedCamera!= null)
            {
                globalOrigin.relativeOriginChanged.AddListener(MoveCamera);
            }
            else
            {
                globalOrigin.relativeOriginChanged.AddListener(MoveAndRotateGameObject);
            }
        }
       
    }

    public void SetPosition(double X, double Y, double Z)
    {
        this.ecefPosition = new Vector3ECEF(X,Y,Z);
        rotation = transform.rotation;
        MoveAndRotateGameObject(Vector3.zero);
    }

    public void SetPositionAndRotation(double X, double Y, double Z,Quaternion defaultRotation)
    {
        this.ecefPosition = new Vector3ECEF(X, Y, Z);
        rotation = defaultRotation;
        MoveAndRotateGameObject(Vector3.zero);
    }
    private void SaveOrigin()
    {
        ecefPosition = CoordConvert.UnityToECEF(transform.position);
        rotation =  Quaternion.Inverse(CoordConvert.ecefRotionToUp())* transform.rotation;
        
    }

    /// <summary>
    /// Store current Unity coordinate as late as possible.
    /// This way any other systems placing this object have finished their possible manipulations
    /// </summary>
    IEnumerator DelayListeners()
    {
        yield return new WaitForEndOfFrame();

        
    }

    private void OnDestroy()
    {
        if (globalOrigin != null)
        {
            globalOrigin.relativeOriginChanged.RemoveListener(MoveAndRotateGameObject);
            globalOrigin.prepareForOriginShift.RemoveListener(SaveOrigin);
        }
    }

    private void MoveAndRotateGameObject(Vector3 offset)
    {

        transform.SetPositionAndRotation(
            CoordConvert.ECEFToUnity(ecefPosition),
            CoordConvert.ecefRotionToUp()*rotation
        );

    }
    private void MoveCamera(Vector3 offset)
    {
        if (connectedCamera == Camera.main)
        {
            transform.position = CoordConvert.ECEFToUnity(ecefPosition);
        }
        else
        {
            transform.SetPositionAndRotation(
           CoordConvert.ECEFToUnity(ecefPosition),
           rotation * CoordConvert.ecefRotionToUp()
       );
        }
        
    }
}
