using System.Collections.Generic;
using System.Threading.Tasks;
using Data.ChannelTargets;
using UnityEngine;

public class AnimatedImageCover : AnimatedImage
{
    protected override void Summon()
    {
        base.Summon();
        
        foreach (AnimationHolder holder in holders)
        {
            string name = holder.animationInfo.imageName;
            if (!string.IsNullOrEmpty(name) && name.Equals("ArtCover"))
            {
                holder.rawImage.texture = MetadataController._instance.cameraTexture;
                holder.rawImage.SetNativeSize();
                MetadataController._instance.RestartBoxRotation();
                
                LoadTexture(holder);
            }
        }
    }
    
    private void LoadTexture(AnimationHolder holder)
    {
        if (holder != null && holder.currentChannel != null && holder.currentChannel.GetTarget() is HasGameMetadata target)
        {
            // We load the text onto the holder
            MetadataController._instance.LoadFullCover(target.GetGameMetadata());
            
            // Here we replace the texture with the camera from the rotating model
            /*holder.rawImage.texture = MetadataController._instance.cameraTexture;
            holder.rawImage.SetNativeSize();*/
        }
    }
}