using System;
using System.Collections;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ThemedImage : ThemedElement
{
    protected RawImage rawImage;
    protected Image image;
    public bool preserveTransparency = false;
    public string fakeName; // we use this to duplicate the texture name even if it's the same texture xd

    private string textureName;
    private Sprite originalSprite;
    private Texture originalTexture;
    protected ThemeTexture themeTexture;

    protected override void FindElements()
    {
        FindElements(false);
    }

    public void FindElements(bool force)
    {
        if (!force && (rawImage is not null || image is not null))
        {
            return;
        }
        
        rawImage = GetComponent<RawImage>();
        image = GetComponent<Image>();

        if (rawImage is not null && rawImage.texture != null)
        {
            originalTexture = rawImage.texture;
            textureName = originalTexture.name;
        }
        else if (image is not null && image.sprite != null)
        {
            originalSprite = image.sprite;
            textureName = originalSprite.name;
        }
    }

    protected override void _UpdateElement()
    {
        FindElements();
        UpdateColor();
        UpdateTexture();
    }

    public Texture GetOriginalTexture()
    {
        if (originalSprite != null)
        {
            return originalSprite.texture;
        }
        return originalTexture;
    }

    [Button]
    public void UpdateColor()
    {
        // first we retrieve our color
        Color color = ThemeController.GetColor(themeColor);

        // then we apply it to the proper component
        if (image is not null)
        {
            if (preserveTransparency)
            {
                color = new Color(color.r, color.g, color.b, image.color.a);
                //color.a = image.color.a;
            }
            image.color = color;
        }
        else if (rawImage is not null)
        {
            if (preserveTransparency)
            {
                color.a = rawImage.color.a;
            }
            rawImage.color = color;
        }
    }

    [Button]
    public virtual void UpdateTexture()
    {
        // first we retrieve the theme texture that matches ours
        themeTexture = ThemeController.GetThemeTexture(GetTextureName());

        // we retrieve the replacement sprite, and exit if there's none
        Sprite sprite = null;

        /*if (image is not null && image.sprite != null)
        {
            sprite = ThemeController.GetTexture(image.sprite.texture);
        }
        else if(rawImage is not null)
        {
            sprite = ThemeController.GetTexture(rawImage.texture);
        }*/
        sprite = ThemeController.GetTexture(GetTextureName());
        
        if (sprite is null)
        {
            // in that case there's no replacement needed, we set the default texture
            if (originalSprite is not null && image is not null)
            {
                image.sprite = originalSprite;
            }
            else if(originalTexture is not null && rawImage is not null)
            {
                rawImage.texture = originalTexture;
            }
        }
        else
        {
            // otherwise, we find our proper component and update the texture
            if (image is not null)
            {
                image.sprite = sprite;
            }
            else if (rawImage is not null)
            {
                rawImage.texture = sprite.texture;
            }
        }
    }

    private void Update()
    {
        if (themeTexture is not null && themeTexture.isGif)
        {
            UpdateGif();
        }
    }

    private void UpdateGif()
    {
        if (image is not null)
        {
            image.sprite = themeTexture.texture;
        }
        else if (rawImage is not null)
        {
            rawImage.texture = themeTexture.texture.texture;
        }
    }
    
    public virtual Texture GetTexture()
    {
        FindElements();
        if (image is not null && image.sprite != null)
        {
            return image.sprite.texture;
        }
        if (rawImage is not null)
        {
            return rawImage.texture;
        }
        return null;
    }

    public string GetTextureName()
    {
        FindElements();
        // here we return the fakeName if possible, to trick the app into thinking it's a separate texture
        if (!string.IsNullOrEmpty(fakeName))
        {
            return fakeName;
        }

        return textureName;
        /*Texture texture = GetTexture();
        if (texture != null)
        {
            return texture.name;
        }
        return null;*/
    }
}
