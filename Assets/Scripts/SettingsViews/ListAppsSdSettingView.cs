using Animations;
using Misc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
    public class ListAppsSdSettingView : SettingsView
    {
        
        public ListAppsSdSettingView() : base(SettingsController.SETTINGSMENU.SD_LAUNCH,
            "settings.title.selectapp", ThemeColor.SettingsBackgroundDark)
        {
        }

        // For now this class is pretty much the same as the ListApps one.
        // Maybe one day I do something more fancy with it
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