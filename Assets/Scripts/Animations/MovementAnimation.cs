using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using DG.Tweening;
using UnityEngine;

public class MovementAnimation : TweenAnimation
{

    public Vector3 finalPosition = new Vector3();
    public bool isRelative = false;
    
    protected override void _Start()
    {
        foreach (AnimationHolder holder in animInfo.holders)
        {
            Vector3 fixedPosition = finalPosition;
            if (isRelative)
            {
                fixedPosition += holder.transform.localPosition;
            } 
            
            animationTweens.Add(holder.transform.DOLocalMove(fixedPosition, GetDuration()).SetEase(ease));
        }
    }
    
    public override List<Option> GetOptions()
    {
        List<Option> optionList = base.GetOptions();
        
        optionList.Add(Option.Create("editor.title.finalpos", () => (Vector2) finalPosition, x => finalPosition = x));
        optionList.Add(Option.Create("editor.title.relativecoords", "editor.description.relativecoords", () => isRelative, x => isRelative = x));

        return optionList;
    }
}
