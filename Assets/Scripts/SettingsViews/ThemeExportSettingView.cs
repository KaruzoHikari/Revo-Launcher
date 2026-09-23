using Misc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
    public class ThemeExportSettingView : AbstractThemesSettingView
    {
        public ThemeExportSettingView() : base(SettingsController.SETTINGSMENU.THEME_SELECTION_EXPORT)
        {
        }

        public override void Show()
        {
            base.Show();
            _instance.SetupThemeSelection_Editor(true, OnSelectedTheme);
        }

        private void OnSelectedTheme(Theme theme)
        {
            SettingsController._instance.RequestExportTheme(theme);
        }

        public override void ClickedBack()
        {
            _instance.themesSettingView.Show();
        }
    }
}