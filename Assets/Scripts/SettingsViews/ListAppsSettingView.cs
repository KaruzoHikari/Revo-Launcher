using Animations;
using Misc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
    public class ListAppsSettingView : SettingsView
    {
        
        public ListAppsSettingView() : base(SettingsController.SETTINGSMENU.APP_LAUNCH,
            "settings.title.selectapp", ThemeColor.SettingsBackgroundLight)
        {
        }

        public override void Show()
        {
            base.Show();
            EnableMainGameObject(_instance.selectionMenu);

            _instance.SetupAppSelection_Editor(OnSelectedApp, true);
        }

        private void OnSelectedApp(AppInfo appInfo)
        {
            // we just launch it
            AndroidLinker.LaunchApp(appInfo);
        }

        public override void ClickedBack()
        {
            _instance.ExitSettings(firstGrid: false);
        }
    }
}