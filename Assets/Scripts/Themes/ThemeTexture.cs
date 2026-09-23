using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using ThreeDISevenZeroR.UnityGifDecoder;
using UnityEngine;
using UnityEngine.Networking;

public class ThemeTexture
{
    public string textureName;
    public string imageFileName;
    public string filePath;
    [JsonIgnore] public Sprite texture;
    [JsonIgnore] public bool finishedLoading;
    
    public bool isGif = false;
    public bool isVideo = false;
    [JsonIgnore] public List<Sprite> gifFrames = new List<Sprite>();
    [JsonIgnore] public List<float> gifFrameDelays = new List<float>();
    public int gifFrame = 0;
    public float gifDelay = 0;
    
    private Theme theme;

    public void LinkTheme(Theme theme)
    {
        this.theme = theme;
    }
    
    public void SetupImage(string extractedPath)
    {
        // Now we load it, whether it's a gif or image
        filePath = extractedPath + "/" + imageFileName;
        if (!File.Exists(filePath))
        {
            Debug.Log($"Texture file at {filePath} doesn't exist!");
            finishedLoading = true;
            return;
        }

        if (isVideo)
        {
            // we only need the filepath, done
            finishedLoading = true;
            return;
        }
        
        if (isGif)
        {
            SyncContext.RunOnUnityThread(LoadGif);
            return;
        }
        try
        {
            SyncContext.RunOnUnityThread(() => { AppController._instance.StartCoroutine(LoadImage(extractedPath)); });
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    
    private void LoadGif()
    {
        List<Texture2D> myFrames = new List<Texture2D>();
        StaticUtils.LoadGif(filePath, myFrames, gifFrameDelays);
        foreach(Texture2D tex in myFrames)
        {
            tex.name = textureName;
            Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            sprite.name = textureName;
            gifFrames.Add(sprite);
            tex.MarkAsNonReadable();
        }
        myFrames.Clear();
        
        texture = gifFrames.Count > 0 ? gifFrames[0] : null;
        finishedLoading = true;
    }

    private IEnumerator LoadImage(string path)
    {
        string finalPath = "file://" + path + "/" + imageFileName;
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(finalPath,true))
        {
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log(uwr.error);
                texture = null;
            }
            else
            {
                Texture2D tex = ((DownloadHandlerTexture)uwr.downloadHandler).texture;
                if (tex != null)
                {
                    texture = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                    texture.name = textureName;
                    texture.texture.name = textureName;
                }
            }

            finishedLoading = true;
        }
    }

    public void SaveImage(string extractedPath)
    {
        string newPath = extractedPath + "/" + imageFileName;
        if (!File.Exists(newPath))
        {
            FileManager.CopyFile(filePath, newPath, false);
        }

        filePath = theme?.MoveToDeserializedFolder(filePath) ?? filePath;
    }

    public void Unload()
    {
        GameObject.Destroy(texture);
        texture = null;
        gifFrameDelays.Clear();
        gifFrames.ForEach(GameObject.Destroy);
        gifFrames.Clear();
    }
    
    public void UpdateGif()
    {
        if (isGif && finishedLoading)
        {
            gifDelay += Time.deltaTime;
            float targetDelay = gifFrameDelays[gifFrame];
            if (gifDelay >= targetDelay)
            {
                if (gifFrame >= gifFrames.Count - 1)
                {
                    // Here we've reached the end of the GIF
                    RefreshGif();
                }
                else
                {
                    // Here there's still more GIFs
                    gifFrame++;
                    texture = gifFrames[gifFrame];
                }
                gifDelay = 0;
            }
        }
    }
    
    public void RefreshGif()
    {
        if (isGif)
        {
            gifFrame = 0;
            gifDelay = 0;
            if (HasGifFrames())
            {
                texture = gifFrames[0];
            }
        }
    }
    
    public bool HasGifFrames()
    {
        return gifFrames.Count != 0;
    }
}