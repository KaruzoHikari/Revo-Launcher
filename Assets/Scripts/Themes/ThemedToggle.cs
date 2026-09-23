using System.Collections;
using System.Collections.Generic;
using TMPro;
using TriInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ThemedToggle : ThemedElement
{
    protected Toggle toggle;

    protected override void _UpdateElement()
    {
        FindElements();
        UpdateColor();
    }
    
    protected override void FindElements()
    {
        if (toggle is not null)
        {
            return;
        }
        toggle = GetComponent<Toggle>();
    }

    [Button]
    public void UpdateColor()
    {
        // first we retrieve our color
        Color color = ThemeController.GetColor(themeColor);

        // then we apply it to the proper component
        if (toggle.targetGraphic is not null)
        {
            toggle.targetGraphic.color = color;
        }
        
        /*toggle.targetGraphic.color = color;
        
        var colors = toggle.colors;
        colors.normalColor = color;
        colors.disabledColor = color;
        colors.highlightedColor = color;
        colors.pressedColor = color;
        colors.selectedColor = color;
        toggle.colors = colors;*/
    }
}
