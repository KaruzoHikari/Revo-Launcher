using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RotationAnimation : TweenAnimation
{
    public Vector3 finalRotation = new Vector3();
    public RotationMode rotationMode = RotationMode.ALWAYS_RIGHT;
    
    protected override void _Start()
    {
        foreach (AnimationHolder holder in animInfo.holders)
        {
            animationTweens.Add(holder.transform.DOLocalRotate(finalRotation, GetDuration(), GetRelatedTweenMode()).SetEase(ease));
        }
    }
    
    public override List<Option> GetOptions()
    {
        List<Option> optionList = base.GetOptions();
        
        optionList.Add(Option.Create("editor.title.finalrot", () => finalRotation, x => finalRotation = x));
        optionList.Add(Option.Create("editor.title.rotmode", () => rotationMode, x => rotationMode = x));

        return optionList;
    }

    protected RotateMode GetRelatedTweenMode()
    {
        switch (rotationMode)
        {
            case RotationMode.SHORTEST: return RotateMode.Fast;
            case RotationMode.RELATIVE: return RotateMode.LocalAxisAdd;
            default: return RotateMode.FastBeyond360;
        }
    }

    public enum RotationMode
    {
        ALWAYS_RIGHT, SHORTEST, RELATIVE
    }
}
