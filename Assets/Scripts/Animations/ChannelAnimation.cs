using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Animations;
using Newtonsoft.Json;
using UnityEngine;
using Object = System.Object;

public class ChannelAnimation : ContentContainer, HasOptions
{
    public CHANNELTYPE type;
    public List<AnimatedImage> images = new List<AnimatedImage>();
    public AnimatedImage background;

    [JsonIgnore] public List<Channel> linkedChannels = new List<Channel>();
    
    // This variable is only for icons;
    public float duration = 5f;
    
    // This variable is only for banners;
    public bool shouldLoopAudio = false;
    public float loopPoint = 0;
    [JsonIgnore] public AudioClip audioClip;
    [JsonIgnore] public string audioPath;
    private bool finishedLoadingAudio;

    // This is for info retrieved from online sources
    [JsonIgnore] public bool isTemp;
    
    // Temp fix so icon animations don't tick more than once
    [JsonIgnore] public bool tickedThisFrame;

    private bool isUnloaded = false;

    public ChannelAnimation(CHANNELTYPE type)
    {
        this.type = type;
    }

    public override void RunFirstSetup()
    {
        // First we setup the name
        name = "New " + type;
        
        // Then the rest of the values
        base.RunFirstSetup();

        // Then we setup the background
        SetupBackground();

        // We mark it as loaded since there's nothing else to load
        MarkAsFinishedLoading();
    }

    public AnimationBasicInfo GetBasicInfo()
    {
        return new AnimationBasicInfo
        {
            animationName = name,
            animationFileName = importedFileName,
            animationLastEdited = lastEdited,
            isOnline = onlineInfo != null,
            isEmulator = SaveManager.IsBoxArt(importedFileName)
        };
    }

    private void SetupBackground()
    {
        background = new AnimatedImage();
        background.channelAnimation = this;
        background.layer = -100;
        background.initialLocalPosition = new Vector3(0, 0, 1000);
        background.imageName = "Background";
        background.initialLocalScale = type == CHANNELTYPE.ICON ? new Vector3(1024, 1024, 1) : new Vector3(1080, CameraController.GetFixedHeight(), 1);
        background.finishedLoading = true;
    }

    public float GetTimelineTime()
    {
        if (type == CHANNELTYPE.BANNER)
        {
            return AnimationController._instance.channelTimer;
        }
        else
        {
            if (this.Equals(PreviewController._instance.currentAnimation))
            {
                return AnimationController._instance.channelTimer % duration;
            }

            float globalTimer = AnimationController._instance.globalTimer;
            return globalTimer < 0 ? -1f : globalTimer % duration;
        }
    }

    public void PlayAudio()
    {
        AudioController.PlayMainAudio(audioClip, shouldLoopAudio, loopingPoint: loopPoint);
    }

    public void SetChannelType(CHANNELTYPE newType)
    {
        this.type = newType;
        if (type == CHANNELTYPE.ICON)
        {
            audioClip = null;
            audioPath = null;
        }
    }

    public void AddImage(AnimatedImage image)
    {
        image.channelAnimation = this;
        images.Add(image);
    }

    public void DeleteImage(AnimatedImage image)
    {
        image.Destroy();
        images.Remove(image);
    }

    public void UnloadIfUnused()
    {
        if (type == CHANNELTYPE.BANNER || linkedChannels.Count == 0)
        {
            // we only unload icons if they don't have any linked channel, or if they're all disabled
            Unload();
        }
    }

    public void Unload()
    {
        if (isUnloaded)
        {
            return;
        }
        
        // we unload the images and the audio
        isUnloaded = true;
        Debug.Log($"Unloading {type.ToString().ToUpperInvariant()}: {name}");
        images.ForEach(im => im.Unload());
        background.Unload();
        GameObject.Destroy(audioClip);
        linkedChannels.ForEach(channel =>
        {
            if (type == CHANNELTYPE.BANNER)
            {
                channel.bannerAnimation = null;   
            }
            else
            {
                channel.iconAnimation = null;
            }
        });
        ChannelController._instance.UnlistAnimation(this);
    }

    [OnDeserialized]
    internal void OnDeserializedMethod(StreamingContext context)
    {
        SyncContext.RunOnUnityThread(() =>
        {
            foreach (AnimatedImage image in images)
            {
                image.channelAnimation = this;
                //image.CheckCacheTexture();
            }
        
            background.channelAnimation = this;
            background.initialLocalScale = type == CHANNELTYPE.ICON ? new Vector3(1024, 1024, 1) : new Vector3(1080, CameraController.GetFixedHeight(), 1);
            //background.CheckCacheTexture();
        });
    }

