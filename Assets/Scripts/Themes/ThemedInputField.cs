using System.Collections;
using System.Collections.Generic;
using TMPro;
using TriInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ThemedInputField : ThemedElement
{
    protected TMP_InputField inputField;

    protected override void _UpdateElement()
    {
        FindElements();
        UpdateColor();
    }

    protected override void FindElements()
    {
        inputField = GetComponent<TMP_InputField>();
    }

    [Button]
    public void UpdateColor()
    {
        // first we retrieve our color
        Color color = ThemeController.GetColor(themeColor);

        // then we apply it to the proper component
        if (inputField.targetGraphic is not null)
        {
            inputField.targetGraphic.color = color;
        }
        
        /*var colors = inputField.colors;
        colors.normalColor = color;
        colors.disabledColor = color;
        colors.highlightedColor = color;
        colors.pressedColor = color;
        colors.selectedColor = color;
        inputField.colors = colors;*/
    }
}
