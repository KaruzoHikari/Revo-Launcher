using Animations;
using Misc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
    public class ChannelDeleteSettingView : AbstractChannelsSettingView
    {
        public ChannelDeleteSettingView() : base(SettingsController.SETTINGSMENU.CHANNEL_SELECTION_DELETE)
        {
        }

        public override void Show(CHANNELTYPE type)
        {
            base.Show(type);
            _instance.SetupChannelSelection_Editor(type, true, OnSelectedChannel);
        }

        private void OnSelectedChannel(ChannelAnimation animation)
        {
            PopupController.ShowPopup("popup.confirmdeleteanim", () =>
            {
                // We delete it
                ChannelController._instance.DeleteChannelAnimation(animation);
        
                PopupController.ShowPopup("popup.deletedanim", () =>
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