using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ColorAnimation : TweenAnimation
{
    public Color finalColor = Color.white;
    
    protected override void _Start()
    {
        foreach (AnimationHolder holder in animInfo.holders)
        {
            animationTweens.Add(holder.rawImage.DOColor(finalColor, GetDuration()).SetEase(ease));
        }
    }
    
    public override List<Option> GetOptions()
    {
        List<Option> optionList = base.GetOptions();
        
        optionList.Add(Option.Create("editor.title.finalcolor", () => finalColor, x => finalColor = x));

        return optionList;
    }
}
