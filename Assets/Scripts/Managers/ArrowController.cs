using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ArrowController : MonoBehaviour
{
    public bool shouldIncrease;
    private bool isHiding = true;

    public RectTransform rectTransform;
    public RectTransform channelHitboxTransform;
    public RectTransform innerHolder;
    public GameObject arrowImage;
    public GameObject signImage;
    public Image signImageFader;
    public Image signImageAccentFader;
    private Vector3 originalScale;

    private Tween animationLoop;

    public void Awake()
    {
        StartLoopAnimation();
        gameObject.SetActive(false);
        originalScale = arrowImage.transform.localScale;
    }

    public void ClickedArrow()
    {
        if (GridController._instance.isChangingGrid || AppController.IsTransitioning() || !gameObject.activeInHierarchy || isHiding)
        {
            return;
        }

        if (ChannelController._instance.currentChannel == null)
        {
            // Here, we're clicking arrows from the Main
            int newGrid = GridController._instance.currentGrid.number;
            newGrid = shouldIncrease ? newGrid + 1 : newGrid - 1;
            GridController._instance.ChangeGrid(newGrid);
            AudioController.PlaySoundEffect(AudioLibrary._instance.CLICK_PLUSLESS);
        
            RunHighlight(signImageFader);
            RunHighlight(signImageAccentFader);
        }
        else
        {
            // Here we click arrows from the channel view
            StartCoroutine(ChangeChannel());
        }
    }

    private void RunHighlight(Image image)
    {
        DOTween.Sequence()
            .Append(DOTween.ToAlpha(() => image.color, x => image.color = x, 0.9f, 0.067f))
            .AppendInterval(0.1f)
            .Append(DOTween.ToAlpha(() => image.color, x => image.color = x, 0f, 0.167f));
    }

    private IEnumerator ChangeChannel()
    {
        AudioController.PlaySoundEffect(AudioLibrary._instance.CLICK_PLUSLESS);
        yield return new WaitForSeconds(AudioLibrary._instance.CLICK_PLUSLESS.length/3);
        ChannelController._instance.ChangeChannel(shouldIncrease);
    }

    public void MouseEnter()
    {
        DOTween.Sequence()
            .Append(arrowImage.transform.DOScaleY(0.8f, 0.066f))
            .Append(arrowImage.transform.DOScaleY(1f, 0.05f))
            .Join(signImage.transform.DOScale(new Vector3(1f, 1f, 1f), 0.1f))
            .Append(signImage.transform.DOScaleX(1.15f, 0.05f))
            .Append(signImage.transform.DOScaleX(1f, 0.05f));
    }


    public void MouseLeave()
    {
        DOTween.Sequence()
            .Append(signImage.transform.DOScaleX(1.15f, 0.05f))
            .Append(arrowImage.transform.DOScaleY(0.8f, 0.05f))
            .Join(signImage.transform.DOScale(new Vector3(0f, 0f, 0f), 0.1f))
            .Append(arrowImage.transform.DOScaleY(1f, 0.05f));
    }

    private void ResetState()
    {
        animationLoop?.SetLoops(0);
        animationLoop?.Complete();
        animationLoop = null;
    }

    private void ResetSign()
    {
        signImage.transform.localScale = new Vector3(0, 0, 0);
        arrowImage.transform.localScale = originalScale;
    }

    public void Hide()
    {
        ResetState();
        isHiding = true;
        animationLoop = DOTween.Sequence()
            .Append(rectTransform.DOAnchorPosX(shouldIncrease ? 150 : -150, 0.25f))
            .AppendCallback(() =>
            {
                ResetSign();
                gameObject.SetActive(false);
            });
    }

    public void Show()
    {
        ResetState();
        isHiding = false;
        gameObject.SetActive(true);
        rectTransform.DOAnchorPosX(shouldIncrease ? 8 : -8, 0.25f);
    }

    private void StartLoopAnimation()
    {
        float value = shouldIncrease ? 12 : -12;
        DOTween.Sequence()
            .Append(innerHolder.DOAnchorPosX(value, 0.7f))
            .SetLoops(-1, LoopType.Yoyo);
    }
}
