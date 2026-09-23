using System;
using System.Collections.Generic;

public class AnimationLister
{
    public string name;
    public string description;
    public Type classType;
    public bool isHidden = false;

    private static List<AnimationLister> animations = new List<AnimationLister>();
    
    static AnimationLister()
    {
        animations.Add(new AnimationLister()
        {
            name = "editor.anim.movement.title",
            description = "editor.anim.movement.description",
            classType = typeof(MovementAnimation)
        });
        animations.Add(new AnimationLister()
        {
            name = "editor.anim.rotation.title",
            description = "editor.anim.rotation.description",
            classType = typeof(RotationAnimation)
        });
        animations.Add(new AnimationLister()
        {
            name = "editor.anim.scale.title",
            description = "editor.anim.scale.description",
            classType = typeof(SizeAnimation)
        });
        animations.Add(new AnimationLister()
        {
            name = "editor.anim.path.title",
            description = "editor.anim.path.description",
            classType = typeof(PathAnimation)
        });
        animations.Add(new AnimationLister()
        {
            name = "editor.anim.color.title",
            description = "editor.anim.color.description",
            classType = typeof(ColorAnimation)
        });
        animations.Add(new AnimationLister()
        {
            name = "editor.anim.scroll.title",
            description = "editor.anim.scroll.description",
            classType = typeof(ScrollAnimation)
        });
        animations.Add(new AnimationLister()
        {
            name = "editor.anim.shadow.title",
            description = "editor.anim.shadow.description",
            classType = typeof(ShadowAnimation)
        });
        animations.Add(new AnimationLister()
        {
            name = "editor.anim.fadein.title",
            description = "editor.anim.fadein.description",
            classType = typeof(FadeInAnimation)
        });
        animations.Add(new AnimationLister()
        {
            name = "editor.anim.fadeout.title",
            description = "editor.anim.fadeout.description",
            classType = typeof(FadeOutAnimation)
        });
        animations.Add(new AnimationLister()
        {
            name = "editor.anim.glow.title",
            description = "editor.anim.glow.description",
            classType = typeof(OutlineAnimation)
        });
        animations.Add(new AnimationLister()
        {
            name = "BoxArt",
            description = "Debug box art rotation",
            classType = typeof(BoxArtRotationAnimation),
            isHidden = true
        });
    }

    public static List<AnimationLister> GetAllAnimations()
    {
        return animations;
    }

    public static AnimationLister GetInfo(Animation animation)
    {
        Type animationType = animation.GetType();
        foreach (AnimationLister lister in animations)
        {
            if (lister.classType == animationType)
            {
                return lister;
            }
        }

        return null;
    }
}