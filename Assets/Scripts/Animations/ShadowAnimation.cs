using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class ShadowAnimation : TweenAnimation
{
    private List<Shadow> shadowComponents = new List<Shadow>();
    public Color shadowColor = new Color(0.25f, 0.25f, 0.25f, 0.75f);
    public Vector2 shadowDirection = new Vector2();
    public float shadowFillTime = 0f;
    
    protected override void _Start()
    {
        foreach (AnimationHolder holder in animInfo.holders)
        {
            Shadow shadow = holder.gameObject.GetComponent<Shadow>();
            if (shadow == null)
            {
                shadow = holder.gameObject.AddComponent<Shadow>();
            }
            shadow.effectColor = shadowColor;
            shadow.effectDistance = new Vector2();
            animationTweens.Add(DOTween.To(()=> shadow.effectDistance, x=> shadow.effectDistance = x, shadowDirection, shadowFillTime));
            shadowComponents.Add(shadow);
        }
    }

    protected override void _Stop()
    {
        base._Stop();
        foreach (Shadow shadow in shadowComponents)
        {
            shadow.effectColor = new Color(0, 0, 0, 0);
        }
        shadowComponents.Clear();
    }
    
    public override bool IsInTime()
    {
        float time = GetTimelineTime();
        return time > startTime && ((endTime >= 0 && time < endTime) || endTime < 0);
    }

    public override List<Option> GetOptions()
    {
        List<Option> optionList = base.GetOptions();
        
        optionList.Add(Option.Create("editor.title.shadowcolor", () => shadowColor, x => shadowColor = x));
        optionList.Add(Option.Create("editor.title.shadowdirection", () => shadowDirection, x => shadowDirection = x));
        optionList.Add(Option.Create("editor.title.shadowfilltime", "editor.description.shadowfilltime",
            () => shadowFillTime, x => shadowFillTime = x));

        return optionList;
    }
}
