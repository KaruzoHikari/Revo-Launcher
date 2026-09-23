using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Animations;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class AnimatedImage : HasOptions
{
    [JsonIgnore] public ChannelAnimation channelAnimation;
    [JsonIgnore] public Texture2D image;
    public string imageName;
    public bool isCodeTexture = false;

    public bool enabled = true;
    public bool freezeLastFrame;
    public bool refreshAfterEnd = true;
    public bool isIcon = false;
    public float playbackSpeed = 1f;
    
    // Video vars
    public bool isVideo = false;
    [JsonIgnore] public string filePath;
    // Gif vars
    public bool isGif = false;
    [JsonIgnore] public List<Texture2D> gifFrames = new List<Texture2D>();
    [JsonIgnore] public List<float> gifFrameDelays = new List<float>();
    
    [JsonIgnore] public bool finishedLoading = false;
    [JsonIgnore] public string decompPath;
    public int layer = 0;
    public List<Animation> animationList = new List<Animation>();
    public float startTime = 0f;
    public float endTime = -1;
    public Color initialColor = Color.white;
    public Vector3 initialLocalPosition = new Vector3(0,0,0);
    public Vector3 initialLocalScale = new Vector3(1,1,1);
    public Vector3 initialLocalRotation = new Vector3(0,0,0);
    
    [JsonIgnore] public List<AnimationHolder> holders = new List<AnimationHolder>();
    [JsonIgnore] private bool isActive = false;

    public void Reset()
    {
        initialLocalPosition = new Vector3();
        initialLocalRotation = new Vector3();
        initialLocalScale = new Vector3(1, 1, 1);
        startTime = 0;
        layer = 0;
        endTime = -1;
        playbackSpeed = 1;
        initialColor = Color.white;

        foreach (Animation animation in new List<Animation>(animationList))
        {
            DeleteAnimation(animation);
        }
    }

    public void SetImageManually(Texture2D tex)
    {
        image = tex != null ? tex : AnimationController._instance.missingDefaultTexture;
        finishedLoading = true;
    }
    
    // NEW:
    public void SetupImage(string extractedPath)
    {
        isCodeTexture = isCodeTexture || isIcon; // hotfix for icons that don't have this variables set to true
        decompPath = extractedPath;
        if (isCodeTexture || string.IsNullOrEmpty(imageName))
        {
            finishedLoading = true;
            return;
        }

        // Now we load it, whether it's a video, gif or image
        filePath = extractedPath + "/" + imageName;
        if (isVideo)
        {
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
            
            LoadImage(extractedPath);
            
            
            /*SyncContext.RunOnUnityThread(() =>
            {
                Texture2D tex = new Texture2D(2, 2); // Empty texture
                Task.Run(() =>
                {
                    LoadAsyncImage(extractedPath, tex);
                });
            });*/
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public void LoadGif()
    {
        StaticUtils.LoadGif(filePath, gifFrames, gifFrameDelays);
        gifFrames.ForEach(tex => tex.MarkAsNonReadable());
        finishedLoading = true;
    }

    private void LoadImage(string path, int attempt = 0)
    {
        SyncContext.RunOnUnityThread(() =>
        {
            AppController._instance.StartCoroutine(_LoadImage(path,attempt));
        });
    }

    // sometimes the image doesn't load properly at first but it does the second time? don't know why
    private IEnumerator _LoadImage(string path, int attempt)
    {
        // we load the image sync
        // todo we should optimize this so images with the same name only load once!
        string finalPath = "file://" + path + "/" + imageName;
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(finalPath,true))
        {
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                if (attempt > 2)
                {
                    Debug.Log(uwr.error);
                    image = AnimationController._instance.missingDefaultTexture;
                }
                else
                {
                    // we try again
                    Debug.Log("Trying to retrieve texture at " + FileManager.GetFileName(path) + " again!");
                    LoadImage(path, attempt+1);
                    yield break;
                }
            }
            else
            {
                Texture2D texture = ((DownloadHandlerTexture)uwr.downloadHandler).texture;
                image = texture;
            }
        }

        finishedLoading = true;
    }
    
    /*private static AsyncImageLoader.LoaderSettings MyJPGLoaderSettings => new AsyncImageLoader.LoaderSettings {
        linear = false,
        markNonReadable = true,
        generateMipmap = false,
        autoMipmapCount = false,
        format = AsyncImageLoader.FreeImage.Format.FIF_JPEG,
        logException = true,
    };
    private static AsyncImageLoader.LoaderSettings MyPNGLoaderSettings => new AsyncImageLoader.LoaderSettings {
        linear = false,
        markNonReadable = true,
        generateMipmap = false,
        autoMipmapCount = false,
        format = AsyncImageLoader.FreeImage.Format.FIF_PNG,
        logException = true
    };
    private static AsyncImageLoader.LoaderSettings MyUnknownLoaderSettings => new AsyncImageLoader.LoaderSettings {
        linear = false,
        markNonReadable = true,
        generateMipmap = false,
        autoMipmapCount = false,
        format = AsyncImageLoader.FreeImage.Format.FIF_UNKNOWN,
        logException = true
    };

    private async Task LoadAsyncImage(string path, Texture2D texture)
    {
        try
        {
            string finalPath = path + "/" + imageName;
            var imageData = await File.ReadAllBytesAsync(finalPath);
            AsyncImageLoader.LoaderSettings settings;
            switch (Path.GetExtension(finalPath).ToLowerInvariant())
            {
                case ".png": { settings = MyPNGLoaderSettings; break; }
                case ".jpg": case ".jpeg": { settings = MyJPGLoaderSettings; break; }
                default: { settings = MyUnknownLoaderSettings; break; }
            }

            bool success = await AsyncImageLoader.LoadImageAsync(texture, imageData, settings);
            if (!success)
            {
                Debug.Log($"Error loading async texture at {finalPath}");
                
                // we now try to load it sync
                LoadImage(path);
                return;
            }
            else
            {
                image = texture;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error! {e}");
        }
        
        finishedLoading = true;
    }*/

    public void SaveImage(string extractedPath)
    {
        if (isCodeTexture || string.IsNullOrEmpty(imageName))
        {
            return;
        }

        // We used to write the bytes, now we just copy the original file
        string newPath = extractedPath + "/" + imageName;
        if (!File.Exists(newPath))
        {
            FileManager.CopyFile(filePath, newPath, false);
        }
        
        // we move it to the deserialized folder and save! or it'll get lost when the AssetLoad folder dies
        // if it returns null, then we keep the same filepath
        filePath = channelAnimation.MoveToDeserializedFolder(filePath) ?? filePath;

        /*
        if (isVideo || isGif)
        {
            File.Copy(filePath, extractedPath + "/" + imageName);
        }
        else
        {
            File.WriteAllBytes(extractedPath + "/" + imageName, image.EncodeToPNG());
        }*/
    }

    [OnDeserialized]
    internal void OnDeserializedMethod(StreamingContext context)
    {
        foreach(Animation animation in animationList)
        {
            animation.animInfo = this;
        }
    }

    protected virtual void Summon()
    {
        if ((isVideo || isGif) && string.IsNullOrEmpty(filePath))
        {
            return;
        }

        isActive = true;
        
        foreach (AnimationHolder holder in holders)
        {
            GameObject.Destroy(holder.gameObject);
        }
        holders.Clear();

        List<Transform> parents = AnimationController._instance.GetOrAddParents(this);
        foreach (Transform parent in parents)
        {
            GameObject gameObject = GameObject.Instantiate(AnimationController._instance.animatedImagePrefab, parent);
            AnimationHolder holder = gameObject.GetComponent<AnimationHolder>();
            holders.Add(holder);
            holder.animationInfo = this;
            holder.currentChannel = GetCurrentChannel(parent);
            holder.Setup();

            Transform holderTransform = holder.transform;
            holderTransform.localPosition = initialLocalPosition;
            //holderTransform.localPosition += new Vector3(0, 0, -950f); // we push it forward 950z so it won't collide with UI
            holderTransform.localScale = initialLocalScale;
            holderTransform.localRotation = Quaternion.Euler(initialLocalRotation);

            if (!isCodeTexture)
            {
                // If it's not an icon, we have to set it to the actual image
                if (isVideo)
                {
                    holder.video.url = filePath;
                    holder.video.isLooping = !freezeLastFrame;
                }
                else if (isGif)
                {
                    if (gifFrames.Count > 0)
                    {
                        holder.RefreshGif();
                        holder.rawImage.SetNativeSize();
                    }
                }
                else
                {
                    holder.rawImage.texture = image;
                    holder.rawImage.SetNativeSize();
                }
            }
            else
            {
                if (isIcon)
                {
                    // Here we're in the editor, or there's no app selected as target
                    holder.rawImage.texture = holder.currentChannel == null || holder.currentChannel.info == null ? AnimationController._instance.iconDefaultTexture : holder.currentChannel.info.icon;
                    holder.rawImage.rectTransform.sizeDelta = new Vector2(512,512);
                }
                else
                {
                    // It's a different kind of image, the texture will be set by an external source
                    // So we set it empty
                    holder.rawImage.texture = MetadataController._instance.emptyTexture;
                }
            }

            // We set the image's alpha to 0 if there's a fade in animation that should play now
            Color animationColor = new Color(initialColor.r, initialColor.g, initialColor.b, initialColor.a);
            foreach (Animation animation in animationList)
            {
                if (animation is FadeInAnimation && (animation.IsInTime() || animation.startTime == 0 && animation.GetTimelineTime() == 0))
                {
                    animationColor.a = 0f;
                }
            }
            holder.rawImage.color = animationColor;
        }
    }

    public Channel GetCurrentChannel(Transform parent)
    {
        AppHandler appHandler = parent.GetComponentInParent<AppHandler>();
        if (appHandler != null)
        {
            // Here we're in an icon
            return appHandler.channel;
        }
        if (ChannelController._instance.currentChannel != null)
        {
            // Here we're in a banner
            return ChannelController._instance.currentChannel;
        }

        return null;
    }

    public void Destroy()
    {
        isActive = false;
        
        foreach (Animation animation in animationList)
        {
            animation.Stop();
        }

        foreach (AnimationHolder holder in holders)
        {
            try
            {
                GameObject.Destroy(holder.gameObject);
            }
            catch (Exception e)
            {
                // ignored (a gameobject might be null while the channel is being moved)
            }
        }
        holders.Clear();
    }

    public void Unload()
    {
        Destroy();
        
        // we unload all the textures!
        if (image is not null)
        {
            GameObject.Destroy(image);
        }
        gifFrames.ForEach(GameObject.Destroy);
    }

    public void AddAnimation(Animation animation)
    {
        animation.animInfo = this;
        animationList.Add(animation);
    }

    public void DeleteAnimation(Animation animation)
    {
        animationList.Remove(animation);
    }

    public bool HasGifFrames()
    {
        return gifFrames.Count != 0;
    }

    private float previousTime = -1f;
    public virtual void Update()
    {
        if (!enabled)
        {
            if (isActive)
            {
                Destroy();
            }
            return;
        }

        float time = GetTimelineTime();
        bool isTime = IsInTime();
        
        // First we check if we need to reset it
        if (refreshAfterEnd && time < previousTime && channelAnimation.type == CHANNELTYPE.ICON)
        {
            Destroy();
        }
        
        // Then we check if we're either in or out of the timeline
        if (!isActive && isTime)
        {
            Summon();
        }
        else if (isActive && !isTime)
        {
            Destroy();
        }

        // Finally, if we're active, we run all the animations
        if (isActive)
        {
            foreach (Animation animation in animationList)
            {
                animation.Update();
            }

            if (isGif && HasGifFrames() && time > 0)
            {
                foreach (AnimationHolder holder in holders)
                {
                    holder.UpdateGif();
                }
            }

            if (isVideo)
            {
                foreach (AnimationHolder holder in holders)
                {
                    if (holder.gameObject.activeInHierarchy && !holder.video.isPlaying && !holder.finishedVideo)
                    {
                        if (GetTimelineTime() > 0)
                        {
                            Color rawImageColor = holder.rawImage.color;
                            rawImageColor.a = initialColor.a;
                            holder.rawImage.color = rawImageColor;
                            holder.rawImage.texture = holder.video.texture;
                            holder.video.Play();
                            holder.video.isLooping = !freezeLastFrame;
                            holder.video.loopPointReached += source => { holder.finishedVideo = true; };
                            holder.CheckDebugText();
                        }
                        else if(!holder.isPreparing)
                        {
                            Color rawImageColor = holder.rawImage.color;
                            rawImageColor.a = 0f;
                            holder.rawImage.color = rawImageColor;
                            holder.PrepareVideo();
                        }
                    }
                }
            }
        }

        previousTime = time;
    }

    public float GetPlaybackSpeed()
    {
        return playbackSpeed < 0 ? 0f : playbackSpeed;
    }

    public void Pause()
    {
        foreach (Animation animation in animationList)
        {
            animation.Pause();
        }
        
        // If it's a video, we need to pause it as well
        if (isVideo)
        {
            foreach (AnimationHolder holder in holders)
            {
                holder.video.Pause();
            }
        }
    }

    public void Resume()
    {
        foreach (Animation animation in animationList)
        {
            animation.Resume();
        }
        
        // If it's a video, we need to resume it as well
        if (isVideo)
        {
            foreach (AnimationHolder holder in holders)
            {
                holder.video.Play();
            }
        }
    }
    
    public float GetDuration()
    {
        return endTime - startTime;
    }

    protected bool IsInTime()
    {
        float time = GetTimelineTime();
        return time >= startTime && (endTime < 0 || time < endTime);
    }

    public float GetTimelineTime()
    {
        // This time will be either a continuous time (for banner) or normalized (for icon)
        float time = channelAnimation.GetTimelineTime();
        return time;
    }

    public Texture2D GetThumbnail()
    {
        if (isVideo)
        {
            return AnimationController._instance.videoDefaultTexture;
        }
        if (isGif && HasGifFrames())
        {
            return gifFrames[0];
        }

        return image;
    }

    public string GetFileName()
    {
        if (isVideo || isGif)
        {
            return FileManager.GetFileName(filePath);
            
        }

        return imageName;
    }

    public AnimatedImage DeepClone()
    {
        // We're gonna clone it the lazy way (aka through serialization <-> deserialization)
        string extractedPath = SaveManager.TEMP_SERIALIZE + "/ImageClones/" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + "/";
        AnimatedImage cloned = null;
        try
        {
            // -- SERIALIZATION --
            Debug.Log("Deep cloning " + channelAnimation.name + ": Serializing...");

            JsonSerializerSettings settings = JsonConvert.DefaultSettings.Invoke();
            settings.TypeNameHandling = TypeNameHandling.Auto;

            if (Directory.Exists(extractedPath))
            {
                Directory.Delete(extractedPath, true);
            }
            Directory.CreateDirectory(extractedPath);

            File.WriteAllText(extractedPath + "imageclone.json", JsonConvert.SerializeObject(new DeepSerializationImage() { image = this}, settings));
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

        try {
            // -- DESERIALIZATION --
            Debug.Log("Deep cloning " + channelAnimation.name + ": Deserializing...");
            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            DeepSerializationImage deep = JsonConvert.DeserializeObject<DeepSerializationImage>(File.ReadAllText(extractedPath + "imageclone.json"), settings);
            cloned = deep.image;

            cloned.decompPath = decompPath;
            cloned.finishedLoading = finishedLoading;
            cloned.filePath = filePath;
            if (image != null)
            {
                cloned.SetImageManually(image.DuplicateTexture());
            }
            else if (isGif)
            {
                cloned.LoadGif();
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }

        return cloned;
    }

    public List<Option> GetOptions()
    {
        List<Option> optionList = new List<Option>();
        
        optionList.Add(Option.Create("editor.title.enabled", () => enabled, x => enabled = x));
        optionList.Add(Option.Create("editor.title.initpos", () => (Vector2)initialLocalPosition, x => initialLocalPosition = x));
        optionList.Add(Option.Create("editor.title.initscale", () => (Vector2)initialLocalScale, x => initialLocalScale = x));
        optionList.Add(Option.Create("editor.title.initrot", () => initialLocalRotation.z, z => initialLocalRotation = new Vector3(0, 0, z)));
        if (channelAnimation.type == CHANNELTYPE.ICON)
        {
            optionList.Add(Option.Create("editor.title.refreshafterend", () => refreshAfterEnd, x => refreshAfterEnd = x));
        }
        if (isGif || isVideo)
        {
            optionList.Add(Option.Create("editor.title.freezelastframe", () => freezeLastFrame, x => freezeLastFrame = x));
            optionList.Add(Option.Create("editor.title.speed", () => playbackSpeed, x => playbackSpeed = x));
        }
        optionList.Add(Option.Create("editor.title.layer", () => layer, x => layer = x));
        optionList.Add(Option.Create("editor.title.initcolor", () => initialColor, x => initialColor = x));
        optionList.Add(Option.Create("editor.title.starttime", () => startTime, x => startTime = x));
        optionList.Add(Option.Create("editor.title.endtime.full", () => endTime, x => endTime = x));

        return optionList;
    }
    
    private class DeepSerializationImage
    {
        // just a holder to serialize
        public AnimatedImage image;
    }
}
