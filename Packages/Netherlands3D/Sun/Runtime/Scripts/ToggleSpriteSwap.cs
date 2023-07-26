using System;
using UnityEngine;
using UnityEngine.UI;

public class ToggleSpriteSwap : MonoBehaviour
{
    [SerializeField] private Toggle toggle;
    [SerializeField] private Image imageToSwap;
    [SerializeField] private Sprite startSprite;
    [SerializeField] private Sprite swappedSprite;

    // Start is called before the first frame update
    void Start()
    {
        if (toggle == null)
        {
            throw new NullReferenceException("Toggle hasn't been set in the inspector!");
        }

        toggle.onValueChanged.AddListener(SwapSprites);
    }

    private void SwapSprites(bool isActive)
    {
        imageToSwap.sprite = isActive ? startSprite : swappedSprite;
    }
}
