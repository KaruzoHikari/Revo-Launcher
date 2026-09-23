using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Animations;
using Data;
using Misc;
using RTLTMPro;
using SettingsViews;
using SFB;
using TMPro;
using TriInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

/*
 * I regret pretty much everything in this class. In case anyone ever sees this - I'm so sorry.
 * Kaz, 2023/12/07: Hope it's a little better now that there's separate view classes for each action
 */

public class SettingsController : MonoBehaviour
{
    public static SettingsController _instance;
    public GameObject settingsView;
    public GameObject generalButtons;
    public GameObject themeButtons;
    public GameObject wallpaperButtons;
    public GameObject channelTypeSelection;
    public RectTransform optionsHolder;
    public GameObject selectionMenu;
    public RawImage background;
    public ThemedBackground backgroundTheme;
    public Button backButton;
    public Button createButton;
    public TextMeshProUGUI title;
    public GameObject optionsFader;
    public GameObject actionButtons;
    public GameObject themeActionButtons;
    public Scrollbar preferencesScrollbar;
    public GameObject helpIcon;
    public GameObject searchIcon;
    public TMP_InputField searchField;
    private Dictionary<GameObject, List<string>> searchDictionary = new();
    public RectTransform optionsViewport;

    [Title("Prefab")]
    public GameObject settingsAppPrefab;
    public GameObject settingsChannelPrefab;
    public GameObject settingsChannelOnlinePrefab;
    public GameObject settingsSetAppLauncherPrefab;
    public GameObject settingsRemoveAppLauncherPrefab;
    public GameObject settingsPreferenceAppPrefab;
    public GameObject settingsPreferenceQuickReview;
    public GameObject settingsPreferenceWiimoteMenu;
    public GameObject settingsPreferencePairWiimote;
    public GameObject settingsPreferenceGesturesMenu;
    public GameObject settingsPreferenceRestart;
    public GameObject settingsPreferenceDiscord;
    public GameObject preferencePrefab;
    public GameObject preferenceIntPrefab;
    public GameObject preferenceFloatPrefab;
    public GameObject preferenceFloatSliderPrefab;
    public GameObject preferenceDropdownPrefab;

    [Title("Channel targets")]
    public GameObject setupChannelAppPrefab;
    public GameObject setupChannelAppMockPrefab;
    public GameObject setupChannelEmulatorPrefab;
    public GameObject setupChannelSteamPrefab;
    public GameObject setupChannelFilePrefab;
    public GameObject setupChannelWebPrefab;
    public GameObject setupChannelWebhookPrefab;
    public GameObject setupChannelShopPrefab;
    public GameObject setupChannelAppleLinkPrefab;
    public GameObject setupChannelAppleShortcutsPrefab;
    public GameObject appSourcePrefab;
    public GameObject pathSourcePrefab;
    public GameObject coverSourcePrefab;
    public GameObject iconSourcePrefab;
    public GameObject bannerSourcePrefab;

