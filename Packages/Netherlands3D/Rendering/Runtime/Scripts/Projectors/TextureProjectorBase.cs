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
    public class TextureProjectorBase : MonoBehaviour
    {
        protected Texture2D texture;
        protected private Material materialInstance;

        /// <summary>
        /// Sets the size of the projection area by scaling the renderer object
        /// </summary>
        /// <param name="size">Box size of projector</param>
        public virtual void SetSize(Vector3 size) { }
        public virtual void SetSize(float x, float y, float z)
        {
            SetSize(new Vector3(x, y, z));
        }

        /// <summary>
        /// Replace the projection texture on this projector
        /// </summary>
        public virtual void SetTexture(Texture2D texture)
        {
            //Always start by clearing previous texture from memory
            if (this.texture) Destroy(this.texture);
        }

        /// <summary>
        /// Clear current projector texture from memory
        /// </summary>
        public virtual void ClearTexture()
        {
            if (texture)
                Destroy(texture);
        }

        public void OnDestroy()
        {
            ClearTexture();

            if (materialInstance)
                Destroy(materialInstance);
        }
    }
}
