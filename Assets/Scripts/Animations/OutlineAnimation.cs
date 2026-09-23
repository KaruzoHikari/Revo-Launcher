using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class OutlineAnimation : TweenAnimation
{
    private List<NicerOutline> outlineComponents = new List<NicerOutline>();
    public Color outlineColor = new Color(0.25f, 0.25f, 0.25f, 0.75f);
    public Vector2 outlineDirection = new Vector2();
    public float outlineFillTime = 0f;
    
    protected override void _Start()
    {
        foreach (AnimationHolder holder in animInfo.holders)
        {
            NicerOutline outline = holder.gameObject.GetComponent<NicerOutline>();
            if (outline == null)
            {
                outline = holder.gameObject.AddComponent<NicerOutline>();
            }
            outline.effectColor = outlineColor;
            outline.effectDistance = new Vector2();
            animationTweens.Add(DOTween.To(()=> outline.effectDistance, x=> outline.effectDistance = x, outlineDirection, outlineFillTime));
            outlineComponents.Add(outline);
        }
    }

    protected override void _Stop()
    {
        base._Stop();
        foreach (NicerOutline outline in outlineComponents)
        {
            outline.effectColor = new Color(0, 0, 0, 0);
        }
        outlineComponents.Clear();
    }
    
    public override bool IsInTime()
    {
        float time = GetTimelineTime();
        return time > startTime && ((endTime >= 0 && time < endTime) || endTime < 0);
    }

    public override List<Option> GetOptions()
    {
        List<Option> optionList = base.GetOptions();
        
        optionList.Add(Option.Create("editor.title.outlinecolor", () => outlineColor, x => outlineColor = x));
        optionList.Add(Option.Create("editor.title.outlinedirection", () => outlineDirection, x => outlineDirection = x));
        optionList.Add(Option.Create("editor.title.outlinefilltime", "editor.description.outlinefilltime",
            () => outlineFillTime, x => outlineFillTime = x));

        return optionList;
    }
}
