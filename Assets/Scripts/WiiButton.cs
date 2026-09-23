using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;

public class WiiButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public bool shouldGetBigger = true;
    public bool shouldOutline = false;
    public bool shouldPlayClick = true;
    public bool shouldPlayBack = false;
    public bool shouldPlayHover = true;
    private bool isBigger = false;
    private bool isOutline = false;
    private Tween scaleTween;
    private Tween outlineTween;
    public float multiplier = 1.175f;
    public Vector2 outlineSource = new Vector2(5, 5);
    public Vector2 outlineTarget = new Vector2(12, 12);
    public float outlineFillTime = 0.2f;
    private NicerOutline outline;

    private void Awake()
    {
        TryGetComponent(out outline);
    }

    private Vector3 CompleteAndSetScale()
    {
        var localScale = transform.localScale;
        Vector3 currentScale = new Vector3(localScale.x, localScale.y, localScale.z);
        scaleTween?.Complete();
        Vector3 originalScale = transform.localScale;
        transform.localScale = currentScale;
        return originalScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (shouldPlayHover)
        {
            AudioController.PlaySoundEffect(AudioLibrary._instance.HOVER_BUTTON);
        }
        if (shouldGetBigger && !isBigger)
        {
            isBigger = true;
            Vector3 localScale = CompleteAndSetScale();
            if (transform != null)
            {
                scaleTween = DOTween.Sequence()
                    .Append(transform.DOScale(new Vector3(localScale.x * multiplier, localScale.y * multiplier,
                        localScale.z * multiplier), 0.25f))
                    .AppendCallback(() => scaleTween = null);
            }
        }
        if (shouldOutline && !isOutline)
        {
            isOutline = true;
            outlineTween?.Complete();
            if (outline != null)
            {
                outlineTween = DOTween.Sequence()
                    .Append(DOTween.To(() => outline.effectDistance, x => outline.effectDistance = x, outlineTarget, outlineFillTime))
                    .AppendCallback(() => outlineTween = null);
            }
        }
        WiimoteController.RumbleMain(WiimoteController._instance.buttonRumbleTime);
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        GetSmaller();
    }

    private void GetSmaller()
    {
        if (shouldGetBigger && isBigger)
        {
            isBigger = false;
            Vector3 localScale = CompleteAndSetScale();
            if (transform != null)
            {
                scaleTween = DOTween.Sequence()
                    .Append(transform.DOScale(new Vector3(localScale.x / multiplier, localScale.y / multiplier,
                        localScale.z / multiplier), 0.25f))
                    .AppendCallback(() => scaleTween = null);
            }
        }
        if (shouldOutline && isOutline)
        {
            isOutline = false;
            outlineTween?.Complete();
            if (outline != null)
            {
                outlineTween = DOTween.Sequence()
                    .Append(DOTween.To(() => outline.effectDistance, x => outline.effectDistance = x, outlineSource, outlineFillTime))
                    .AppendCallback(() => outlineTween = null);
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (shouldPlayClick)
        {
            if (shouldPlayBack)
            {
                AudioController.PlaySoundEffect(AudioLibrary._instance.CLICK_BUTTON_BACK);
            }
            else
            {
                AudioController.PlaySoundEffect(AudioLibrary._instance.CLICK_BUTTON);
            }
        }
        GetSmaller();
    }
}
