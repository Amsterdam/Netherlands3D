/*
*  Copyright (C) X Gemeente
*                X Amsterdam
*                X Economic Services Departments
*
*  Licensed under the EUPL, Version 1.2 or later (the "License");
*  You may not use this work except in compliance with the License.
*  You may obtain a copy of the License at:
*
*    https://github.com/Amsterdam/Netherlands3D/blob/main/LICENSE.txt
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" basis,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
*  implied. See the License for the specific language governing
*  permissions and limitations under the License.
*/

using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Netherlands3D.Rendering
{
    public class TextureDecalProjector : TextureProjectorBase
    {
        [SerializeField] private DecalProjector projector;

        public override void SetSize(Vector3 size)
        {
            projector.size = size;
        }

        public override void SetTexture(Texture2D texture)
        {
            base.SetTexture(texture);

            if (!materialInstance)
            {
                materialInstance = new Material(projector.material);
                projector.material = materialInstance;
            }

            materialInstance.mainTexture = texture;
        }

        public override void ClearTexture()
        {
            if (texture)
                Destroy(texture);
        }
    }
}
