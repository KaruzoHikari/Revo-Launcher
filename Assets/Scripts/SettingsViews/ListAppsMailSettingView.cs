using Animations;
using Misc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
    public class ListAppsMailSettingView : SettingsView
    {
        
        public ListAppsMailSettingView() : base(SettingsController.SETTINGSMENU.MAIL_CHOOSING,
            "settings.title.selectapp", ThemeColor.SettingsBackgroundLight)
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
            // we save it
            PREFS.MessagingApp.SetString(appInfo.packageName);
            PopupController.ShowPopup("popup.messagingapp.2", () =>
            {
                SettingsController._instance.ExitSettings(firstGrid: false);
            });
        }

        public override void ClickedBack()
        {
            _instance.ExitSettings(firstGrid: false);
        }
    }
}