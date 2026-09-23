using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Animations;
using TMPro;
using TriInspector;
using UnityEngine;
using UnityEngine.Serialization;

[DeclareFoldoutGroup("Test Banner Group")]
[DeclareFoldoutGroup("Test Scroll Group")]
[DeclareFoldoutGroup("Test Scale Group")]
[DeclareFoldoutGroup("Test Rotate Group")]
[DeclareFoldoutGroup("Test Move Group")]
[DeclareFoldoutGroup("Test Shadow Group")]
public class AnimationController : MonoBehaviour
{
    public static AnimationController _instance;
    public GameObject channelParent;
    public GameObject channelDecoy;
    public GameObject iconDecoy;
    public GameObject layerPrefab;
    public GameObject animatedImagePrefab;
    public GameObject holderTextPrefab;
    public TextMeshProUGUI timer;
    public Texture2D iconDefaultTexture;
    public Texture2D videoDefaultTexture;
    public Texture2D missingDefaultTexture;
    public bool paused = false;
    
    public ChannelAnimation currentChannelAnimation;

    public float globalTimer = 0f;
    public float channelTimer = 0f;
    public bool shouldTimeChannel = false;
    public bool shouldTimeGlobal = true;

    private void Awake()
    {
        _instance = this;
    }

    public List<Transform> GetOrAddParents(AnimatedImage image)
    {
        List<Transform> parents = new List<Transform>();
        if (image.channelAnimation.Equals(PreviewController._instance.currentAnimation))
        {
            // Here we spawn the image in the Editor decoy
            parents.Add(image.channelAnimation.type == CHANNELTYPE.ICON
                ? iconDecoy.transform
                : channelDecoy.transform);
        }
        else
        {
            // Here we spawn the image either in the channel banner, or in its respective icon holder
            if (image.channelAnimation.type == CHANNELTYPE.BANNER)
            {
                parents.Add(channelParent.transform);
            }
            else
            {
                // We need to find all the channels that have this image and link them
                foreach (Channel channel in image.channelAnimation.linkedChannels)
                {
                    if (channel.appHandler != null && channel.appHandler.iconImageHolder != null)
                    {
                        parents.Add(channel.appHandler.iconImageHolder);
                    }
                }
            }
        }


        string layerName = image.layer.ToString();
        List<Transform> transList = new List<Transform>();
        foreach (Transform parent in parents)
        {
            if (parent == null)
            {
                continue;
            }
            
            Transform trans = parent.Find(layerName);
            if (trans == null)
            {
                // If we haven't found a layer object with that index already, we create it
                // First, we iterate all the current layers, to find the sibling index
                int index = 0;
                for (int i = 0; i < parent.childCount; i++)
                {
                    Transform child = parent.GetChild(i);
                    if (int.Parse(child.name) > image.layer)
                    {
                        index = child.GetSiblingIndex();
                        break;
                    }

                    if (i == parent.childCount - 1)
                    {
                        index = child.GetSiblingIndex() + 1;
                        break;
                    }
                }

                // Then, we spawn the layer
                trans = Instantiate(layerPrefab, parent).transform;
                trans.gameObject.name = layerName;
                trans.SetSiblingIndex(index);
            }
            transList.Add(trans);
        }

        return transList;
    }

    public void ChangeCurrentAnimation(ChannelAnimation newChannelAnimation)
    {
        currentChannelAnimation?.Stop();
        if (AppController._instance.IsLowMemoryMode() && currentChannelAnimation != newChannelAnimation)
        {
            currentChannelAnimation?.UnloadIfUnused();
        }
        currentChannelAnimation = newChannelAnimation;
    }

    public void RestartChannelAnimation()
    {
        RestartChannelAnimation(true);
    }

    public void RestartChannelAnimation(bool stopCurrentAnim)
    {
        if (stopCurrentAnim)
        {
            currentChannelAnimation?.Stop();
        }
        UnfreezeChannelTimer();
        currentChannelAnimation?.PlayAudio();
    }

    public void FreezeChannelTimer()
    {
        shouldTimeChannel = false;
        ResetChannelTimer();
    }

    public void UnfreezeChannelTimer()
    {
        paused = false;
        shouldTimeChannel = true;
        ResetChannelTimer();
    }

    public void FreezeGlobalTimer()
    {
        shouldTimeGlobal = false;
        globalTimer = -1f;
        ChannelController._instance.RefreshIconAnimations();
    }
    
    public void RestartIconAnimations()
    {
        shouldTimeGlobal = true;
        globalTimer = 0f;
        ChannelController._instance.RefreshIconAnimations();
    }

    public void ResetChannelTimer()
    {
        channelTimer = 0f;
        timer.text = "0.00";
    }

    public void Pause()
    {
        paused = true;
        AudioController.PauseMainAudio();
        
        if (currentChannelAnimation != null)
        {
            currentChannelAnimation.Pause();
        }
    }

    public void Resume()
    {
        paused = false;
        AudioController.ResumeMainAudio();
        
        if (currentChannelAnimation != null)
        {
            currentChannelAnimation.Resume();
        }
    }

