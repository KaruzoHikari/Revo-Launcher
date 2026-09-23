using Animations;
using Misc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
    public class ChannelConvertSettingView : AbstractChannelsSettingView
    {
        public ChannelConvertSettingView() : base(SettingsController.SETTINGSMENU.CHANNEL_SELECTION_CONVERT)
        {
        }

        public override void Show(CHANNELTYPE type)
        {
            base.Show(type);
            _instance.SetupChannelSelection_Editor(type, true, OnSelectedChannel);
        }

        private void OnSelectedChannel(ChannelAnimation animation)
        {
            PopupController.ShowPopup("popup.confirmconvertanim", () =>
            {
                // We convert it
                ChannelController._instance.ConvertChannelAnimation(animation);
        
                PopupController.ShowPopup("popup.convertedanim", () =>
                {
                    _instance.channelEditSettingView.Show(channelType);
                }, true, new [] { animation.name });
            }, PopupController.ClosePopup, new [] { animation.name });
        }

        public override void ClickedBack()
        {
            _instance.editorTypeSettingView.Show();
        }
    }
}