    [Title("Views")]
    public GeneralSettingView generalSettingView = new GeneralSettingView();
    public PreferencesSettingView preferencesSettingView = new PreferencesSettingView();
    public PreferencesWiimoteSettingView preferencesWiimoteSettingView = new PreferencesWiimoteSettingView();
    public PreferencesGesturesSettingView preferencesGesturesSettingView = new PreferencesGesturesSettingView();
    public EditorTypeSettingView editorTypeSettingView = new EditorTypeSettingView();
    public ThemesSettingView themesSettingView = new ThemesSettingView();
    public WallpaperSettingView wallpaperSettingView = new WallpaperSettingView();
    public ThemeUploadSettingView themeUploadSettingView = new ThemeUploadSettingView();
    public ThemeApplySettingView themeApplySettingView = new ThemeApplySettingView();
    public ThemeCreateSettingView themeCreateSettingView = new ThemeCreateSettingView();
    public ThemeDeleteSettingView themeDeleteSettingView = new ThemeDeleteSettingView();
    public ThemeEditSettingView themeEditSettingView = new ThemeEditSettingView();
    public ThemeExportSettingView themeExportSettingView = new ThemeExportSettingView();
    public ChannelEditSettingView channelEditSettingView = new ChannelEditSettingView();
    public ChannelUploadSettingView channelUploadSettingView = new ChannelUploadSettingView();
    public ChannelCloneSettingView channelCloneSettingView = new ChannelCloneSettingView();
    public ChannelExportSettingView channelExportSettingView = new ChannelExportSettingView();
    public ChannelConvertSettingView channelConvertSettingView = new ChannelConvertSettingView();
    public ChannelDeleteSettingView channelDeleteSettingView = new ChannelDeleteSettingView();
    public SetupTypeSettingView setupTypeSettingView = new SetupTypeSettingView();
    public SetupAppSettingView setupAppSettingView = new SetupAppSettingView();
    public SetupAnimSettingView setupAnimSettingView = new SetupAnimSettingView();
    public SetupFileSettingView setupFileSettingView = new SetupFileSettingView();
    public SetupEmulatorSettingView setupEmulatorSettingView = new SetupEmulatorSettingView();
    public SetupSteamSettingView setupSteamView = new SetupSteamSettingView();
    public SetupEmulatorPathSettingView setupEmulatorPathSettingView = new SetupEmulatorPathSettingView();
    public SetupOverviewAppSettingView setupOverviewAppSettingView = new SetupOverviewAppSettingView();
    public SetupOverviewFileSettingView setupOverviewFileSettingView = new SetupOverviewFileSettingView();
    public SetupOverviewWebSettingView setupOverviewWebSettingView = new SetupOverviewWebSettingView();
    public SetupOverviewEmulatorSettingView setupOverviewEmulatorSettingView = new SetupOverviewEmulatorSettingView();
    public SetupOverviewShopSettingView setupOverviewShopSettingView = new SetupOverviewShopSettingView();
    public SetupOverviewWebhookSettingView setupOverviewWebhookSettingView = new SetupOverviewWebhookSettingView();
    public SetupOverviewAppleLinkSettingView setupOverviewAppleLinkSettingView = new SetupOverviewAppleLinkSettingView();
    public SetupOverviewAppleShortcutSettingView setupOverviewAppleShortcutSettingView = new SetupOverviewAppleShortcutSettingView();
    //public SetupOverviewSteamSettingView setupOverviewSteamSettingView = new SetupOverviewSteamSettingView();
    public ListAppsSettingView listAppsSettingView = new ListAppsSettingView();
    public ListAppsSdSettingView listAppsSdSettingView = new ListAppsSdSettingView();
    public ListAppsMailSettingView listAppsMailSettingView = new ListAppsMailSettingView();

    public SettingsView currentMenu;

    // These variables save the information for the creation of a new channel
    private AppInfo newChannelApp;
    private ChannelAnimation newChannelIcon;
    private ChannelAnimation newChannelBanner;

