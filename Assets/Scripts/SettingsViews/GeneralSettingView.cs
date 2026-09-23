using UnityEngine.Events;

namespace SettingsViews
{
    public class GeneralSettingView : SettingsView
    {
        public GeneralSettingView() : base(SettingsController.SETTINGSMENU.GENERAL, "settings.title.settings", ThemeColor.SettingsBackgroundDark)
        {
        }

        public override void Show()
        {
            base.Show();
            EnableMainGameObject(_instance.generalButtons);
        }

        public override void ClickedBack()
        {
            SettingsController._instance.ExitSettings();
        }
    }
}