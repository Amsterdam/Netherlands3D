using UnityEngine;

namespace SLIDDES.UI
{
    /// <summary>
    /// Extension class for RectTransform
    /// </summary>
    public static class RectTransformExtensions
    {
        public static void SetLeft(this RectTransform rt, float left)
        {
            rt.offsetMin = new Vector2(left, rt.offsetMin.y);
        }

        public static void SetRight(this RectTransform rt, float right)
        {
            rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
        }

        public static void SetTop(this RectTransform rt, float top)
        {
            rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
        }

        public static void SetBottom(this RectTransform rt, float bottom)
        {
            rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
        }

        public static void SetHeight(this RectTransform rt, float height)
        {
            rt.localPosition = new Vector3(rt.localPosition.x, height / 2, rt.localPosition.z);
            SetBottom(rt, -height);
        }

        public static void SetRect(this RectTransform rt, float top, float bottom, float left, float right)
        {
            SetTop(rt, top);
            SetBottom(rt, bottom);
            SetLeft(rt, left);
            SetRight(rt, right);
        }
    }
}