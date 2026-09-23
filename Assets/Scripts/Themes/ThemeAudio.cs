using System;
using System.Collections;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class ThemeAudio
{
    public string audioClipName;
    public string audioFileName;
    public string filePath;
    [JsonIgnore] public AudioClip audioClip;
    [JsonIgnore] public bool finishedLoading;
    
    private Theme theme;

    public void LinkTheme(Theme theme)
    {
        this.theme = theme;
    }

    public void LoadAudio(string extractedPath)
    {
        // Now we load it, whether it's a video, gif or image
        filePath = extractedPath + Path.DirectorySeparatorChar + audioFileName;
        if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
        {
            SyncContext.RunOnUnityThread(() =>
            {
                AudioController.LoadAudio(filePath, audio =>
                {
                    if (audio != null)
                    {
                        audioClip = audio;
                        audioClip.name = audioClipName;
                    }
                    finishedLoading = true;
                });
            });
        }
        else
        {
            Debug.Log($"Audio file at {filePath} doesn't exist!");
            finishedLoading = true;
        }
    }

    public void SaveAudio(string extractedPath)
    {
        string newPath = extractedPath + "/" + audioFileName;
        if (!File.Exists(newPath))
        {
            FileManager.CopyFile(filePath, newPath, false);
        }
        
        filePath = theme.MoveToDeserializedFolder(filePath) ?? filePath;
    }

    public void Reload()
    {
        AudioClip currentBack = AudioController._instance.backgroundAudio.clip;
        if (currentBack != null && currentBack.name.Equals(audioClipName))
        {
            AudioController.RestartBackgroundAudio();
        }
    }

    public void Unload()
    {
        // we also reload so the background audio comes back to normal
        GameObject.Destroy(audioClip);
        audioClip = null;
    }
}