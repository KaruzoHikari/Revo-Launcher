using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FadeController : MonoBehaviour
{

    public Image cover;
    public static FadeController _instance;
    private Tween currentTween;
    private float speedMultiplier = 1f;

    private void Awake()
    {
        _instance = this;
        RefreshSpeed();
    }

    public void RefreshSpeed()
    {
        float value = PREFS.FadeoutSpeed.GetFloat();
        speedMultiplier = value != 0 ? 1f/value : 1f;
    }

    public static float GetSpeedMultiplier()
    {
        return _instance.speedMultiplier;
    }

    public void FadeInAndOut(float inDuration, float waitTime, float outDuration)
    {
        StartCoroutine(_FadeInAndOut(inDuration, waitTime, outDuration));
    }

    private IEnumerator _FadeInAndOut(float inDuration, float waitTime, float outDuration)
    {
        FadeIn(inDuration);
        yield return new WaitForSeconds((inDuration + waitTime) * speedMultiplier);
        FadeOut(outDuration);
    }

    private void CancelCurrentTween()
    {
        if (currentTween != null)
        {
            currentTween.Kill();
            currentTween = null;
        }
    }

    public void FadeIn(float duration)
    {
        CancelCurrentTween();
        cover.raycastTarget = true;
        currentTween = DOTween.ToAlpha(() => cover.color, x => cover.color = x, 1, duration * speedMultiplier);
    }
    
    public void FadeOut(float duration)
    {
        CancelCurrentTween();
        currentTween = DOTween.Sequence()
            .Append(DOTween.ToAlpha(() => cover.color, x => cover.color = x, 0, duration * speedMultiplier))
            .AppendCallback(() => cover.raycastTarget = false);
    }

    public bool IsFading()
    {
        return currentTween != null && currentTween.active;
    }
}
