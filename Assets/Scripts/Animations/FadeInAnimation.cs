using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class FadeInAnimation : TweenAnimation
{
    protected override void _Start()
    {
        float endValue = animInfo.initialColor.a;
        if (endValue == 0f)
        {
            // It doesn't make sense to fade to nothing, so we fade in to the max
            endValue = 1f;
        }
        
        foreach (AnimationHolder holder in animInfo.holders)
        {
            animationTweens.Add(DOTween.ToAlpha(() => holder.rawImage.color, x => holder.rawImage.color = x, endValue, GetDuration()));
        }
    }
}
