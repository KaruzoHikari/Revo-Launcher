using System.Collections;
using Misc;
using UnityEngine;

namespace Data.ChannelTargets
{
    public class ChannelShopTarget : ChannelTarget
    {
        public ChannelShopTarget() : base(CHANNELTARGETS.SHOP)
        {
        }


        public override void Execute()
        {
            FadeController._instance.FadeOut(1f);
            AudioController.MuteBackgroundAudio(true);
            ShopController._instance.OpenShop(true);
        }

        public override string GetTag()
        {
            return "Content Hub";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}