using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class ScrollAnimation : Animation
{
    public float offsetX;
    public float offsetY;
    public float speedPerSecond;
    [JsonIgnore] private bool paused = false;

    protected override void _Update()
    {
        if (paused)
        {
            return;
        }

        float deltaTime = Time.deltaTime;
        float deltaX = deltaTime * speedPerSecond * offsetX;
        float deltaY = deltaTime * speedPerSecond * offsetY;

        foreach (AnimationHolder holder in animInfo.holders)
        {
            Rect currentRect = holder.rawImage.uvRect;
            currentRect.x += deltaX;
            currentRect.y += deltaY;
            holder.rawImage.uvRect = currentRect;
        }
    }

    public override void Pause()
    {
        paused = true;
    }

    public override void Resume()
    {
        paused = false;
    }

    public override List<Option> GetOptions()
    {
        List<Option> optionList = base.GetOptions();
        
        optionList.Add(Option.Create("editor.title.scrollx", () => offsetX, x => offsetX = x));
        optionList.Add(Option.Create("editor.title.scrolly", () => offsetY, x => offsetY = x));
        optionList.Add(Option.Create("editor.title.scrollspeed", () => speedPerSecond, x => speedPerSecond = x));

        return optionList;
    }
}
