using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TriInspector;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "Default Theme", menuName = "DefaultThemes/DefaultThemeScript", order = 1)]
public class DefaultThemeScript : ScriptableObject // was SerializedScriptableObject from Odin
{
    public string themeFileName;
    public string themeName;
    public int themeId;
    public int themeVersion;
    public Dictionary<ThemeColor, Color> colorMap = new Dictionary<ThemeColor, Color>();

    [Button(ButtonSizes.Gigantic)]
    public void UpdateDictionary()
    {
        if (colorMap is null)
        {
            colorMap = new Dictionary<ThemeColor, Color>();
        }
        
        foreach (ThemeColor themeColors in Enum.GetValues(typeof(ThemeColor)))
        {
            if (!colorMap.ContainsKey(themeColors))
            {
                colorMap[themeColors] = Color.white;
            }
        }
        
        colorMap = colorMap.OrderBy(obj => obj.Key).ToDictionary(obj => obj.Key, obj => obj.Value);
        ReloadTheme();
    }

    [Button(ButtonSizes.Gigantic)]
    public void ReloadTheme()
    {
        if (Application.isPlaying)
        {
            ThemeController._instance.TriggerThemeReload();
        }
    }

    [Button(ButtonSizes.Gigantic)]
    public void CopyFromDefaultTheme()
    {
        if (Application.isPlaying)
        {
            colorMap.Clear();
            colorMap = ThemeController._instance.defaultTheme.colorMap.ToDictionary(entry => entry.Key,
                entry => entry.Value);
            ReloadTheme();
        }
    }

    [Button]
    public void CreateTheme()
    {
        Theme theme = new Theme();
        theme.RunFirstSetup();
        theme.name = themeName;
        theme.id = themeId;
        theme.colorMap = this.colorMap.ToDictionary(entry => entry.Key, entry => entry.Value);
        theme.innerVersion = themeVersion;
        theme.isDefaultTheme = true;
        theme.Save(toStreamingAssets: true);
        Debug.Log("Saved theme successfully!");
    }

    [Button, GUIColor(1, 0, 0)]
    public void RandomizeDictionary()
    {
        if (Application.isPlaying && !ThemeController._instance.defaultTheme.Equals(this))
        {
            UpdateDictionary();
            foreach (ThemeColor themeColors in Enum.GetValues(typeof(ThemeColor)))
            {
                colorMap[themeColors] = Random.ColorHSV();
            }
            ReloadTheme();
        }
    }

    public Color GetDefaultColor(ThemeColor themeColor)
    {
        return colorMap[themeColor];
    }
}
