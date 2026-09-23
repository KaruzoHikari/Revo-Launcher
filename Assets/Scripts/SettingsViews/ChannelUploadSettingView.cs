using Animations;
using Misc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
    public class ChannelUploadSettingView : AbstractChannelsSettingView
    {
        public ChannelUploadSettingView() : base(SettingsController.SETTINGSMENU.CHANNEL_SELECTION_UPLOAD)
        {
        }

        public override void Show()
        {
            base.Show();
            _instance.SetupChannelSelection_Editor(channelType, false, ShopController._instance.OnSelectedAnimation);
        }

        public override void ClickedBack()
        {
            ShopController._instance.OpenShop();
        }
    }
}