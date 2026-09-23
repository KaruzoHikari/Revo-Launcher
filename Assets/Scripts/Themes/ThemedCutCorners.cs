using System.Collections;
using System.Collections.Generic;
using TMPro;
using TriInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class ThemedCutCorners : ThemedElement
{
    protected UICornerCut corner;

    protected override void _UpdateElement()
    {
        FindElements();
        UpdateColor();
    }

    protected override void FindElements()
    {
        corner = GetComponent<UICornerCut>();
    }

    [Button]
    public void UpdateColor()
    {
        // first we retrieve our color
        Color color = ThemeController.GetColor(themeColor);

        // then we apply it to the proper component
        corner.color = color;
    }
}
