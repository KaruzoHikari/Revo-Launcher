using Misc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
    public class EditorTypeSettingView : SettingsView
    {
        public EditorTypeSettingView() : base(SettingsController.SETTINGSMENU.EDITOR_CHANNELTYPE,
            "settings.title.animtype", ThemeColor.SettingsBackgroundDark)
        {
        }

        public override void Show()
        {
            base.Show();
            EnableMainGameObject(_instance.channelTypeSelection);
        }

        public override void ClickedBack()
        {
            _instance.generalSettingView.Show();
        }
    }
}