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

public class TextureRendererProjector : TextureProjectorBase
{
    [SerializeField] private Renderer projectorRenderer;

    /// <summary>
    /// Sets the size of the projection area
    /// </summary>
    /// <param name="size">Box size of projector</param>
    public override void SetSize(Vector3 size) {
        transform.localScale = size;
    }

    /// <summary>
    /// Replace the projection texture on this projector
    /// </summary>
    /// <param name="texture"></param>
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
