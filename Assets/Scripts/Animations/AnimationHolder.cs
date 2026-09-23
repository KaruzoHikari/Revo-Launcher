using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.Video;

public class AnimationHolder : MonoBehaviour
{
    public AnimatedImage animationInfo;
    public RawImage rawImage;
    public VideoPlayer video;
    public TextMeshProUGUI text;
    public Channel currentChannel;
    public bool isPreparing = false;
    public bool finishedVideo = false;
    public int gifFrame = 0;
    public float gifDelay = 0;
    private bool isFrozen = false;

    public void Setup()
    {
        if (animationInfo.isVideo)
        {
            video = gameObject.AddComponent<VideoPlayer>();
            video.playOnAwake = false;
            video.renderMode = VideoRenderMode.APIOnly;
            video.audioOutputMode = VideoAudioOutputMode.None;
            video.playbackSpeed = animationInfo.GetPlaybackSpeed();
        }
    }

    public void AddText()
    {
        GameObject obj = Instantiate(AnimationController._instance.holderTextPrefab, transform);
        text = obj.GetComponent<TextMeshProUGUI>();
    }

    public void LinkChannel(Channel channel)
    {
        this.currentChannel = channel;
    }

    public void PrepareVideo()
    {
        isPreparing = true;
        video.Prepare();
    }

    public void CheckDebugText()
    {
        if (animationInfo.Equals(PreviewController._instance.currentDebugImage))
        {
            PreviewController._instance.maxDebugText.gameObject.SetActive(true);
            PreviewController._instance.maxDebugText.text = "MAX: " + (Math.Floor(video.length * 100) / 100f);
        }
    }

    public void UpdateGif()
    {
        if (animationInfo.isGif && !isFrozen)
        {
            gifDelay += (Time.deltaTime * animationInfo.GetPlaybackSpeed());
            float targetDelay = animationInfo.gifFrameDelays[gifFrame];
            if (gifDelay >= targetDelay)
            {
                if (gifFrame >= animationInfo.gifFrames.Count - 1)
                {
                    // Here we've reached the end of the GIF
                    if (animationInfo.freezeLastFrame)
                    {
                        isFrozen = true;
                    }
                    else
                    {
                        RefreshGif();
                    }
                }
                else
                {
                    // Here there's still more GIFs
                    gifFrame++;
                    rawImage.texture = animationInfo.gifFrames[gifFrame];
                }
                gifDelay = 0;
            }
        }
    }

    public void RefreshGif()
    {
        if (animationInfo.isGif)
        {
            gifFrame = 0;
            gifDelay = 0;
            isFrozen = false;
            if (animationInfo.HasGifFrames())
            {
                rawImage.texture = animationInfo.gifFrames[0];
            }
        }
    }
}
