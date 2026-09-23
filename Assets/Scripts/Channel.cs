using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Animations;
using Data;
using Data.ChannelTargets;
using DG.Tweening;
using Misc;
using Newtonsoft.Json;
using UnityEngine;

public class Channel
{
    [JsonIgnore] public AppHandler appHandler;
    [JsonIgnore] public ChannelAnimation iconAnimation;
    [JsonIgnore] public ChannelAnimation bannerAnimation;
    [JsonIgnore] public string iconName;
    [JsonIgnore] public string bannerName;

    public string IconAnimation
    {
        get => iconAnimation?.GetImportedFileName() ?? iconName;
        set
        {
            iconName = value;
            if (!AppController._instance.IsUltraLowMemoryMode())
            {
                //SetIconAnimation(ChannelController._instance.GetOrLoadChannelAnimation(value));
            }
        }
    }

    public string BannerAnimation
    {
        get => bannerAnimation?.GetImportedFileName() ?? bannerName;
        set
        {
            bannerName = value;
            if (!AppController._instance.IsLowMemoryMode() && !AppController._instance.IsUltraLowMemoryMode())
            {
                //SetBannerAnimation(ChannelController._instance.GetOrLoadChannelAnimation(value));
            }
        }
    }
    
    public AppInfo info;
    public int gridNumber = 0;

    public int position = -1;
    public int xPosition = 0;
    public int yPosition = 0;
    
    public long id = -1;
    public string overrideTag = null;
    public ChannelTarget target;

    [JsonIgnore] public bool isFullySetup = true;
    [JsonIgnore] public bool isDisabled = false;

    public Channel()
    {
    }
    
    // Legacy methods
    public bool ShouldSerializexPosition()
    {
        return false;
    }
    
    public bool ShouldSerializeyPosition()
    {
        return false;
    }

    public void SetAnimation(ChannelAnimation anim)
    {
        if (anim.type == CHANNELTYPE.ICON)
        {
            SetIconAnimation(anim);
        }
        else
        {
            SetBannerAnimation(anim);
        }
    }

    public void SetIconAnimation(ChannelAnimation anim)
    {
        iconAnimation = anim;
        iconName = iconAnimation?.GetImportedFileName();
        iconAnimation?.linkedChannels.Add(this);
    }

    public void SetBannerAnimation(ChannelAnimation anim)
    {
        bannerAnimation = anim;
        bannerName = bannerAnimation?.GetImportedFileName();
        bannerAnimation?.linkedChannels.Add(this);
    }

    
    public void SetTag(string tag)
    {
        overrideTag = tag;
        Save();
    }

    public string GetTag()
    {
        if (!string.IsNullOrEmpty(overrideTag))
        {
            return overrideTag;
        }
        return GetTarget().GetTag();
    }

    public ChannelTarget GetTarget()
    {
        if (target == null)
        {
            // I don't really want to mess with the serialization at this point, so I'll keep the AppInfo in this class
            target = new ChannelAppTarget();
            target.LinkChannel(this);
        }
        return target;
    }

    public void Execute()
    {
        // we execute the target with a try catch (some app intents set by the users could crash, for example)
        try
        {
            GetTarget().Execute();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    
    [OnDeserialized]
    internal void OnDeserializedMethod(StreamingContext context)
    {
        // we link the channel to the target so they can access info from here if needed
        GetTarget().LinkChannel(this);
        
        // we fix the position since it's different from the previous ones
        if (position == -1)
        {
            position = yPosition * 2 + xPosition;
        }
    }

    public ChannelAnimation GetAnimation(CHANNELTYPE type)
    {
        return type == CHANNELTYPE.ICON ? iconAnimation : bannerAnimation;
    }

    public string GetAnimationName(CHANNELTYPE type)
    {
        return type == CHANNELTYPE.ICON ? iconName : bannerName;
    }

    public void CreateID()
    {
        id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public void Save()
    {
        if (isFullySetup && target.IsValid())
        {
            SaveManager.SaveChannel(this);
        }
    }

    public void SetAppInfo(AppInfo appInfo)
    {
        // we delete the old channel scheme
        SaveManager.DeleteOldChannel(this);
        
        // and we change the info
        info = appInfo;
        Save();
    }

    public ChannelAnimation GetBannerAnimation()
    {
        if (bannerAnimation == null)
        {
            bannerAnimation = ChannelController._instance.GetOrLoadChannelAnimation(bannerName);
            bannerAnimation?.linkedChannels.Add(this);
        }
        return bannerAnimation;
    }

    public string GetFileName()
    {
        return "channel_" +
               GetIdentifier() + "_" +
               id + ".json";
    }

    private string GetIdentifier()
    {
        return GetTarget().GetTargetType().ToString().ToLowerInvariant();
    }

    public string GetOldFilename()
    {
        // we need this to delete old channel names. shouldn't have used app names as identifier, my bad
        if (info != null && GetTarget().GetTargetType() == CHANNELTARGETS.APP)
        {
            string name = info.isWindows
                ? StaticUtils.GetOnlyAlphanumeric(info.name).ToLowerInvariant().Replace(" ", "")
                : info.packageName.ToLowerInvariant().Replace(" ", "");
            return "channel_" + name + "_" + id + ".json";
        }
        return null;
    }

    public void Cover(float alpha = 0.75f, float duration = 0.25f)
    {
        DOTween.ToAlpha(() => appHandler.cover.color, x => appHandler.cover.color = x, alpha, duration);
    }

    public void Uncover(float duration = 0.25f)
    {
        DOTween.ToAlpha(() => appHandler.cover.color, x => appHandler.cover.color = x, 0f, duration);
    }

    public void LinkAnimations()
    {
        iconAnimation?.linkedChannels.Add(this);
        bannerAnimation?.linkedChannels.Add(this);
    }

    public void UnlinkAnimations()
    {
        _UnlinkAnimation(iconAnimation);
        _UnlinkAnimation(bannerAnimation);
    }

    private void _UnlinkAnimation(ChannelAnimation animation)
    {
        if(animation != null)
        {
            // We remove the linked images (if any)
            if (appHandler != null)
            {
                foreach (AnimatedImage image in animation.images)
                {
                    AnimationHolder linkedHolder = null;
                    foreach (AnimationHolder holder in image.holders)
                    {
                        if (holder.transform.IsChildOf(appHandler.transform))
                        {
                            linkedHolder = holder;
                            break;
                        }
                    }

                    image.holders.Remove(linkedHolder);
                }
            }

            // Then we remove ourselves from the channel animation
            animation.linkedChannels.Remove(this);
        }
    }
}