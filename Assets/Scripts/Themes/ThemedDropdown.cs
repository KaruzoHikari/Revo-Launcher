using System.Collections;
using System.Collections.Generic;
using TMPro;
using TriInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ThemedDropdown : ThemedElement
{
    protected TMP_Dropdown dropDown;

    protected override void _UpdateElement()
    {
        FindElements();
        UpdateColor();
    }
    
    protected override void FindElements()
    {
        if (dropDown is not null)
        {
            return;
        }
        dropDown = GetComponent<TMP_Dropdown>();
    }

    [Button]
    public void UpdateColor()
    {
        // first we retrieve our color
        Color color = ThemeController.GetColor(themeColor);

        // then we apply it to the proper component
        if (dropDown.targetGraphic is not null)
        {
            dropDown.targetGraphic.color = color;
        }
        
        /*var colors = dropDown.colors;
        colors.normalColor = color;
        colors.disabledColor = color;
        colors.highlightedColor = color;
        colors.pressedColor = color;
        colors.selectedColor = color;
        dropDown.colors = colors;*/
    }
}
