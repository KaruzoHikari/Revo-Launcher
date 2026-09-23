using Animations;
using Misc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
    public class ChannelCloneSettingView : AbstractChannelsSettingView
    {
        public ChannelCloneSettingView() : base(SettingsController.SETTINGSMENU.CHANNEL_SELECTION_CLONE)
        {
        }

        public override void Show(CHANNELTYPE type)
        {
            base.Show(type);
            _instance.SetupChannelSelection_Editor(type, true, OnSelectedChannel);
        }

        private void OnSelectedChannel(ChannelAnimation animation)
        {
            ChannelController._instance.CloneChannelAnimation(animation);
            PopupController.ShowPopup("popup.clonedanim", () =>
            {
                _instance.channelEditSettingView.Show(channelType);
            }, true, new [] { animation.name });
        }

        public override void ClickedBack()
        {
            _instance.editorTypeSettingView.Show();
        }
    }
}