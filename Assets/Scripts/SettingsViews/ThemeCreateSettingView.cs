using Misc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
    public class ThemeCreateSettingView : AbstractThemesSettingView
    {
        public ThemeCreateSettingView() : base(SettingsController.SETTINGSMENU.THEME_SELECTION_CREATE)
        {
        }

        public override void Show()
        {
            base.Show();
            PopupController.ShowPopup("popup.choosethemeorigin", () =>
            {
                _instance.SetupThemeSelection_Editor(true, OnSelectedTheme);
            });
        }

        private void OnSelectedTheme(Theme theme)
        {
            // first we clone it based on the one we selected
            string clonedPath = ThemeController._instance.CloneTheme(theme);
        
            // now we launch the editor
            Theme newTheme = SaveManager.LoadTheme(clonedPath);
            PopupController.ShowPopup("popup.clonedtheme", () =>
            {
                ThemeController._instance.LaunchEditor(newTheme);
            }, replacementArray: new [] { theme.name });
        }

        public override void ClickedBack()
        {
            _instance.themesSettingView.Show();
        }
    }
}