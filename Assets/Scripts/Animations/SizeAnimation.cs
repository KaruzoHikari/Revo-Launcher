using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class SizeAnimation : TweenAnimation
{

    public Vector3 finalSize = new Vector3(1, 1, 1);
    
    protected override void _Start()
    {
        foreach (AnimationHolder holder in animInfo.holders)
        {
            animationTweens.Add(holder.transform.DOScale(finalSize, GetDuration()).SetEase(ease));
        }
    }
    
    public override List<Option> GetOptions()
    {
        List<Option> optionList = base.GetOptions();
        
        optionList.Add(Option.Create("editor.title.finalsize", () => (Vector2) finalSize, x => finalSize = x));

        return optionList;
    }
}
