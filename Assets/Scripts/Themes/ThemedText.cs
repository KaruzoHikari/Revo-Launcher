using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using TriInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ThemedText : ThemedElement
{
    protected TranslatedText translatedText;
    protected TextMeshProUGUI text;
    public TMP_FontAsset originalFont;
    private float originalLineSpacing;
    protected string fontName;
    public bool tintSprites = false;
    public bool preserveTransparency = false;

    protected override void _UpdateElement()
    {
        FindElements();
        UpdateColor();
        UpdateFont();

        if (translatedText != null)
        {
            translatedText.Refresh();
        }
    }

    protected override void FindElements()
    {
        if (!string.IsNullOrEmpty(fontName))
        {
            return;
        }
        text = GetComponent<TextMeshProUGUI>();
        translatedText = GetComponent<TranslatedText>();
        originalLineSpacing = text.lineSpacing;
        if (originalFont == null)
        {
            originalFont = text.font;
        }
        if (originalFont != null)
        {
            fontName = originalFont.name;
        }
    }

    [Button]
    public void UpdateColor()
    {
        // first we retrieve our color
        Color color = ThemeController.GetColor(themeColor);
        
        // then we apply it to the proper component
        if (preserveTransparency)
        {
            color = new Color(color.r, color.g, color.b, text.color.a);
        }
        text.color = color;
        
        // finally, we tint the sprites if present with regex, "yay"
        if (tintSprites && !string.IsNullOrEmpty(text.text))
        {
            string parsedColor = ColorUtility.ToHtmlStringRGBA(color);
            string pattern = @"(color=#[a-fA-F0-9]{8}|color=#[a-fA-F0-9]{6})\>";
            string substitution = $@"color=#{parsedColor}>";
            RegexOptions options = RegexOptions.Multiline;
        
            Regex regex = new Regex(pattern, options);
            text.text = regex.Replace(text.text, substitution);
        }
    }

    protected virtual void UpdateFont()
    {
        // first we try to find a special font for our color
        ThemeFont specialFont = ThemeController.GetThemeFont(themeColor);
        if (specialFont is not null)
        {
            text.font = specialFont.font;
            text.lineSpacing = originalLineSpacing < 0 ? 0f : originalLineSpacing;
        }
        else
        {
            ThemeFont font = ThemeController.GetThemeFont(fontName);
            if (font != null)
            {
                text.font = font.font;
                text.lineSpacing = originalLineSpacing < 0 ? 0f : originalLineSpacing;
            }
            else
            {
                text.font = originalFont;
                text.lineSpacing = originalLineSpacing;
                // todo kaz maybe we can fix with "font properties" added at the bottom?
            }
        }
    }
}
