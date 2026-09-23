using System;
using System.Collections;
using System.Collections.Generic;
using Animations;
using DG.Tweening;
using TMPro;
using TriInspector;
using UnityEngine;
using UnityEngine.UI;

public class PreviewController : MonoBehaviour
{
    private GameObject currentDecoy;
    public GameObject mainDecoy;
    public GameObject decoyBackground;
    public RectTransform decoyContainer;
    public GameObject decoyGlobalOverlay;
    public GameObject decoyWhiteBottom;
    public GameObject decoyOverlay;
    public GameObject decoyOverlayThumbnail;
    public GameObject decoyOverlayThumbnailPreview;
    public GameObject decoyBanner;
    public GameObject decoyIcon;

    public AnimatedImage currentDebugImage;
    public TextMeshProUGUI maxDebugText;
    
    [Title("Thumbnail Corners")]
    public GameObject iconBottomCorner;
    public GameObject iconTopCorner;
    public GameObject bannerBottomCorner;
    public GameObject bannerTopCorner;

    public ChannelAnimation currentAnimation;
    public Texture2D currentThumbnail;
    private Tween currentTween;
    public static PreviewController _instance;
    public bool isOpen;

    private void Awake()
    {
        _instance = this;

        // we're gonna cheat badly for a sec, until we implement proper wide support
        bool ogBool = CameraController._instance.shouldDebugOrientation;
        ScreenOrientation ogOrientation = CameraController._instance.debugOrientation;
        CameraController._instance.shouldDebugOrientation = true;
        CameraController._instance.debugOrientation = ScreenOrientation.Portrait;
        float height = CameraController.GetFixedHeight();
        CameraController._instance.shouldDebugOrientation = ogBool;
        CameraController._instance.debugOrientation = ogOrientation;
        
        // now that we have a 100% portrait height, we can continue
        Vector3 size = new Vector3(1080f, height, 0);
        RectTransform rectTransform = mainDecoy.GetComponent<RectTransform>();
        rectTransform.localPosition = new Vector3(0, height, 0);
        rectTransform.sizeDelta = size;
        float sizeMultiplier = CameraController.GetChannelSize(1f);
        decoyContainer.localScale = new Vector3(sizeMultiplier, sizeMultiplier, 0f);
    }

    public void SetupDebugText(AnimatedImage image)
    {
        currentDebugImage = image;
        maxDebugText.gameObject.SetActive(true);
        maxDebugText.text = "MAX: (?)";
    }

    public void ShowDecoy(ChannelAnimation animation, bool showEditorOverlay = false, bool showThumbnailButtons = false, bool manuallyPlayAnimation = false)
    {
        isOpen = true;
        currentAnimation = animation;
        currentDecoy?.SetActive(false);
        currentDecoy = animation.type == CHANNELTYPE.ICON ? decoyIcon : decoyBanner;
        AnimationController._instance.FreezeChannelTimer();
        AudioController.MuteBackgroundAudio(true);
        mainDecoy.SetActive(true);
        currentDecoy.SetActive(true);
        decoyOverlay.SetActive(showEditorOverlay || showThumbnailButtons);
        decoyOverlayThumbnail.SetActive(showThumbnailButtons);

        currentTween?.Kill();
        StartCoroutine(_ShowDecoy(animation, manuallyPlayAnimation));
    }

    public IEnumerator _ShowDecoy(ChannelAnimation animation, bool manuallyPlayAnimation = false)
    {
        // we wait until the animation is ready!
        yield return new WaitUntil(() => animation is null || animation.FinishedLoading());
        
        if (manuallyPlayAnimation)
        {
            currentTween = mainDecoy.transform.DOLocalMoveY(0f, 0.7f);
        }
        else
        {
            currentTween = DOTween.Sequence()
                .AppendCallback(() => AnimationController._instance.ChangeCurrentAnimation(animation))
                .Append(mainDecoy.transform.DOLocalMoveY(0f, 0.7f))
                .AppendCallback(() =>
                {
                    AnimationController._instance.RestartChannelAnimation(false);
                });
        }
    }

    public void HideDecoy(bool isEditor)
    {
        isOpen = false;
        currentDebugImage = null;
        maxDebugText.gameObject.SetActive(false);
        currentTween?.Kill();
        
        currentTween = DOTween.Sequence()
            .Append(mainDecoy.transform.DOLocalMoveY(CameraController.GetFixedHeight(), 0.7f))
            .AppendCallback(() =>
            {
                currentDecoy.SetActive(false);
                mainDecoy.SetActive(false);
                currentAnimation = null;
                HideThumbnail();
                if (!isEditor)
                {
                    AnimationController._instance.ChangeCurrentAnimation(null);
                }
            });
        AudioController.StopMainAudio();
        AudioController.UnmuteBackgroundAudio();
    }

    public void ShowThumbnail(Texture2D thumbnail)
    {
        currentThumbnail = thumbnail;
        decoyOverlayThumbnailPreview.SetActive(true);
        RawImage rawImage = decoyOverlayThumbnailPreview.transform.Find("Image").GetComponent<RawImage>();
        rawImage.texture = thumbnail;
        rawImage.SetNativeSize();
    }

    public void HideThumbnail()
    {
        currentThumbnail = null;
        decoyOverlayThumbnailPreview.SetActive(false);
        decoyOverlayThumbnailPreview.transform.Find("Image").GetComponent<RawImage>().texture = null;
    }

    public void ClearPreview()
    {
        foreach (Transform child in (currentAnimation.type == CHANNELTYPE.ICON ?
            AnimationController._instance.iconDecoy.transform : AnimationController._instance.channelDecoy.transform)) {
            Destroy(child.gameObject);
        }
    }

    public GameObject GetCurrentDecoy()
    {
        return currentDecoy;
    }

    public GameObject GetBottomCorner(CHANNELTYPE channeltype)
    {
        return channeltype == CHANNELTYPE.BANNER ? bannerBottomCorner : iconBottomCorner;
    }
    
    public GameObject GetTopCorner(CHANNELTYPE channeltype)
    {
        return channeltype == CHANNELTYPE.BANNER ? bannerTopCorner : iconTopCorner;
    }
}
