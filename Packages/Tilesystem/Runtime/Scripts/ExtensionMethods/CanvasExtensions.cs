using UnityEngine;

public enum SizeRef { WIDTH, HEIGHT }

public static class CanvasExtensions
{
    public static void ScaleWithAspectRatio(this RectTransform rect, float refSize, SizeRef sizeRef = SizeRef.WIDTH)
    {
        float aspectRatio = rect.rect.width / rect.rect.height;
        Vector2 newDimensions;
        switch (sizeRef)
        {
            case SizeRef.WIDTH:
                newDimensions = new Vector2(refSize, refSize / aspectRatio);
                break;
            case SizeRef.HEIGHT:
                newDimensions = new Vector2(refSize * aspectRatio, refSize);
                break;
            default:
                throw new System.Exception("Could not calculate dimensions, due to missing SizeRef");
        }
        rect.sizeDelta = newDimensions;
    }



}
