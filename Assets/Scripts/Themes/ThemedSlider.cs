using System.Collections;
using System.Collections.Generic;
using TMPro;
using TriInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ThemedSlider : ThemedElement
{
    protected Slider slider;
    protected Image background;
    protected Image knob;
    protected Image fill;
    public ThemeColor backColor;

    protected override void FindElements()
    {
        slider = GetComponent<Slider>();
        background = transform.Find("Background").GetComponent<Image>();
        fill = gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>();

        if (gameObject.transform.Find("Handle Slide Area") != null)
        {
            knob = gameObject.transform.Find("Handle Slide Area").Find("Handle").GetComponent<Image>();
        }
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
        var colors = slider.colors;
        
        // now the accent
        fill.color = ThemeController.GetColor(ThemeColor.MainAccent);
        
        // finally the knob
        Color mainColor = ThemeController.GetColor(themeColor);
        if (knob != null)
        {
            knob.color = mainColor;
        }
        
        // then we apply it to the proper component
        if (slider.targetGraphic is not null)
        {
            slider.targetGraphic.color = mainColor;
        }
        
        /*slider.targetGraphic.color = color;
        
        var colors = slider.colors;
        colors.normalColor = color;
        colors.disabledColor = color;
        colors.highlightedColor = color;
        colors.pressedColor = color;
        colors.selectedColor = color;
        slider.colors = colors;*/
    }
    
    public override ThemeColor[] GetAllColors()
    {
        return new ThemeColor[] { themeColor, backColor };
    }
}
