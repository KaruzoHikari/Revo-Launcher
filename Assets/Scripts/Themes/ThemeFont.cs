using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TextCore;

public class ThemeFont
{
    public string originalFontName;
    public string newFontName;
    public string fontFileName;
    public string filePath;
    public bool isSpecialFont;
    public float originalLineHeight = -1f;
    public ThemeColor specialPlace;
    [JsonIgnore] public TMP_FontAsset font;
    [JsonIgnore] public bool finishedLoading;
    
    private Theme theme;

    public void LinkTheme(Theme theme)
    {
        this.theme = theme;
    }

    public void LoadFont(string extractedPath)
    {
        // Now we load it
        filePath = extractedPath + Path.DirectorySeparatorChar + fontFileName;
        if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
        {
            Font osFont = new Font(filePath);
            font = TMP_FontAsset.CreateFontAsset(osFont);
            font.name = newFontName;
            
            // now we set the original font as the fallback
            List<TMP_FontAsset> fallback = new List<TMP_FontAsset>();
            fallback.Add(ThemeController.GetOriginalFont(originalFontName));
            font.fallbackFontAssetTable = fallback;
        }
        else
        {
            Debug.Log($"Font file at {filePath} doesn't exist!");
        }
        finishedLoading = true;
    }

    public void SaveFont(string extractedPath)
    {
        string newPath = extractedPath + "/" + fontFileName;
        if (!File.Exists(newPath))
        {
            FileManager.CopyFile(filePath, newPath, false);
        }
        filePath = theme.MoveToDeserializedFolder(filePath) ?? filePath;
    }

    public void Unload()
    {
        GameObject.Destroy(font);
        font = null;
    }
}