using System.Collections.Generic;
using System.Threading.Tasks;
using Data.ChannelTargets;
using UnityEngine;

public class AnimatedImageText : AnimatedImage
{
    protected override void Summon()
    {
        base.Summon();
        
        foreach (AnimationHolder holder in holders)
        {
            string name = holder.animationInfo.imageName;
            if (!string.IsNullOrEmpty(name) && name.Equals("GameTitle"))
            {
                MetadataController._instance.RestartBoxRotation();
                LoadText(holder);
            }
        }
    }

    private void LoadText(AnimationHolder holder)
    {
        if (holder != null && holder.currentChannel != null && holder.currentChannel.GetTarget() is HasGameMetadata target)
        {
            // We load the game's text
            holder.rawImage.color = Color.clear;
            holder.rawImage.texture = MetadataController._instance.emptyTexture;
            holder.AddText();
            holder.text.color = initialColor;

            string tag = holder.currentChannel.GetTag();
            if (target.GetGameMetadata() != null && target.GetGameMetadata().console != null)
            {
                tag += "<br><line-height=28%><br><size=90%><line-height=100%><i>[" + target.GetGameMetadata().console.name + "]</i>";
            }

            holder.text.text = tag;
        }
    }
    
    public override void Update()
    {
        base.Update();
        
        foreach (AnimationHolder holder in holders)
        {
            string name = holder.animationInfo.imageName;
            if (!string.IsNullOrEmpty(name) && name.Equals("GameTitle") && holder.text != null)
            {
                holder.text.color = holder.rawImage.color;
            }
        }
    }
}