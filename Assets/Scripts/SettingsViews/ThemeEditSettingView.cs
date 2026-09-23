using Misc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
    public class ThemeEditSettingView : AbstractThemesSettingView
    {
        public ThemeEditSettingView() : base(SettingsController.SETTINGSMENU.THEME_SELECTION_EDITOR)
        {
        }

        public override void Show()
        {
            base.Show();
            _instance.SetupThemeSelection_Editor(true, OnSelectedTheme);
        }

        public void OnSelectedTheme(Theme theme)
        {
            if (theme.IsOnlineTheme() || theme.isDefaultTheme)
            {
                // We forbid directly editing online or default animations
                // Instead, we recommend users to clone them
                PopupController.ShowPopup("popup.editforbiddentheme");
            }
            else
            {
                ThemeController._instance.LaunchEditor(theme);
            }
        }

        public override void ClickedBack()
        {
            _instance.themesSettingView.Show();
        }
    }
}