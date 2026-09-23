using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using DG.Tweening;
using UnityEngine;

public class PathAnimation : TweenAnimation
{
    public List<Vector2> positions = new List<Vector2>();
    public PathType pathType = PathType.Linear;
    public bool isRelative = false;
    
    protected override void _Start()
    {
        bool isValid = (pathType != PathType.CubicBezier) || (positions.Count > 0 && positions.Count % 3 == 0);
        if (isValid)
        {
            foreach (AnimationHolder holder in animInfo.holders)
            {
                Vector2[] finalPositions = positions.ToArray();
                if (isRelative)
                {
                    for (int i = 0; i < finalPositions.Length; i++)
                    {
                        Vector2 offset = holder.transform.localPosition;
                        for (int j = 0; j < i; j++)
                        {
                            offset += positions[j];
                        }
                        offset += positions[i];
                        finalPositions[i] = offset;
                    }
                }

                animationTweens.Add(holder.transform.DOLocalPath(finalPositions.ToVector3Array(), GetDuration(), pathType).SetEase(ease));
            }
        }
    }
    
    public override List<Option> GetOptions()
    {
        List<Option> optionList = base.GetOptions();
        
        optionList.Add(Option.Create("editor.title.pathtype", () => pathType, x => pathType = x));
        optionList.Add(Option.Create("editor.title.relativecoords", "editor.description.relativecoords", () => isRelative, x => isRelative = x));
        optionList.Add(Option.Create("editor.title.waypoints", () => positions, x => positions = x));

        return optionList;
    }
}
