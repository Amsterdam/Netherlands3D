using System.Collections;
using System.Collections.Generic;
using RuntimeHandle;
using UnityEngine;

public class HandleGameObjectsContainer : MonoBehaviour
{
    private Dictionary<GameObject,RuntimeTransformHandle> handleGameObjects = new Dictionary<GameObject, RuntimeTransformHandle>();

    private void OnTransformChildrenChanged()
    {
        //Add handles to new child objects
        foreach (Transform child in transform)
        {
            if (!handleGameObjects.ContainsKey(child.gameObject))
            {
                var newHandle = RuntimeTransformHandle.Create(child, HandleType.POSITION);
                handleGameObjects.Add(child.gameObject, newHandle);
            }
        }

        //Child objects can be moved out of this parent or destroyed
        foreach(KeyValuePair<GameObject, RuntimeTransformHandle> objectAndHandle in handleGameObjects)
        {
            if (objectAndHandle.Key == null)
            {
                handleGameObjects.Remove(objectAndHandle.Key);
            }
            else if(objectAndHandle.Key.transform.parent != this.transform)
            {
                Destroy(objectAndHandle.Value);
                handleGameObjects.Remove(objectAndHandle.Key);
            }
        }
    }
}
