using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;

namespace Netherlands3D.Events
{
    public class StringEventTrigger : MonoBehaviour
    {
        public StringEvent stringEvent;

        public void call(string value)
        {
            stringEvent.InvokeStarted(value);
        }
    }
}
