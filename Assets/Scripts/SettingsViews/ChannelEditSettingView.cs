using Animations;
using Misc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
    public class ChannelEditSettingView : AbstractChannelsSettingView
    {
        public ChannelEditSettingView() : base(SettingsController.SETTINGSMENU.CHANNEL_SELECTION_EDITOR, true)
        {
        }

        public override void Show(CHANNELTYPE type)
        {
            base.Show(type);
            _instance.SetupChannelSelection_Editor(type, true, OnSelectedChannel);
        }

        private void OnSelectedChannel(ChannelAnimation animation)
        {
            if (animation.IsOnlineAnimation())
            {
                // We forbid directly editing online-animations
                // Instead, we recommend them to clone them
                PopupController.ShowPopup("popup.editonline");
            }
            else
            {
                EditorController._instance.OpenEditor(animation);
            }
        }

        public override void ClickedBack()
        {
            _instance.editorTypeSettingView.Show();
        }

        public override void ClickedCreate()
        {
            _instance.ClickedMoreAnims();
        }
    }
}