using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SLIDDES.UI
{
    public static class ContentFitterRefresh
    {        
        public static void RefreshContentFitter(this RectTransform transform)
        {
            if(transform == null || !transform.gameObject.activeSelf)
            {
                return;
            }

            foreach(RectTransform child in transform)
            {
                RefreshContentFitter(child);
            }

            var layoutGroup = transform.GetComponent<LayoutGroup>();
            var contentSizeFitter = transform.GetComponent<ContentSizeFitter>();
            if(layoutGroup != null)
            {
                layoutGroup.SetLayoutHorizontal();
                layoutGroup.SetLayoutVertical();
            }

            if(contentSizeFitter != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
            }
        }
    }
}
