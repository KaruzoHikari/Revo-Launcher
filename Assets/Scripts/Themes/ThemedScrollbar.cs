using System.Collections;
using System.Collections.Generic;
using TMPro;
using TriInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ThemedScrollbar : ThemedElement
{
    protected Scrollbar scrollbar;
    protected Image background;
    public ThemeColor backColor;

    protected override void FindElements()
    {
        scrollbar = GetComponent<Scrollbar>();
        background = GetComponent<Image>();
    }

    protected override void _UpdateElement()
    {
        FindElements();
        UpdateColor();
    }

    [Button]
    public void UpdateColor()
    {
        // we apply it to the proper components
        background.color = ThemeController.GetColor(backColor);
        Color color = ThemeController.GetColor(themeColor);

        // then we apply it to the proper component
        if (scrollbar.targetGraphic is not null)
        {
            scrollbar.targetGraphic.color = color;
        }

        /*scrollbar.targetGraphic.color = color;
        var colors = scrollbar.colors;
        var colors = scrollbar.colors;
        colors.normalColor = color;
        colors.disabledColor = color;
        colors.highlightedColor = color;
        colors.pressedColor = color;
        colors.selectedColor = color;
        scrollbar.colors = colors;*/
    }
    
    public override ThemeColor[] GetAllColors()
    {
        return new ThemeColor[] { themeColor, backColor };
    }
}