    public void Pause()
    {
        foreach(AnimatedImage image in images)
        {
            image.Pause();
        }
    }

    public void Resume()
    {
        foreach(AnimatedImage image in images)
        {
            image.Resume();
        }
    }

    public void Update()
    {
        foreach(AnimatedImage image in images)
        {
            image.Update();
        }
        background?.Update();
    }

    public void Stop()
    {
        foreach (AnimatedImage image in images)
        {
            image.Destroy();
        }
        background?.Destroy();
    }

    public void ClearImages()
    {
        foreach(AnimatedImage animatedImage in new List<AnimatedImage>(images))
        {
            DeleteImage(animatedImage);
        }
    }

    public void SetupImages()
    {
        foreach(AnimatedImage animatedImage in images)
        {
            animatedImage.SetupImage(deserializedPath);
        }
    }

    public void SaveImages(string extractedPath)
    {
        foreach(AnimatedImage animatedImage in images)
        {
            animatedImage.SaveImage(extractedPath);
        }
    }

    public void LoadAudio()
    {
        string path = FindAudioName(deserializedPath);
        if (!string.IsNullOrEmpty(path) && File.Exists(path))
        {
            audioPath = path;

            SyncContext.RunOnUnityThread(() =>
            {
                AudioController.LoadAudio(path, audio =>
                {
                    audioClip = audio;
                    finishedLoadingAudio = true;
                });
            });
        }
        else
        {
            finishedLoadingAudio = true;
        }
    }

    private string FindAudioName(string extractedPath)
    {
        string[] extensions = {".wav", ".bin", ".mp3", ".ogg"};
        for (int i = 0; i < extensions.Length; i++)
        {
            string path = extractedPath + "/audio" + extensions[i];
            if (File.Exists(path))
            {
                return path;
            }
        }

        return null;
    }

    public void SaveAudio(string extractedPath)
    {
        if (!string.IsNullOrEmpty(audioPath))
        {
            FileManager.CopyFile(audioPath, extractedPath + "/audio" + Path.GetExtension(audioPath));
            audioPath = MoveToDeserializedFolder(audioPath) ?? audioPath;
        }
    }

    public bool FinishedLoading()
    {
        foreach (AnimatedImage image in images)
        {
            if (!image.finishedLoading)
            {
                return false;
            }
        }
        
        return finishedLoadingAudio;
    }

    public bool Save(bool asCopy = false)
    {
        Debug.Log("Saving channel animation!");
        try
        {
            string oldName = GetImportedFileName();
            bool shouldDeleteOld = !string.IsNullOrEmpty(requestedNewName) && !requestedNewName.Equals(name);
        
            // If the name has been changed, we need to remove the previous files
            if(!asCopy && shouldDeleteOld) {
                SaveManager.DeleteChannelAnimationFile(oldName);
            }

            AssignSaveValues();

            // Then, we save the current one
            SaveManager.SaveChannelAnimation(this);
            importedFileName = GetCurrentFileName();

            if (!asCopy)
            {
                // Finally, we update the channels to the new file name
                foreach (Channel channel in linkedChannels)
                {
                    channel.Save();
                }
            }

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            PopupController.ShowPopup("popup.savingerror");
            return false;
        }
    }

    public override string GetCurrentFileName()
    {
        return type.ToString().ToLowerInvariant() + "_" + GetValidFileName() + "_" + id + ".zip";
    }

    public bool IsOnlineAnimation()
    {
        return onlineInfo != null;
    }

    public void MarkAsFinishedLoading()
    {
        finishedLoadingAudio = true;
        // images should load on their own
    }

    public List<Option> GetOptions()
    {
        List<Option> optionList = new List<Option>();
        optionList.Add(Option.Create("editor.title.animname", () => LatestName, x => requestedNewName = x));
        if (type == CHANNELTYPE.ICON)
        {
            optionList.Add(Option.Create("editor.title.loopduration", () => duration, x => duration = x));
        }
        optionList.Add(Option.Create("editor.title.bgcolor", () => background.initialColor, x => background.initialColor = x));
        if (type == CHANNELTYPE.BANNER)
        {
            //optionList.Add(Option.Create("Should Cache Textures?", () => shouldCacheTextures, x => shouldCacheTextures = x));
            optionList.Add(Option.Create("editor.title.loopaudio", () => shouldLoopAudio, x => shouldLoopAudio = x));
            optionList.Add(Option.Create("editor.title.loopaudiopoint", "editor.description.loopaudiopoint", () => loopPoint, x => loopPoint = x));
        }
        return optionList;
    }
}
