using Misc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
    public class ThemeDeleteSettingView : AbstractThemesSettingView
    {
        public ThemeDeleteSettingView() : base(SettingsController.SETTINGSMENU.THEME_SELECTION_DELETE)
        {
        }

        public override void Show()
        {
            base.Show();
            _instance.SetupThemeSelection_Editor(true, OnSelectedTheme);
        }

        private void OnSelectedTheme(Theme theme)
        {
            if (theme.isDefaultTheme)
            {
                // We shouldn't delete default themes
                PopupController.ShowPopup("popup.deleteforbiddentheme");
                return;
            }

            PopupController.ShowPopup("popup.confirmdeletetheme", () =>
            {
                // We delete it
                ThemeController._instance.DeleteTheme(theme);
        
                PopupController.ShowPopup("popup.deletedtheme", () =>
                {
                    _instance.themesSettingView.Show();
                }, true, new [] { theme.name });
            }, PopupController.ClosePopup, new [] { theme.name });
        }

        public override void ClickedBack()
        {
            _instance.themesSettingView.Show();
        }
    }
}