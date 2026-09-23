using System.Collections.Generic;
using System.Threading.Tasks;
using Data.ChannelTargets;
using UnityEngine;

public class AnimatedImageGame : AnimatedImage
{
    protected override void Summon()
    {
        base.Summon();
        
        foreach (AnimationHolder holder in holders)
        {
            string name = holder.animationInfo.imageName;
            if (!string.IsNullOrEmpty(name) && name.Equals("FrontCover"))
            {
                AdjustSize(holder, null);
                
                // Here we replace the texture with the static cover image!
                LoadTexture(holder);
            }
        }
    }
    
    private async Task LoadTexture(AnimationHolder holder)
    {
        if (holder != null && holder.currentChannel != null && holder.currentChannel.GetTarget() is HasGameMetadata target)
        {
            target.GetGameMetadata().GetFrontCover(tex =>
            {
                AdjustSize(holder, tex);
            });
        }
    }

    private void AdjustSize(AnimationHolder holder, Texture2D tex)
    {
        if (holder != null)
        {
            Texture2D finalTex = tex == null ? MetadataController._instance.unknownFront : tex;
            holder.rawImage.texture = finalTex;

            float width = 700f;
            float height = finalTex.height * width / finalTex.width;
                    
            // now we check whether we went over the max height, in which case we reduce it a little
            float maxHeight = 550f;
            if (height > maxHeight)
            {
                height = maxHeight;
                width = finalTex.width * height / finalTex.height;
            }
                    
            holder.rawImage.rectTransform.sizeDelta = new Vector2(width, height);
        }
    }
}