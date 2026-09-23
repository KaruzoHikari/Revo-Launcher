using System.Collections;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ThemedBackground : ThemedImage
{
    private float originalWidth;
    private float originalHeight;
    private bool loadedRect = false;

    protected override void FindElements()
    {
        base.FindElements();

        if (!loadedRect && rawImage is not null)
        {
            originalWidth = rawImage.uvRect.width;
            originalHeight = rawImage.uvRect.height;
            loadedRect = true;
        }
    }
    
    [Button]
    public override void UpdateTexture()
    {
        base.UpdateTexture();
        
        // we need to remove the tiling we did for the background, since now it's a custom texture
        if (rawImage is not null)
        {
            Sprite sprite = ThemeController.GetTexture(rawImage.texture);
            if (sprite is not null)
            {
                rawImage.uvRect = new Rect(0f, 0f, 1f, 1f);
            }
            else
            {
                rawImage.uvRect = new Rect(0f, 0f, originalWidth, originalHeight);
            }
        }
    }
}
