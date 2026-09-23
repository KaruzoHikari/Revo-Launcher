using Data.ChannelTargets;
using DG.Tweening;

public class BoxArtRotationAnimation : RotationAnimation
{
    protected override void _Start()
    {
        bool shouldStart = true;
        foreach (AnimationHolder holder in animInfo.holders)
        {
            if (holder.currentChannel != null && holder.currentChannel.target is HasGameMetadata target && target.GetGameMetadata() != null)
            {
                shouldStart = shouldStart && target.GetGameMetadata().ShouldRotateBox();
            }
        }

        if (shouldStart)
        {
            // pretty much the same as the regular rotation anim, except we rotate the box instead
            animationTweens.Add(MetadataController._instance.boxContainer.transform.DOLocalRotate(finalRotation, GetDuration(), GetRelatedTweenMode()).SetEase(ease));
        }
    }
}
