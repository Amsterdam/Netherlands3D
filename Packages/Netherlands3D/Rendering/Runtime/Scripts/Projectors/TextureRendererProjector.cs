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
using UnityEngine;

namespace Netherlands3D.Rendering
{
    public class TextureRendererProjector : TextureProjectorBase
    {
        [SerializeField] private Renderer projectorRenderer;

        public override void SetSize(Vector3 size)
        {
            transform.localScale = size;
        }

        public override void SetTexture(Texture2D texture)
        {
            base.SetTexture(texture);

            if (!materialInstance)
            {
                materialInstance = projectorRenderer.material;
            }

            materialInstance.mainTexture = texture;
        }
    }
}
