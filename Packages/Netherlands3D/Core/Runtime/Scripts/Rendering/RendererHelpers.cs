using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
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
public static class RendererHelpers
{
    private const string fieldName = "m_RendererDataList";

    public static void EnableStencilByLayerName(string layerName, bool enable)
    {
        var pipeline = ((UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset);
        FieldInfo propertyInfo = pipeline.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        var _scriptableRendererData = ((ScriptableRendererData[])propertyInfo?.GetValue(pipeline))?[0];

        foreach (var renderObjSetting in _scriptableRendererData.rendererFeatures.OfType<UnityEngine.Experimental.Rendering.Universal.RenderObjects>())
        {
            int buildingLayermask = LayerMask.NameToLayer(layerName);
            LayerMask layermask = renderObjSetting.settings.filterSettings.LayerMask;

            if ((layermask.value & 1 << buildingLayermask) > 0)
            {
                renderObjSetting.settings.stencilSettings.stencilReference = (enable) ? 1 : 0;
                _scriptableRendererData.SetDirty();
                Debug.Log(renderObjSetting.name + " set stencil set to " + renderObjSetting.settings.stencilSettings.stencilReference);
            }
        }
    }

    public static void EnableStencilByFeatureName(string featureName, bool enable)
    {
        var pipeline = ((UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset);
        FieldInfo propertyInfo = pipeline.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        var _scriptableRendererData = ((ScriptableRendererData[])propertyInfo?.GetValue(pipeline))?[0];

        foreach (var renderObjSetting in _scriptableRendererData.rendererFeatures.OfType<UnityEngine.Experimental.Rendering.Universal.RenderObjects>())
        {
            if (renderObjSetting.name == featureName)
            {
                renderObjSetting.settings.stencilSettings.stencilReference = (enable) ? 1 : 0;
                _scriptableRendererData.SetDirty();
                Debug.Log(renderObjSetting.name + " set stencil set to " + renderObjSetting.settings.stencilSettings.stencilReference);
            }
        }
    }
}