using Misc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
    public class ThemesSettingView : SettingsView
    {
        public ThemesSettingView() : base(SettingsController.SETTINGSMENU.THEMES,
            "settings.title.themes", ThemeColor.SettingsBackgroundDark)
        {
        }

        public override void Show()
        {
            base.Show();
            EnableMainGameObject(_instance.themeButtons);
        }

        public override void ClickedBack()
        {
            _instance.generalSettingView.Show();
        }
    }
}