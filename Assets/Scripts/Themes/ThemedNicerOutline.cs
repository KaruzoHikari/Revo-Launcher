using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using TriInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class ThemedNicerOutline : ThemedElement
{
    private NicerOutline outline;

    protected override void _UpdateElement()
    {
        FindElements();
        UpdateColor();
    }

    protected override void FindElements()
    {
        if (outline is not null)
        {
            return;
        }
        outline = GetComponent<NicerOutline>();
    }

    [Button]
    public void UpdateColor()
    {
        // first we retrieve our color
        Color color = ThemeController.GetColor(themeColor);
        
        // then we apply it to the proper component
        outline.effectColor = color;
    }
}