    private void Update()
    {
        if (paused)
        {
            return;
        }
        
        // Update timers
        if (shouldTimeGlobal)
        {
            globalTimer += Time.deltaTime;
        }

        if (shouldTimeChannel)
        {
            channelTimer += Time.deltaTime;
        }
        
        // Update text timer
        string currentTimelineTime = /*currentChannelAnimation == null ? "0.00" : (Math.Truncate(100f * currentChannelAnimation.GetTimelineTime()) / 100f).ToString();*/ "0.00";
        if (currentChannelAnimation != null)
        {
            float timelineTime = currentChannelAnimation.GetTimelineTime();
            int seconds = (int) Math.Floor(timelineTime);
            int milliseconds = (int) Math.Floor((timelineTime - seconds) * 1000);
            string milliString = milliseconds <= 0
                ? "00"
                : milliseconds < 10
                    ? milliseconds + "0"
                    : milliseconds.ToString().Substring(0, 2);
            currentTimelineTime = seconds + "." + milliString;
        }
        /*else
        {
            currentTimelineTime = "0.00";
        }*/
        timer.text = currentTimelineTime;
        
        // Update the main animation
        if (currentChannelAnimation != null)
        {
            currentChannelAnimation.Update();
        }

        // Update icon animations
        if (shouldTimeGlobal)
        {
            // first we mark them as ready to tick
            foreach (Channel channel in ChannelController._instance.loadedChannels)
            {
                if (channel.iconAnimation is not null)
                {
                    channel.iconAnimation.tickedThisFrame = false;
                }
            }
            
            // then we tick them
            foreach (Channel channel in ChannelController._instance.loadedChannels)
            {
                if (channel.iconAnimation is not null && !channel.iconAnimation.tickedThisFrame)
                {
                    channel.iconAnimation.tickedThisFrame = true;
                    channel.iconAnimation?.Update();
                }
            }
        }
    }

    [Title("Test Animations")]
    public bool shouldTime = true;
    [GroupNext("Test Banner Group")]
    public Texture2D testImage;
    public float testInitTime;
    public float testEndTime;
    public int testLayer;
    public Color testColor;
    public Vector3 testScale;
    public Vector3 testPosition;
    public Vector3 testRotation;
    
    [GroupNext("Test Scroll Group")]
    [Group("Test Scroll Group")]
    public bool shouldScroll;
    public float scrollInitTime;
    public float scrollEndTime;
    public float scrollSpeed;
    public float testOffsetX;
    public float testOffsetY;
    
    [GroupNext("Test Scale Group")]
    public bool shouldSize;
    public Vector3 targetSize;
    public float sizeStartTime;
    public float sizeEndTime;
    
    [GroupNext("Test Rotate Group")]
    public bool shouldRotate;
    public Vector3 targetRotation;
    public float rotateStartTime;
    public float rotateEndTime;
    
    [GroupNext("Test Move Group")]
    public bool shouldMove;
    public Vector3 targetMovement;
    public float moveStartTime;
    public float moveEndTime;
    
    [GroupNext("Test Shadow Group")]
    public bool shouldShadow;
    public Color shadowColor;
    public Vector2 shadowDirection;
    public float shadowStartTime;
    public float shadowFillDuration;
    public float shadowEndTime;
    [UnGroupNext]
    
    [Button("Summon Test Banner")]
    public void SummonTestBanner()
    {
        /*if (currentChannelAnimation == null)
        {
            currentChannelAnimation = new ChannelAnimation_BANNER("Test Banner", null);
        }

        AnimatedImage animatedImage = new AnimatedImage();
        animatedImage.image = testImage;
        animatedImage.startTime = testInitTime;
        animatedImage.endTime = testEndTime;
        animatedImage.layer = testLayer;
        animatedImage.initialColor = testColor;
        animatedImage.initialLocalPosition = testPosition;
        animatedImage.initialLocalScale = testScale;
        animatedImage.initialLocalRotation = testRotation;

        if (shouldShadow)
        {
            ShadowAnimation shadowAnimation = new ShadowAnimation();
            shadowAnimation.shadowColor = shadowColor;
            shadowAnimation.startTime = shadowStartTime;
            shadowAnimation.endTime = shadowEndTime;
            shadowAnimation.shadowFillTime = shadowFillDuration;
            shadowAnimation.shadowDirection = shadowDirection;
            animatedImage.AddAnimation(shadowAnimation);
        }
        
        if (shouldScroll)
        {
            ScrollAnimation scrollAnimation = new ScrollAnimation();
            scrollAnimation.speedPerSecond = scrollSpeed;
            scrollAnimation.offsetX = testOffsetX;
            scrollAnimation.offsetY = testOffsetY;
            scrollAnimation.startTime = scrollInitTime;
            scrollAnimation.endTime = scrollEndTime;
            animatedImage.AddAnimation(scrollAnimation);
        }

        if (shouldSize)
        {
            SizeAnimation sizeAnimation = new SizeAnimation();
            sizeAnimation.finalSize = targetSize;
            sizeAnimation.startTime = sizeStartTime;
            sizeAnimation.endTime = sizeEndTime;
            animatedImage.AddAnimation(sizeAnimation);
        }
        
        if (shouldMove)
        {
            MovementAnimation movementAnimation = new MovementAnimation();
            movementAnimation.finalPosition = targetMovement;
            movementAnimation.startTime = moveStartTime;
            movementAnimation.endTime = moveEndTime;
            animatedImage.AddAnimation(movementAnimation);
        }
        
        if (shouldRotate)
        {
            RotationAnimation rotateAnimation = new RotationAnimation();
            rotateAnimation.finalRotation = targetRotation;
            rotateAnimation.startTime = rotateStartTime;
            rotateAnimation.endTime = rotateEndTime;
            animatedImage.AddAnimation(rotateAnimation);
        }
        
        currentChannelAnimation.AddImage(animatedImage);
        //animatedImage.Summon();
        */
    }
}
