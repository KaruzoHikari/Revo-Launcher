using Misc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
    public class ThemeApplySettingView : AbstractThemesSettingView
    {
        public ThemeApplySettingView() : base(SettingsController.SETTINGSMENU.THEME_SELECTION_APPLY, true)
        {
        }

        public override void Show()
        {
            base.Show();
            _instance.SetupThemeSelection_Editor(true, OnSelectedTheme);
        }

        private void OnSelectedTheme(Theme theme)
        {
            if (theme == null)
            {
                Debug.Log("User chose a null theme!");
                if (AppController._instance.isThereNewVersion)
                {
                    PopupController.ShowPopup("popup.mismatchversion", replacementArray: new[] {AppController._instance.serverVersion, Application.version});
                }
                return;
            }
            
            if (ThemeController._instance.IsOpen())
            {
                PopupController.ShowPopup("popup.noapplytheme");
            }
            else
            {
                if (theme.wallpaper != null)
                {
                    PopupController.ShowPopup("popup.usewallpaper", () => _instance.ApplyTheme(theme, true),
                        () => _instance.ApplyTheme(theme, false));
                }
                else
                {
                    _instance.ApplyTheme(theme, false);
                }
            }
        }

        public override void ClickedCreate()
        {
            _instance.ClickedMoreThemes();
        }

        public override void ClickedBack()
        {
            _instance.themesSettingView.Show();
        }
    }
}