using Misc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
    public class ThemeUploadSettingView : AbstractThemesSettingView
    {
        public ThemeUploadSettingView() : base(SettingsController.SETTINGSMENU.THEME_SELECTION_UPLOAD)
        {
        }

        public override void Show()
        {
            base.Show();
            _instance.SetupThemeSelection_Editor(false, ShopController._instance.OnSelectedTheme);
        }

        public override void ClickedBack()
        {
            ShopController._instance.OpenShop();
        }
    }
}