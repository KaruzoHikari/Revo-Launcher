using Misc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
    public class WallpaperSettingView : SettingsView
    {
        public WallpaperSettingView() : base(SettingsController.SETTINGSMENU.WALLPAPER,
            "settings.title.wallpaper", ThemeColor.SettingsBackgroundDark)
        {
        }

        public override void Show()
        {
            base.Show();
            EnableMainGameObject(_instance.wallpaperButtons);
        }

        public override void ClickedBack()
        {
            _instance.themesSettingView.Show();
        }
    }
}