using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Newtonsoft.Json;
using UnityEngine;

public class TweenAnimation : Animation
{
    [JsonIgnore] protected List<Tween> animationTweens = new List<Tween>();
    public Ease ease = DOTween.defaultEaseType;
    protected override void _Stop()
    {
        foreach(Tween animationTween in animationTweens) {
            animationTween.Complete();
        }
    }

    public override void Pause()
    {
        foreach(Tween animationTween in animationTweens) {
            animationTween.Pause();
        }
    }

    public override void Resume()
    {
        foreach(Tween animationTween in animationTweens) {
            animationTween.Play();
        }
    }

    public override List<Option> GetOptions()
    {
        List<Option> optionList = base.GetOptions();
        
        optionList.Add(Option.Create("editor.title.ease", () => ease, x => ease = x));

        return optionList;
    }
}
