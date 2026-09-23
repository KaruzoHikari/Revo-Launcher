using Data;
using Misc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
    public class PreferencesSettingView : SettingsView
    {
        public PreferencesSettingView() : base(SettingsController.SETTINGSMENU.PREFERENCES,
            "settings.title.preferences", ThemeColor.SettingsBackgroundDark)
        {
        }

        public override void Show()
        {
            base.Show();
            EnableMainGameObject(_instance.selectionMenu);
            AppController._instance.versionText.gameObject.SetActive(true);
            SetupPreferences();
        }

        public override void ClickedBack()
        {
            _instance.generalSettingView.Show();
        }

        private void SetupPreferences()
        {
            SetContentPivot(1f);
            PreferencesSpawner.SpawnVoid(55f);

            if (Application.isMobilePlatform)
            {
                // we spawn the launcher button
                #if !UNITY_IOS
                bool launcher = PREFS.IsLauncherVersion.GetBool();
                GameObject prefab = GameObject.Instantiate(launcher ? _instance.settingsRemoveAppLauncherPrefab : _instance.settingsSetAppLauncherPrefab, _instance.optionsHolder);
                prefab.GetComponent<Button>().onClick.AddListener(() =>
                {
                    if (!launcher)
                    {
                        PopupController.ShowPopup("popup.shouldlauncher",
                            () =>
                            {
                                AndroidLinker._instance.SetLauncherVersion(true);
                                AndroidLinker._instance.OpenPhoneLauncherSettings();
                            }, PopupController.ClosePopup);
                    }
                    else
                    {
                        PopupController.ShowPopup("popup.shouldremovelauncher", () =>
                        {
                            AndroidLinker._instance.SetLauncherVersion(false);
                            PopupController.ShowPopup("popup.disabledlauncher", () =>
                            {
                                AndroidLinker._instance.OpenPhoneLauncherSettings();
                            }, () =>
                            {
                                // if we don't want to go to android's settings, we refresh the menu so it shows the button again
                                // todo test
                                Show();
                                PopupController.ClosePopup();
                            });
                        }, PopupController.ClosePopup);
                    }
                });
                #endif
                
                #if !UNITY_IOS
                // settings button
                GameObject settingPrefab = GameObject.Instantiate(_instance.settingsPreferenceAppPrefab, _instance.optionsHolder);
                settingPrefab.GetComponent<Button>().onClick.AddListener(() =>
                {
                    AndroidLinker._instance.OpenPhoneGeneralSettings();
                });
                
                // restart app button
                GameObject restart = GameObject.Instantiate(_instance.settingsPreferenceRestart, _instance.optionsHolder);
                restart.GetComponent<Button>().onClick.AddListener(() =>
                {
                    PopupController.ShowPopup("popup.restartapp", () =>
                    {
                        AndroidLinker._instance.RestartApp();
                    }, PopupController.ClosePopup);
                });
                #endif
            }
            else
            {
                // We spawn the Wiimote preferences
                GameObject wiimotePrefab = GameObject.Instantiate(_instance.settingsPreferenceWiimoteMenu, _instance.optionsHolder);
                wiimotePrefab.GetComponent<Button>().onClick.AddListener(() =>
                {
                    _instance.preferencesWiimoteSettingView.Show();
                });
            }
            
            // gestures. hopefully that way they'll learn how to lift channels
            GameObject gestures = GameObject.Instantiate(_instance.settingsPreferenceGesturesMenu, _instance.optionsHolder);
            gestures.GetComponent<Button>().onClick.AddListener(() =>
            {
                _instance.preferencesGesturesSettingView.Show();
            });

            PreferencesSpawner.SpawnVoid(90f);
            PreferencesSpawner.SpawnPreference_Bool("preferences.12hformat.title",
                "preferences.12hformat.description",
                PREFS.Is12Format, Clock.CheckPeriodText);
            PreferencesSpawner.SpawnPreference_Bool("preferences.americandate.title",
                "preferences.americandate.description",
                PREFS.IsAmericanDate, Date.RefreshAllDates);
            PreferencesSpawner.SpawnPreference_Language();
            PreferencesSpawner.SpawnPreference_Bool("preferences.sdcard.title",
                "preferences.sdcard.description",
                PREFS.ShowSdCard, () => { GridController._instance.RefreshSdCard(); });
            PreferencesSpawner.SpawnPreference_Bool("preferences.autoloadanims.title",
                "preferences.autoloadanims.description",
                PREFS.LoadChannelsAutomatically);
            
            // we spawn the experimental grid sizes. if it's custom, we separate it
            PreferencesSpawner.SpawnVoid(60f);
            PreferencesSpawner.SpawnPreference_GridSize();
            bool isCustom = GridController._instance.GetChosenGridSize() == GRIDSIZE.CUSTOM;
            if (isCustom)
            {
                bool vertical = !PREFS.AllowHorizontal.GetBool();
                PreferencesSpawner.SpawnPreference_Int("preferences.gridsizehorizontal.title",
                    "preferences.gridsizehorizontal.description",
                    vertical ? PREFS.NumberAppsX : PREFS.NumberAppsXHorizontal, 1, 12,
                    () => GridController._instance.SetAppNumbersFromPrefs());
                PreferencesSpawner.SpawnPreference_Int("preferences.gridsizevertical.title",
                    "preferences.gridsizevertical.description",
                    vertical ? PREFS.NumberAppsY : PREFS.NumberAppsYHorizontal, 1, 12,
                    () => GridController._instance.SetAppNumbersFromPrefs());
            }
            PreferencesSpawner.SpawnVoid(60f);
            
            PreferencesSpawner.SpawnPreference_Bool("preferences.horizontal.title",
                "preferences.horizontal.description",
                PREFS.AllowHorizontal, () =>
                {
                    if (PREFS.AllowHorizontal.GetBool())
                    {
                        PopupController.ShowPopup("popup.warninghorizontal");
                    }
                });
            PreferencesSpawner.SpawnPreference_Bool("preferences.loadbannersatboot.title",
                "preferences.loadbannersatboot.description",
                PREFS.LoadBannersAtBoot, () =>
                {
                    if (PREFS.LoadBannersAtBoot.GetBool())
                    {
                        PopupController.ShowPopup("popup.warningloadbanners");
                    }
                });
            PreferencesSpawner.SpawnPreference_Int("preferences.fpslimit.title",
                "preferences.fpslimit.description",
                PREFS.TargetFramerate, 10, 240,
                () => AppController._instance.RefreshFramerate());
            PreferencesSpawner.SpawnPreference_Bool("preferences.decimalstring.title",
                "preferences.decimalstring.description",
                PREFS.DecimalAsString);
            
            #if !UNITY_IOS
            PreferencesSpawner.SpawnPreference_Bool("preferences.standalonebrowser.title",
                "preferences.standalonebrowser.description",
                PREFS.StandaloneBrowser);
            #endif
            
            PreferencesSpawner.SpawnPreference_FloatSlider("preferences.musicvolume.title",
                "preferences.musicvolume.description",
                PREFS.MusicVolume, 0, 1,
                AudioController.RefreshBackgroundVolume);
            PreferencesSpawner.SpawnPreference_FloatSlider("preferences.sfxvolume.title",
                "preferences.sfxvolume.description",
                PREFS.SfxVolume, 0, 1,
                AudioController.RefreshSfxVolume);

            if (!Application.isMobilePlatform)
            {
                // We add a fullscreen option for Windows
                PreferencesSpawner.SpawnPreference_Bool("preferences.fullscreen.title",
                    "preferences.fullscreen.description",
                    PREFS.FullScreen, () => { AppController._instance.RefreshCanvasSize(); });
                PreferencesSpawner.SpawnPreference_Float("preferences.scale.title",
                    "preferences.scale.description",
                    PREFS.Scale, 0.25f, 2f, () => { AppController._instance.RefreshCanvasSize(); });
            }
            else
            {
                // We allow Android users to show the pointer
                PreferencesSpawner.SpawnPreference_Bool("preferences.showcursor.title",
                    "preferences.showcursor.description",
                    PREFS.ShowCursor, InputController._instance.RefreshPointerVisibility);
                // We allow showing the navigation bar
                PreferencesSpawner.SpawnPreference_Bool("preferences.navigationbar.title",
                    "preferences.navigationbar.description",
                    PREFS.ShowNavigationBar, () => { AppController._instance.RefreshAndroidFullscreen(); });
            }

            PreferencesSpawner.SpawnPreference_Bool("preferences.autoapps.title",
                "preferences.autoapps.description",
                PREFS.OpenDirectly);
            PreferencesSpawner.SpawnPreference_Int("preferences.numberofpages.title",
                "preferences.numberofpages.description",
                PREFS.NumberOfPages, 1, 20,
                () => GridController._instance.RefreshGrids());
            PreferencesSpawner.SpawnPreference_Float("preferences.fadeout.title",
                "preferences.fadeout.description",
                PREFS.FadeoutSpeed, 0.75f, 6f, () => { FadeController._instance.RefreshSpeed(); });

            PreferencesSpawner.SpawnVoid(90f);
            PreferencesSpawner.SpawnPreference_Void("preferences.dataimport.title", () =>
            {
                PopupController.ShowPopup("popup.dataimport",
                    () => { _instance.RequestImportData(); }, PopupController.ClosePopup);
            });
            PreferencesSpawner.SpawnPreference_Void("preferences.dataexport.title", () =>
            {
                PopupController.ShowPopup("popup.dataexport",
                    () => { _instance.RequestExportData(); }, PopupController.ClosePopup);
            });
            
            #if !UNITY_IOS
            PreferencesSpawner.SpawnPreference_Void("preferences.reloaddb.title", () =>
            {
                PopupController.ShowPopup("popup.updatetitles", () =>
                {
                    MetadataController._instance.UpdateTitles();
                }, PopupController.ClosePopup);
            });
            #endif

            PreferencesSpawner.SpawnVoid(90f);
            GameObject ratePrefab = GameObject.Instantiate(_instance.settingsPreferenceQuickReview, _instance.optionsHolder);
            ratePrefab.GetComponent<Button>().onClick.AddListener(() => { AndroidLinker._instance.OpenQuickReview(); });
            GameObject discordPrefab = GameObject.Instantiate(_instance.settingsPreferenceDiscord, _instance.optionsHolder);
            discordPrefab.GetComponent<Button>().onClick.AddListener(() =>
            {
                Application.OpenURL("https://discord.gg/xjrEMS9QY4");
            });
            PreferencesSpawner.SpawnPreference_Void("preferences.website.title",
                () => { Application.OpenURL("https://karuzohikari.com"); });
            PreferencesSpawner.SpawnPreference_Void("preferences.credits.title",
                () => { PopupController.ShowPopup("popup.credits"); });
            PreferencesSpawner.SpawnPreference_Void("preferences.licenses.title",
                () => { PopupController.ShowPopup("popup.licenses"); });
            PreferencesSpawner.SpawnPreference_Void("preferences.deleteaccount.title",
                () => { Application.OpenURL("https://karuzohikari.com/RevoLauncherDeleteAccount"); });
            PreferencesSpawner.SpawnVoid(30f);
        }
    }
}