    private void Awake()
    {
        _instance = this;
        backgroundTheme = background.GetComponent<ThemedBackground>();
        searchField.onValueChanged.AddListener(text => FilterSearchItems(text));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F11))
        {
            bool pref = PREFS.FullScreen.GetBool();
            Debug.Log($"Going to fullscreen! {!pref}");
            PREFS.FullScreen.SetBool(!pref);
            AppController._instance.RefreshCanvasSize();
        }
    }

    public void EnableMainGameObject(GameObject obj)
    {
        generalButtons.SetActive(generalButtons.Equals(obj));
        channelTypeSelection.SetActive(channelTypeSelection.Equals(obj));
        selectionMenu.SetActive(selectionMenu.Equals(obj));
        themeButtons.SetActive(themeButtons.Equals(obj));
        wallpaperButtons.SetActive(wallpaperButtons.Equals(obj));
    }

    public void SetBackgroundColor(ThemeColor themeColor)
    {
        backgroundTheme.themeColor = themeColor;
        backgroundTheme.UpdateElement();
    }

    public void ExitSettings(bool toMainMenu = true, bool firstGrid = true, bool transition = true)
    {
        if (AppController.IsTransitioning())
        {
            return;
        }

        AppController.SetTransitioning(true);
        StartCoroutine(_ExitSettings(toMainMenu, firstGrid, transition));
    }

    private IEnumerator _ExitSettings(bool toMainMenu, bool firstGrid, bool transition)
    {
        if (transition)
        {
            FadeController._instance.FadeInAndOut(1f, 1f, 1f);
            yield return new WaitForSeconds(1.5f * FadeController.GetSpeedMultiplier());
        }
        EnableMainGameObject(null);
        ClearActiveList();
        settingsView.SetActive(false);

        if (toMainMenu)
        {
            AppController._instance.ShowMainMenu();
            CameraController.UnlockHorizontalMode();
            int grid = firstGrid ? 0 : GridController._instance.currentGrid.number;
            GridController._instance.ChangeGrid(grid, true, 0f);

            foreach (EmptyAppHandler handler in GridController._instance.GetAllEmptyAppHandlers())
            {
                if (handler != null)
                {
                    handler.RestartAnimation();
                }
            }

            AnimationController._instance.Resume();
            AnimationController._instance.RestartIconAnimations();
        }

        if (transition)
        {
            yield return new WaitForSeconds(1.5f * FadeController.GetSpeedMultiplier());
        }
        AppController.SetTransitioning(false);
    }

    public void ClearActiveList()
    {
        if (optionsHolder.childCount > 0)
        {
            for (int i = 0; i < optionsHolder.childCount; i++)
            {
                Destroy(optionsHolder.GetChild(i).gameObject);
            }
        }

        AppController._instance.versionText.gameObject.SetActive(false);
        optionsFader.SetActive(false);
        actionButtons.SetActive(false);
        themeActionButtons.SetActive(false);
    }

    public void OpenSettings()
    {
        OpenSettings(generalSettingView);
    }

    public void OpenSettings(SettingsView view)
    {
        if (AppController.IsTransitioning() || ChannelController._instance.currentChannel != null)
        {
            return;
        }

        AppController.SetTransitioning(true);
        StartCoroutine(_OpenSettings(view));
    }

    private IEnumerator _OpenSettings(SettingsView view)
    {
        if (view is null)
        {
            // We go back to the main menu
            AppController.SetTransitioning(false);
            ExitSettings();
            
            // And we clear every menu
            yield return new WaitForSeconds(1.5f * FadeController.GetSpeedMultiplier());
            ShopController._instance.ClearShop();
        }
        else
        {
            // We go to the menu we're supposed to go to
            FadeController._instance.FadeInAndOut(1f, 1f, 1f);

            yield return new WaitForSeconds(1.5f * FadeController.GetSpeedMultiplier());
            ClearOtherMenus(view.GetMenu());

            // We start the new menu
            view.Show();

            yield return new WaitForSeconds(1.5f * FadeController.GetSpeedMultiplier());
            AppController.SetTransitioning(false);
        }
    }

    public void ClearOtherMenus(SETTINGSMENU targetMenu)
    {
        // We clear the editor (in case we come from there)
        EditorController._instance.ClearOptions();
        EditorController._instance.channelEditor.SetActive(false);
        AppController._instance.PauseMainMenu();
        
        // We clear the search options
        ClearSearch();

        // And we also clear the shop
        if (targetMenu != SETTINGSMENU.CHANNEL_SELECTION_UPLOAD && targetMenu != SETTINGSMENU.THEME_SELECTION_UPLOAD)
        {
            ShopController._instance.ClearShop();
        }
    }

    public void OpenHelp()
    {
        Application.OpenURL("https://github.com/KaruzoHikari/Revo-Launcher/wiki");
    }
    
    public void ClickedSearch()
    {
        SetSearchField(!searchField.gameObject.activeSelf);
    }
    
    public void ClickedBack()
    {
        currentMenu?.ClickedBack();
    }
    
    public void ClickedMore()
    {
        currentMenu?.ClickedCreate();
    }
    
    public void ClickedMoreAnims()
    {
        optionsFader.SetActive(!optionsFader.activeInHierarchy);
        actionButtons.SetActive(!actionButtons.activeInHierarchy);
    }
    
    public void ClickedMoreThemes()
    {
        optionsFader.SetActive(!optionsFader.activeInHierarchy);
        themeActionButtons.SetActive(!themeActionButtons.activeInHierarchy);
    }

    public void ClickedCreate()
    {
        if (currentMenu.GetMenu() == SETTINGSMENU.THEME_SELECTION_APPLY)
        {
            if (ThemeController._instance.IsOpen())
            {
                PopupController.ShowPopup("popup.nocreatetheme");
            }
            else
            {
                themeCreateSettingView.Show();
            }
        }
        else
        {
            CreateNewChannelAnimation();
        }
    }

    public void ClickedImport()
    {
        if (ThemeController._instance.IsOpen())
        {
            PopupController.ShowPopup("popup.noimporttheme");
        }
        else
        {
            if (currentMenu.GetMenu() == SETTINGSMENU.THEME_SELECTION_APPLY)
            {
                RequestImportTheme();
            }
            else
            {
                RequestImportAnimation();
            }
        }
    }

    private CHANNELTYPE GetCurrentChannelType()
    {
        if (currentMenu is AbstractChannelsSettingView view)
        {
            return view.GetChannelType();
        }
        // default to icon
        return CHANNELTYPE.ICON;
    }

    private void RequestImportAnimation()
    {
        FileManager.RequestFile("Choose a Channel Animation to import it!", FILETYPE.ZIP, path =>
        {
            SaveManager.ImportChannelAnimation(path);
            PopupController.ShowPopup("popup.importedanim",
                () => { channelEditSettingView.Show(GetCurrentChannelType()); }, true,
                new[] { FileManager.GetFileName(path) });
        });
    }
    
    private void RequestImportTheme()
    {
        FileManager.RequestFile("Choose a theme to import it", FILETYPE.ZIP, path =>
        {
            if (path == null)
            {
                Debug.Log("Operation cancelled");
            }
            else
            {
                SaveManager.ImportTheme(path);
                PopupController.ShowPopup("popup.importedtheme",
                    () => { themeApplySettingView.Show(); }, true,
                    new[] {FileManager.GetFileName(path)});
            }
        });
    }
    
    public void ClickedEdit()
    {
        if (ThemeController._instance.IsOpen())
        {
            PopupController.ShowPopup("popup.noedittheme");
        }
        else
        {
            PopupController.ShowPopup("popup.selectedittheme", () => { themeEditSettingView.Show(); });
        }
    }

    public void ClickedExport()
    {
        if (currentMenu.GetMenu() == SETTINGSMENU.THEME_SELECTION_APPLY)
        {
            if (ThemeController._instance.IsOpen())
            {
                PopupController.ShowPopup("popup.noexporttheme");
            }
            else
            {
                PopupController.ShowPopup("popup.selectexporttheme", () => { themeExportSettingView.Show(); });
            }
        }
        else
        {
            PopupController.ShowPopup("popup.selectexport", () => { channelExportSettingView.Show(GetCurrentChannelType()); });
        }
    }
    
    public void ClickedDelete()
    {
        if (currentMenu.GetMenu() == SETTINGSMENU.THEME_SELECTION_APPLY)
        {
            if (ThemeController._instance.IsOpen())
            {
                PopupController.ShowPopup("popup.nodeletetheme");
            }
            else
            {
                PopupController.ShowPopup("popup.selectdeletetheme", () => { themeDeleteSettingView.Show(); });
            }
        }
        else
        {
            PopupController.ShowPopup("popup.selectdelete", () => { channelDeleteSettingView.Show(GetCurrentChannelType()); });
        }
    }

    public void ClickedClone()
    {
        PopupController.ShowPopup("popup.selectclone", () => { channelCloneSettingView.Show(GetCurrentChannelType()); });
    }
    
    public void ClickedConvert()
    {
        PopupController.ShowPopup("popup.selectconvert", () => { channelConvertSettingView.Show(GetCurrentChannelType()); });
    }

    public void EditChannel(Channel channel)
    {
        switch (channel.GetTarget().GetTargetType())
        {
            case CHANNELTARGETS.APP:
            {
                setupOverviewAppSettingView.Show(channel);
                break;
            }
            case CHANNELTARGETS.WEB:
            {
                setupOverviewWebSettingView.Show(channel);
                break;
            }
            case CHANNELTARGETS.FILE:
            {
                setupOverviewFileSettingView.Show(channel);
                break;
            }
            case CHANNELTARGETS.EMULATOR:
            {
                setupOverviewEmulatorSettingView.Show(channel);
                break;
            }
            case CHANNELTARGETS.SHOP:
            {
                setupOverviewShopSettingView.Show(channel);
                break;
            }
            case CHANNELTARGETS.WEBHOOK:
            {
                setupOverviewWebhookSettingView.Show(channel);
                break;
            }
            case CHANNELTARGETS.STEAM:
            {
                setupOverviewEmulatorSettingView.Show(channel);
                break;
            }
            case CHANNELTARGETS.APPLE_LINK:
            {
                setupOverviewAppleLinkSettingView.Show(channel);
                break;
            }
            case CHANNELTARGETS.APPLE_SHORTCUTS:
            {
                setupOverviewAppleShortcutSettingView.Show(channel);
                break;
            }
        }
    }

    public void RequestExportData()
    {
        //StartCoroutine(_RequestExportData());
        string zipPath = SaveManager._instance.ExportData();
        FileManager.ExportFile("Save the app's data", FILETYPE.ZIP, zipPath, "RevoData.zip", done =>
        {
            if (done)
            {
                PopupController.ShowPopup("popup.exporteddata");
            }
        }, true);
    }

    public void RequestImportData()
    {
        //StartCoroutine(_RequestImportData());
        FileManager.RequestFile("Choose your app's data to import it", FILETYPE.ZIP, path =>
        {
            if (path == null)
            {
                Debug.Log("Operation cancelled");
            }
            else
            {
                // We import the data
                bool imported = SaveManager._instance.ImportData(path);

                // And we restart the app
                if (imported)
                {
                    #if UNITY_IOS
                    PopupController.ShowPopup("popup.importeddatanorestart", false);
                    #else
                    PopupController.ShowPopup("popup.importeddata", () => { AndroidLinker._instance.RestartApp(); });
                    #endif
                }
            }
        }, true);
    }

    private void SetupSearch()
    {
        ClearSearch(); // we clear the previous state
        searchIcon.SetActive(true);
        helpIcon.SetActive(false);
    }

    private void ClearSearch()
    {
        helpIcon.SetActive(true);
        searchIcon.SetActive(false);
        searchField.text = null;
        SetSearchField(false);
        searchDictionary.Clear();
    }

    private void SetSearchField(bool option)
    {
        searchField.gameObject.SetActive(option);
        optionsViewport.offsetMax = new Vector2(optionsViewport.offsetMax.x, option ? -250-160 : -250);
        LayoutRebuilder.ForceRebuildLayoutImmediate(optionsViewport);
        if (option)
        {
            searchField.Select();
        }
    }

    private void AddSearchItem(GameObject prefab, string text)
    {
        if (prefab != null)
        {
            // we get or generate the list, and then add the lowercase text
            List<string> list;
            if (searchDictionary.ContainsKey(prefab))
            {
                list = searchDictionary[prefab];
            }
            else
            {
                list = new List<string>();
                searchDictionary[prefab] = list;
            }
            list.Add(text.ToLowerInvariant());
        }
    }

    private void FilterSearchItems(string text)
    {
        if (!searchIcon.activeInHierarchy || text == null)
        {
            return;
        }

        string low = text.ToLowerInvariant();
        foreach (GameObject obj in searchDictionary.Keys)
        {
            obj.SetActive(searchDictionary[obj].Any(x => x.Contains(low)));
        }
    }
    
    public void SetupAppSelection_Editor(UnityAction<AppInfo> action, bool allowDeletion = false)
    {
        // We setup the search button
        SetupSearch();
        
        // Now we setup the apps
        if (AndroidLinker._instance.IsAppCountAccurate())
        {
            PopupController.ClosePopup();
            SpawnAllApps(action, allowDeletion);
        }
        else
        {
            StartCoroutine(_RetrieveApps(action, allowDeletion));
        }
    }

    private IEnumerator _RetrieveApps(UnityAction<AppInfo> action, bool allowDeletion)
    {
        PopupController.ShowPopup("popup.retrievingapps", null);
        yield return new WaitForSeconds(0.6f);
        AndroidLinker._instance.FindAllApps();
        PopupController.ClosePopup();
        SpawnAllApps(action, allowDeletion);
    }

    private void SpawnAllApps(UnityAction<AppInfo> action, bool allowDeletion)
    {
        foreach (AppInfo appInfo in AndroidLinker._instance.apps)
        {
            GameObject prefab = Instantiate(settingsAppPrefab, optionsHolder);
            prefab.transform.Find("Text").GetComponent<RTLTextMeshPro>().text = appInfo.name;
            prefab.transform.Find("AppIcon").GetComponent<RawImage>().texture = appInfo.icon;
            prefab.GetComponent<Button>().onClick.AddListener(() => action.Invoke(appInfo));
            AddSearchItem(prefab, appInfo.name);

            if (allowDeletion)
            {
                // we setup the long click to delete an app, so people can actually use this as a launcher
                prefab.GetComponent<LongClickButton>().onLongClick.AddListener(() =>
                {
                    PopupController.ShowPopup("popup.deleteapp", () =>
                    {
                        AndroidLinker.UninstallApp(appInfo.packageName, () => Destroy(prefab));
                        PopupController.ClosePopup();
                    }, PopupController.ClosePopup);
                });
            }
        }
    }
    
    public void SetupSteamSelection_Editor(UnityAction<SteamGame> action)
    {
        // We setup the search button
        SetupSearch();
        
        // Now we setup the games
        foreach (SteamGame game in MetadataController._instance.GetInstalledSteamGames())
        {
            GameObject prefab = Instantiate(settingsAppPrefab, optionsHolder);
            prefab.transform.Find("Text").GetComponent<RTLTextMeshPro>().text = game.gameTitle;
            prefab.transform.Find("AppIcon").GetComponent<RawImage>().texture = MetadataController._instance.steamIcon;
            prefab.GetComponent<Button>().onClick.AddListener(() => action.Invoke(game));
            AddSearchItem(prefab, game.gameTitle);
        }
    }

    public void OnClickSdCard()
    {
        #if UNITY_IOS
        Application.OpenURL("shareddocuments://");
        #else
        StartCoroutine(_OpenAppsSelection(listAppsSdSettingView));
        #endif
    }
    
    public void OnHoldSdCard()
    {
        if (SwipeController._instance.shouldCheckHoldSdCard)
        {
            GridController._instance.SetSdCardColor(!PREFS.SdCardColor.GetBool());
            PopupController.ShowPopup("popup.changedsdcolor");
        }
    }

    public void OnHoldWiiMenu()
    {
        #if !UNITY_IOS // can't list your apps in iOS
        StartCoroutine(_OpenAppsSelection(listAppsSettingView));
        #endif
    }

    private IEnumerator _OpenAppsSelection(SettingsView target)
    {
        FadeController._instance.FadeInAndOut(0.2f,0.1f,0.2f);
        yield return new WaitForSeconds(0.25f * FadeController.GetSpeedMultiplier());
        target.Show();
    }

    public void SetContentPivot(float pivot)
    {
        optionsHolder.pivot = new Vector2(optionsHolder.pivot.x, pivot);
    }

    public void SetupChannelSelection_Editor(CHANNELTYPE type, bool includeOnline, UnityAction<ChannelAnimation> action, bool includeEmulator = true)
    {
        // We setup the search button
        SetupSearch();
        
        // And we start the coroutine
        StartCoroutine(_SetupChannelSelection_Editor(type, includeOnline, includeEmulator, action));
    }

    private IEnumerator _SetupChannelSelection_Editor(CHANNELTYPE type, bool includeOnline, bool includeEmulator, UnityAction<ChannelAnimation> action)
    {
        PopupController.ShowPopup("popup.retrievinganimations", null);
        yield return new WaitForSeconds(0.6f);
        SetContentPivot(1f);
        List<AnimationBasicInfo> list = ChannelController._instance.GetAllChannelInfo(type).OrderBy(elem => elem.animationName).ToList();
        foreach (AnimationBasicInfo info in list)
        {
            if ((!info.isOnline || includeOnline) && (!info.isEmulator || includeEmulator))
            {
                SpawnAnimationOrTheme(info, () => action.Invoke(ChannelController._instance.GetOrLoadChannelAnimation(info.animationFileName)));
            }
        }
        PopupController.ClosePopup();
    }

    public void SetupThemeSelection_Editor(bool includeOnline, UnityAction<Theme> action)
    {
        // We setup the search button
        SetupSearch();

        // And start the theme coroutine
        StartCoroutine(_SetupThemeSelection_Editor(includeOnline, action));
    }

    private IEnumerator _SetupThemeSelection_Editor(bool includeOnline, UnityAction<Theme> action)
    {
        PopupController.ShowPopup("popup.retrievingthemes", null);
        yield return new WaitForSeconds(0.6f);
        SetContentPivot(1f);
        List<AnimationBasicInfo> list = ChannelController._instance.GetAllThemeInfo().OrderBy(elem => elem.animationName).ToList();
        foreach (AnimationBasicInfo info in list)
        {
            if (!info.isOnline || includeOnline)
            {
                SpawnAnimationOrTheme(info, () => action.Invoke(ThemeController._instance.GetOrLoadTheme(info.animationFileName)), 
                    false, info.animationFileName.Equals(ThemeController._instance.currentTheme.importedFileName));
            }
        }
        PopupController.ClosePopup();
    }

    private void SpawnAnimationOrTheme(AnimationBasicInfo info, UnityAction callback, bool isAnim = true, bool addCheckMark = false)
    {
        GameObject prefab;
        if (!info.isOnline)
        {
            prefab = Instantiate(settingsChannelPrefab, optionsHolder);
            string lastEdit = info.animationLastEdited.Year + "/" + info.animationLastEdited.Month + "/" + info.animationLastEdited.Day;
            prefab.transform.Find("EditHolder").Find("LastEdit").GetComponent<TextMeshProUGUI>().text = lastEdit;
        }
        else
        {
            prefab = Instantiate(settingsChannelOnlinePrefab, optionsHolder);
            OnlineInfo onlineInfo;
            if (isAnim)
            {
                onlineInfo = ChannelController._instance.GetOnlineInfo(info.animationFileName,true,false);
            }
            else
            {
                onlineInfo = ThemeController._instance.GetOnlineInfo(info.animationFileName);
            }
            prefab.transform.Find("EditHolder").Find("Author").GetComponent<TextMeshProUGUI>().text = onlineInfo.authorName;
            AddSearchItem(prefab, onlineInfo.authorName);
        }
        
        prefab.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = info.animationName;
        AddSearchItem(prefab, info.animationName);

        if (addCheckMark)
        {
            prefab.transform.Find("Checkmark").Find("Icon").gameObject.SetActive(true);
        }
        
        prefab.GetComponent<Button>().onClick.AddListener(callback);
        if (isAnim)
        {
            prefab.GetComponent<LongClickButton>().onLongClick.AddListener(() =>
            {
                PreviewController._instance.ShowDecoy(ChannelController._instance.GetOrLoadChannelAnimation(info.animationFileName));
            });
            prefab.GetComponent<LongClickButton>().onRelease.AddListener(() =>
            {
                PreviewController._instance.HideDecoy(false);
            });
        }
        else
        {
            prefab.GetComponent<LongClickButton>().onLongClick.AddListener(() =>
            {
                themeEditSettingView.OnSelectedTheme(ThemeController._instance.GetOrLoadTheme(info.animationFileName));
            });
        }
    }

    public void ApplyTheme(Theme theme, bool useWallpaper)
    {
        PopupController.ShowPopup("popup.themeapplied");
        PREFS.UseThemeWallpaper.SetBool(useWallpaper);
        ThemeController._instance.SetNewTheme(theme, unloadPrevious: !ThemeController._instance.GetCurrentTheme().Equals(theme));
        themeApplySettingView.Show();
    }

    public void RequestExportTheme(Theme theme)
    {
        // StartCoroutine(_RequestExportTheme(theme));
        string zipPath = SaveManager.GetThemeImportedPath(theme);
        FileManager.ExportFile("Save theme", FILETYPE.ZIP, zipPath, theme.GetImportedFileName(), exported =>
        {
            if (exported)
            {
                PopupController.ShowPopup("popup.exportedtheme", () =>
                {
                    themesSettingView.Show();
                }, true, new [] { theme.name });
            }
        });
    }

    public void RequestExportAnim(ChannelAnimation anim)
    {
        // StartCoroutine(_RequestExportAnim(anim));
        string zipPath = SaveManager.GetChannelImportedPath(anim);
        FileManager.ExportFile("Save animation", FILETYPE.ZIP, zipPath, anim.GetImportedFileName(), exported =>
        {
            if (exported)
            {
                PopupController.ShowPopup("popup.exportedanim", () =>
                {
                    channelEditSettingView.Show(anim.type);
                }, true, new [] { anim.name });
            }
        });
    }

    public void CreateNewChannelAnimation()
    {
        // let's retrieve the channel type from the current menu
        if (currentMenu is AbstractChannelsSettingView view)
        {
            ChannelAnimation channelAnimation = new ChannelAnimation(view.GetChannelType());
            channelAnimation.RunFirstSetup();
        
            ChannelController._instance.LoadChannelAnimation(channelAnimation);
            EditorController._instance.OpenEditor(channelAnimation);
        }
    }

    public void OnSelectedIcon()
    {
        channelEditSettingView.Show(CHANNELTYPE.ICON);
    }
    
    public void OnSelectedBanner()
    {
        channelEditSettingView.Show(CHANNELTYPE.BANNER);
    }

    public void OnSelectedSettings()
    {
        preferencesSettingView.Show();
    }

    public void OnSelectedThemes()
    {
        themesSettingView.Show();
    }

    public void OnSelectedWallpaper()
    {
        wallpaperSettingView.Show();
    }

    public void OnSelectedApplyThemes()
    {
        themeApplySettingView.Show();
    }

    public void OnSelectedEditor()
    {
        editorTypeSettingView.Show();
    }

    public void ExecutedKonami()
    {
        PopupController.ShowPopup("popup.deletepreferences", PREFS.ResetAllPreferences, PopupController.ClosePopup);
    }

    public enum SETTINGSMENU
    {
        // These are the general setting menus:
        GENERAL, PREFERENCES, PREFERENCES_WIIMOTE, PREFERENCES_GESTURES, EDITOR_CHANNELTYPE, CHANNEL_SELECTION_EDITOR, CHANNEL_SELECTION_CLONE,
        CHANNEL_SELECTION_EXPORT, CHANNEL_SELECTION_UPLOAD, CHANNEL_SELECTION_DELETE, CHANNEL_SELECTION_CONVERT,
        // These are for settings up a channel:
        CHANNEL_SETUP_TYPE, CHANNEL_SETUP_WEB, CHANNEL_SETUP_EMULATOR, CHANNEL_SETUP_FILE, CHANNEL_SETUP_ANIM, CHANNEL_SETUP_APP,
        CHANNEL_SETUP_SHOP, CHANNEL_SETUP_WEBHOOK, CHANNEL_SETUP_STEAM, CHANNEL_SETUP_APPLE_LINK, CHANNEL_SETUP_APPLE_SHORTCUTS,
        // These are for themes:
        THEMES, THEME_SELECTION_EDITOR, THEME_SELECTION_APPLY, THEME_SELECTION_CREATE, THEME_SELECTION_EXPORT, THEME_SELECTION_UPLOAD, THEME_SELECTION_DELETE,
        WALLPAPER,
        // This is for launching an app:
        APP_LAUNCH, SD_LAUNCH, MAIL_CHOOSING
    }
}
