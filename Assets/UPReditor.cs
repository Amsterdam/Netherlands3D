using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class UPReditor : MonoBehaviour

{
    ScriptableRendererData _scriptableRendererData;
    // Start is called before the first frame update
    void Start()
    {
        ExtractScriptableRendererData();
        foreach (var renderObjSetting in _scriptableRendererData.rendererFeatures.OfType<UnityEngine.Experimental.Rendering.Universal.RenderObjects>())
        {
            int buildingLayermask = LayerMask.NameToLayer("Buildings");
            LayerMask layermask = renderObjSetting.settings.filterSettings.LayerMask;

            if ((layermask.value & 1 << buildingLayermask) >0)
            {
                renderObjSetting.settings.stencilSettings.overrideStencilState = false;
            }

            //renderObjSetting.settings.cameraSettings.cameraFieldOfView = _currentFPSFov;
            //renderObjSetting.settings.cameraSettings.offset = _currentFPSOffset;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void ExtractScriptableRendererData()
    {
        var pipeline = ((UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset);
        FieldInfo propertyInfo = pipeline.GetType().GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);
        _scriptableRendererData = ((ScriptableRendererData[])propertyInfo?.GetValue(pipeline))?[0];
    }

    //bool Includes(this LayerMask mask, int layer)
    //{
    //    return (mask.value & 1 << layer) > 0;
    //}
}
