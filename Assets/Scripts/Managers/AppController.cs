using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Animations;
using DG.Tweening;
using IngameDebugConsole;
using TMPro;
using TriInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Color = UnityEngine.Color;
using Object = UnityEngine.Object;

public class AppController : MonoBehaviour
{
    public static AppController _instance;

    [Title("Build settings")]
    public TextMeshProUGUI versionText;
    public BuildInfo buildInfo;
    [HideInInspector] public TextAsset launcherManifest;
    [HideInInspector] public TextAsset noLauncherManifest;
    [ReadOnly] public bool isLauncherBuild;
    [ReadOnly] public bool isGithubBuild;
    [Button(ButtonSizes.Gigantic), GUIColor("$GetGitHubColor")]
    private void ToggleGithubBuild()
    {
        #if UNITY_EDITOR
        isGithubBuild = !isGithubBuild;
        string packageName = GetPackageName();
        string appName = isGithubBuild ? "Revo Launcher (APK)" : "Revo Launcher";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, packageName);
        PlayerSettings.productName = appName;
        Debug.Log($"The app is now: {PlayerSettings.productName} - {PlayerSettings.applicationIdentifier}");
        EditorUserBuildSettings.buildAppBundle = !isGithubBuild;
        /*PlayerSettings.Android.bundleVersionCode = RaiseBundleVersion();
        string path = Application.dataPath + @"/Plugins/Android/AndroidManifest.xml";
        Debug.Log(path);
        File.Delete(path);
        string text = isLauncherBuild ? launcherManifest.text : noLauncherManifest.text;
        File.WriteAllText(path,text);*/
        #endif
    }
    [ReadOnly] public bool isBeta;
    [Button(ButtonSizes.Medium), GUIColor("$GetBetaColor")]
    private void ToggleBeta()
    {
        #if UNITY_EDITOR
        isBeta = !isBeta;
        Debug.Log($"The app is now: {(isBeta ? "BETA" : "RELEASE")}");
        #endif
    }

    public string GetPackageName()
    {
        return "com.karuzohikari." + (isGithubBuild ? "wiiphonelauncher" : "wiiphone");
    }

    [Button(ButtonSizes.Medium)]
    private int RaiseBundleVersion()
    {
        int info = buildInfo.RaiseBundle();
        Debug.Log($"The version is now: {buildInfo.buildBundle}");
        #if UNITY_EDITOR
        PlayerSettings.Android.bundleVersionCode = info;
        #endif
        return info;
    }

    private Color GetGitHubColor() { return this.isGithubBuild ? Color.yellow : Color.cyan; }
    private Color GetBetaColor() { return this.isBeta ? Color.yellow : Color.cyan; }
    
    public DebugLogManager debugConsole;
    public bool finishedLoading;

    public bool isThereNewVersion;
    public string serverVersion;
    [UnGroupNext]
    
    [Title("Views")]
    public RectTransform gameCanvas;
    public RectTransform overlayCanvas;
    public GameObject mainView;
    public GameObject warningView;
    public GameObject channelView;
    
    private bool isLowMemoryMode;
    private bool isUltraLowMemoryMode;

    private bool isTransitioning = false;

    public float scale = 1;
    
    void Awake()
    {
        QualitySettings.vSyncCount = 1;
        RefreshFramerate();
        RefreshDragThreshold();
        RefreshAndroidFullscreen();
        Application.lowMemory += OnLowMemory;
        SetDebugConsole(PREFS.ShowDebugConsole.GetBool());

        if (_instance != null)
        {
            Destroy(this);
            return;
        }
        _instance = this;

        _instance.warningView.SetActive(true);
        RefreshScale();
        // RefreshCanvasSize();
    }

    public void RefreshCanvasSize()
    {
        RefreshScale();
        Vector2 size = new Vector2(CameraController.GetFixedWidth(), CameraController.GetFixedHeight());
        Debug.Log($"Setting game canvas size to: {size.x} - {size.y}");
        gameCanvas.sizeDelta = size;
        overlayCanvas.sizeDelta = size;
    }

    public void RefreshAndroidFullscreen()
    {
        // seems to be the same thing as in Windows, but some people have issues
        // (https://forum.unity.com/threads/android-hide-status-bar-show-navigation-bar.318557/)
        Screen.fullScreen = !PREFS.ShowNavigationBar.GetBool();
    }

    private void OnLowMemory()
    {
        Debug.Log("IMPORTANT!! Low memory detected!!");
        Resources.UnloadUnusedAssets();
    }

    public void SetDebugConsole(bool check)
    {
        debugConsole.gameObject.SetActive(check);
    }

    public void SetLowMemoryMode(bool status)
    {
        isLowMemoryMode = status;
    }

    public bool IsLowMemoryMode()
    {
        return isLowMemoryMode;
    }

    public void SetUltraLowMemoryMode(bool status)
    {
        isUltraLowMemoryMode = status;
    }

    public bool IsUltraLowMemoryMode()
    {
        return isUltraLowMemoryMode;
    }

    private void RefreshDragThreshold()
    {
        if (Application.isMobilePlatform)
        {
            int defaultValue = EventSystem.current.pixelDragThreshold;
            EventSystem.current.pixelDragThreshold =
                Mathf.Max(
                    defaultValue,
                    (int) (defaultValue * Screen.dpi / 160f));
        }
    }

    public void RefreshFramerate()
    {
        Application.targetFrameRate = PREFS.TargetFramerate.GetInt();
    }

    public void RefreshScale()
    {
        if (Application.isEditor || !Application.isMobilePlatform)
        {
            Debug.Log("Changing scale!");
            scale = PREFS.Scale.GetFloat();

            int regularWidth = (CameraController.IsPortrait() ? 450 : 800);
            int regularHeight = (CameraController.IsPortrait() ? 800 : 450);
            
            int width = (int) (regularWidth * scale);
            int height = (int) (regularHeight * scale);
            
            bool shouldFullScreen = PREFS.FullScreen.GetBool();
            if (!shouldFullScreen)
            {
                // We need to make sure the window doesn't get out of the borders
                if (height > Screen.currentResolution.height * 0.975f)
                {
                    scale = (Screen.height / 800f) * 0.95f;
                }
                else if (width > Screen.currentResolution.width * 0.975f)
                {
                    scale = (Screen.height / 450f) * 0.95f;
                }
                
                // And we recalculate it
                width = (int) (regularWidth * scale);
                height = (int) (regularHeight * scale);
                PREFS.Scale.SetFloat(scale);
            }
            else
            {
                // We just set to max height and calculate width
                height = Screen.currentResolution.height;
                width = CameraController.IsPortrait() ? (int) (height * ((float) regularWidth / regularHeight)) : CameraController.GetMonitorWidth();
            }
            
            Debug.Log($"Setting scale to: {width} x {height} ({shouldFullScreen})");
            Screen.SetResolution(width,height,shouldFullScreen);
        }
    }
    
    public void OnClickDebug()
    {
        // we just resetup the version text, that way I don't need to compile a proper beta version
        isBeta = !isBeta;
        SetupVersionText();
    }

    public void OnHoldDebug()
    {
        // We ask the user whether they want to show or hide the debug console
        bool isEnabled = PREFS.ShowDebugConsole.GetBool();
        string popup = isEnabled ? "popup.hidedebug" : "popup.showdebug";
        PopupController.ShowPopup(popup, () =>
        {
            PREFS.ShowDebugConsole.SetBool(!isEnabled);
            SetDebugConsole(!isEnabled);
            PopupController.ClosePopup();
        }, PopupController.ClosePopup);
    }

    public void OnHoldDate()
    {
        AudioController._instance.TriggerAudioUpdate(true);
    }

    private void Start()
    {
        if (ShouldStartWithWarning())
        {
            AnimationController._instance.FreezeGlobalTimer();
        }
        else
        {
            FadeController._instance.FadeIn(0f);
            SaveManager.LoadAllChannels();
            //StartMainMenu();
        }
        
        SetupVersionText();
        
        AndroidLinker.CheckPackagesPerm();
        Debug.Log("Loaded Revo Launcher!");
    }

    private void SetupVersionText()
    {
        versionText.text = "Revo Launcher v" + Application.version;
        if (isBeta)
        {
            versionText.text += "\n(b" + GetCurrentBundle() + ")";
        }
    }

    public bool ShouldStartWithWarning()
    {
        // Removed the option to skip warning, it only gives issues
        // return PlayerPrefs.GetInt("ShowWarning", 1) == 1;
        return true;
    }

    public void StartMainMenu()
    {
        AnimationController._instance.RestartIconAnimations();
        AudioController._instance.RegisterAudioCallback();
        GridController._instance.RefreshClocks();
        AudioController.PlayBackgroundMenuMusic();
        if (IsFirstGeneralLoad())
        {
            PREFS.FirstTimeOpening.SetBool(false);
            PopupController.ShowPopup("popup.welcome", CheckVersion);
        }
        else
        {
            CheckVersion();
        }

        if (IsFirstLoadCurrentVersion())
        {
            PREFS.FirstTimeCurrentVersion.SetBool(false);
        }
        if (IsFirstLoadCurrentBundle())
        {
            PREFS.LastOpenBundle.SetInt(GetCurrentBundle());
        }

        finishedLoading = true;
    }

    private async void CheckVersion()
    {
        string version = await WebRequestController.SendCheckVersion();
        if (!string.IsNullOrEmpty(version))
        {
            // we save the version
            serverVersion = version;
            
            // First we get them in number form
            string currentVersion = Application.version.Replace(".", "");
            version = version.Replace(".", "");
            
            // Them we pad them so they have the same number of 0's
            currentVersion = currentVersion.PadRight(version.Length, '0');
            version = version.PadRight(currentVersion.Length, '0');
            
            // Finally we transform them in numbers
            int current = int.Parse(currentVersion);
            int server = int.Parse(version);

            if (server > current)
            {
                // New version available!
                isThereNewVersion = true;
                PopupController.ShowPopup("popup.newversion", () =>
                {
                    // We send them either to the Play Store or to my web
                    if (Application.isMobilePlatform)
                    {
                        /*if (isLauncherBuild)
                        {
                            Application.OpenURL("https://karuzohikari.com/RevoLauncher/AndroidLauncher");
                        }
                        else
                        {*/
                            AndroidLinker.OpenInPlayStore(Application.identifier);
                        // }
                    }
                    else
                    {
                        Application.OpenURL("https://karuzohikari.com/RevoLauncher");
                    }
                }, PopupController.ClosePopup);
            }
        }
    }

    public bool IsInMainMenu()
    {
        return mainView.activeInHierarchy && !FadeController._instance.IsFading() && !warningView.activeInHierarchy;
    }

    public bool IsFirstGeneralLoad()
    {
        return SaveManager._instance.simulateFirstLoad || PREFS.FirstTimeOpening.GetBool();
    }

    public bool IsFirstLoadCurrentVersion()
    {
        return SaveManager._instance.simulateFirstLoad || PREFS.FirstTimeCurrentVersion.GetBool();
    }
    
    public bool IsFirstLoadCurrentBundle(bool includeEditor = true)
    {
        return IsFirstLoadCurrentVersion() ||  PREFS.LastOpenBundle.GetInt() < GetCurrentBundle() || (includeEditor && Application.isEditor);
    }

    public int GetCurrentBundle()
    {
        return buildInfo.buildBundle;
    }

    public void PauseMainMenu()
    {
        // We pause the animations
        AnimationController._instance.FreezeGlobalTimer();
        mainView.SetActive(false);
        WallpaperController._instance.StopVideoWallpaper();
    }

    public void ShowMainMenu()
    {
        mainView.SetActive(true);
        WallpaperController._instance.ResumeVideoWallpaper();
    }

    public void OnClickMessages()
    {
        #if UNITY_IOS
        StartCoroutine(_LaunchMessageApp(AndroidLinker.OpenMessages));
        return;
        #endif
        
        if (!PREFS.MessagingApp.IsDefault() && !string.IsNullOrEmpty(PREFS.MessagingApp.GetString()))
        {
            StartCoroutine(_LaunchMessageApp(() => AndroidLinker.LaunchApp(PREFS.MessagingApp.GetString())));
        }
        else
        {
            OnHoldMessages();
        }
    }

    private IEnumerator _LaunchMessageApp(UnityAction action)
    {
        FadeController._instance.FadeIn(1f);
        yield return new WaitForSeconds(1.2f * FadeController.GetSpeedMultiplier());
        action.Invoke();
        yield return new WaitForSeconds(0.25f * FadeController.GetSpeedMultiplier());
        FadeController._instance.FadeOut(0.1f);
    }

    public void OnHoldMessages()
    {
        #if UNITY_IOS // no app to select on iOS
        return;
        #endif
        
        if (IsTransitioning())
        {
            return;
        }
        
        PopupController.ShowPopup("popup.messagingapp.1", () => 
        {
            SettingsController._instance.listAppsMailSettingView.Show();
        }, false);
    }

    public static bool IsTransitioning()
    {
        return _instance.isTransitioning;
    }

    public static void SetTransitioning(bool trans)
    {
        _instance.isTransitioning = trans;
    }
}
