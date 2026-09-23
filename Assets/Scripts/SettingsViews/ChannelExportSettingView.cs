using Animations;
using Misc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
    public class ChannelExportSettingView : AbstractChannelsSettingView
    {
        public ChannelExportSettingView() : base(SettingsController.SETTINGSMENU.CHANNEL_SELECTION_EXPORT)
        {
        }

        public override void Show(CHANNELTYPE type)
        {
            base.Show(type);
            _instance.SetupChannelSelection_Editor(type, true, OnSelectedChannel);
        }

        private void OnSelectedChannel(ChannelAnimation animation)
        {
            _instance.RequestExportAnim(animation);
        }

        public override void ClickedBack()
        {
            _instance.editorTypeSettingView.Show();
        }
    }
}