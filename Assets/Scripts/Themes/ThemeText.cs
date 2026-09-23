using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TextCore;

public class ThemeText
{
    public SystemLanguage language;
    public string filePath;
    [JsonIgnore] private Dictionary<string, string> texts = null; // this only loads when required, usually only 1 loaded

    public ThemeText(SystemLanguage lang)
    {
        this.language = lang;
    }
    
    public void LoadText(string extractedPath)
    {
        // Now we load it
        filePath = extractedPath + Path.DirectorySeparatorChar + language.ToString().ToLowerInvariant() + ".json";
    }

    public Dictionary<string, string> GetDictionary()
    {
        if (texts is null)
        {
            texts = new Dictionary<string, string>();
            TextController._instance.ProcessLanguage(filePath, texts);
        }

        return texts;
    }

    public void Unload()
    {
        texts?.Clear();
    }

    public void SaveText(string extractedPath)
    {
        string newPath = extractedPath + "/" + language.ToString().ToLowerInvariant() + ".json";
        if (texts is null)
        {
            // it was never loaded, we just copy the file
            if (!File.Exists(newPath) && !string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                FileManager.CopyFile(filePath, newPath, false);
            }
        }
        else
        {
            // it was loaded, we save the new one only if it has new files (they might have loaded it by mistake)
            if (texts.Count > 0)
            {
                JsonSerializerSettings settings = JsonConvert.DefaultSettings.Invoke();
                settings.TypeNameHandling = TypeNameHandling.Auto;
                File.WriteAllText(newPath, JsonConvert.SerializeObject(texts, settings));
            }
        }
    }
}