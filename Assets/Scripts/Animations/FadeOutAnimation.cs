using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class FadeOutAnimation : TweenAnimation
{
    protected override void _Start()
    {
        foreach (AnimationHolder holder in animInfo.holders)
        {
            animationTweens.Add(DOTween.ToAlpha(() => holder.rawImage.color, x => holder.rawImage.color = x, 0f, GetDuration()));
        }
    }
}
