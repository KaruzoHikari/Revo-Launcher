using Misc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
    public class PreferencesWiimoteSettingView : SettingsView
    {
        public PreferencesWiimoteSettingView() : base(SettingsController.SETTINGSMENU.PREFERENCES_WIIMOTE,
            "settings.title.preferences", ThemeColor.SettingsBackgroundDark)
        {
        }

        public override void Show()
        {
            base.Show();
            EnableMainGameObject(_instance.selectionMenu);
            AppController._instance.versionText.gameObject.SetActive(true);
            SetupWiimotePreferences();
        }

        public override void ClickedBack()
        {
            _instance.preferencesSettingView.Show();
        }
        
        private void SetupWiimotePreferences()
        {
            SetContentPivot(1f);
            PreferencesSpawner.SpawnVoid(55f);

            GameObject wiimotePrefab = GameObject.Instantiate(_instance.settingsPreferencePairWiimote, _instance.optionsHolder);
            wiimotePrefab.GetComponent<Button>().onClick.AddListener(() =>
            {
                WiimoteController._instance.InitWiimotes();
                PopupController.ShowPopup("popup.pairwiimotes");
            });

            PreferencesSpawner.SpawnPreference_Float("preferences.wiimotes.nunchunk.title",
                "preferences.wiimotes.nunchunk.description",
                PREFS.NunchuckSensitivity, 0f, 100f,
                () => { WiimoteController._instance.RefreshSensitivity(); });
            PreferencesSpawner.SpawnPreference_Float("preferences.wiimotes.dpad.title",
                "preferences.wiimotes.dpad.description",
                PREFS.DPadSensitivity, 0f, 100f,
                () => { WiimoteController._instance.RefreshSensitivity(); });
            PreferencesSpawner.SpawnVoid(30f);
        }
    }
}