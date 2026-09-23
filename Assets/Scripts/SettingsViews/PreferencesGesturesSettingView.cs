using Misc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
    public class PreferencesGesturesSettingView : SettingsView
    {
        public PreferencesGesturesSettingView() : base(SettingsController.SETTINGSMENU.PREFERENCES_GESTURES,
            "settings.title.preferences", ThemeColor.SettingsBackgroundDark)
        {
        }

        public override void Show()
        {
            base.Show();
            EnableMainGameObject(_instance.selectionMenu);
            AppController._instance.versionText.gameObject.SetActive(true);
            SetupGesturesPreferences();
        }

        public override void ClickedBack()
        {
            _instance.preferencesSettingView.Show();
        }
        
        private void SetupGesturesPreferences()
        {
            SetContentPivot(1f);
            PreferencesSpawner.SpawnVoid(55f);

            PreferencesSpawner.SpawnPreference_Bool("preferences.allowswipe.title",
                "preferences.allowswipe.description",
                PREFS.AllowSwipe, SwipeController._instance.RefreshSetting);
            PreferencesSpawner.SpawnPreference_Bool("preferences.allowswipevertical.title",
                "preferences.allowswipevertical.description",
                PREFS.AllowSwipeDown, SwipeController._instance.RefreshSetting);
            PreferencesSpawner.SpawnPreference_Bool("preferences.allowdrag.title",
                "preferences.allowdrag.description",
                PREFS.AllowDrag, SwipeController._instance.RefreshSetting);
            PreferencesSpawner.SpawnPreference_Bool("preferences.holdempty.title",
                "preferences.holdempty.description",
                PREFS.HoldEmptyChannel, SwipeController._instance.RefreshSetting);
            PreferencesSpawner.SpawnPreference_Bool("preferences.holdchannel.title",
                "preferences.holdchannel.description",
                PREFS.EnableHoldChannel, SwipeController._instance.RefreshSetting);
            PreferencesSpawner.SpawnPreference_Bool("preferences.allowdoubleclick.title",
                "preferences.allowdoubleclick.description",
                PREFS.AllowDoubleClick, SwipeController._instance.RefreshSetting);
            PreferencesSpawner.SpawnPreference_Bool("preferences.holdsdcard.title",
                "preferences.holdsdcard.description",
                PREFS.HoldSdCard, SwipeController._instance.RefreshSetting);
        }
    }
}