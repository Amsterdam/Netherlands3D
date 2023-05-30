/*
*  Copyright (C) X Gemeente
*                X Amsterdam
*                X Economic Services Departments
*
*  Licensed under the EUPL, Version 1.2 or later (the "License");
*  You may not use this work except in compliance with the License.
*  You may obtain a copy of the License at:
*
*    https://github.com/Amsterdam/3DAmsterdam/blob/master/LICENSE.txt
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" basis,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
*  implied. See the License for the specific language governing
*  permissions and limitations under the License.
*/

using System.Collections;
using UnityEngine;

namespace Netherlands3D.Coordinates
{
    public class MovingOriginFollower : MonoBehaviour
    {
        private Vector3ECEF ecefPosition;

        void Start()
        {
            StartCoroutine(DelayListeners());
        }

        private void SaveOrigin()
        {
            ecefPosition = CoordinateConverter.UnityToECEF(transform.position);
        }

        /// <summary>
        /// Store current Unity coordinate as late as possible.
        /// This way any other systems placing this object have finished their possible manipulations
        /// </summary>
        IEnumerator DelayListeners()
        {
            yield return new WaitForEndOfFrame();

            MovingOrigin.prepareForOriginShift.AddListener(SaveOrigin);
            MovingOrigin.relativeOriginChanged.AddListener(MoveToNewOrigin);
        }

        private void OnDestroy()
        {
            MovingOrigin.relativeOriginChanged.RemoveListener(MoveToNewOrigin);
        }

        private void MoveToNewOrigin(Vector3 offset)
        {
            transform.position = CoordinateConverter.ECEFToUnity(ecefPosition);
        }
    }
}
