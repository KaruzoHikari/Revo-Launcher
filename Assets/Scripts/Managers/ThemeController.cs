using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Animations;
using DG.Tweening;
using Misc;
using SFB;
using TMPro;
using TriInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

public class ThemeController : MonoBehaviour
{
    public static ThemeController _instance;
    public Theme defaultTheme;
    public Theme previousTheme;
    public Theme currentTheme;
    public GameObject themeGeneralObject;
    public CanvasGroup themeFullPanel;
    public ScrollRect themeFullPanelScrollbar;
    public CanvasGroup themeSelectionPanel;
    public ScrollRect themeSelectionPanelScrollbar;
    public RectTransform themeSelectionList;
    public CanvasGroup themeOverlay;
    public RectTransform pencilButton;
    public RectTransform handButton;
    public RectTransform openEditorButton;
    public GameObject selectionBackObject;
    public TMP_InputField nameField;
    public TextMeshProUGUI wallpaperName;
    public float offset = 50f;
    public GameObject themeReviewOverlay;
    public GameObject themePositionOverlay;

    private Tween currentTween;
    private Tween currentOverlayTween;
    private bool isInEditMode;
    private bool wasAudioMuted;

    [Title("Prefabs")]
    public GameObject themeColorPrefab;
    public GameObject themeTexturePrefab;
    public GameObject themeFontPrefab;
    public GameObject themeAudioPrefab;
    public GameObject themeTitlePrefab;
    public GameObject themeTransPrefab;
    public GameObject themeTextPrefab;
    public GameObject themeTextCategoryPrefab;

    [HideInInspector] public UnityEvent onThemeReload = new UnityEvent();

    [Title("Special overrides")]
    public List<Texture> specialTextures = new List<Texture>();
    public List<TMP_FontAsset> allFonts = new List<TMP_FontAsset>();
    public List<string> legacyNamesList = new List<string>();
    public Dictionary<string, string> legacyNames = new Dictionary<string, string>();
    private List<ThemeColor> textColors;
    private List<ThemeOffsets> skipOffsets = new() { ThemeOffsets.WiiClip, ThemeOffsets.WiiButton, ThemeOffsets.WiiShadow };

    // this is for the text menus. i really don't like it but :(
    private Dictionary<string, string> cacheBaseDictionary = null;
    private Dictionary<string, int> cacheTextCategories = null;
    private SystemLanguage cacheLanguage;
    private bool isViewingTexts = false;

    public static string defaultThemeFileName = "theme_riilight(default)_12345.zip";

    private void Awake()
    {
        _instance = this;
        themeGeneralObject.SetActive(false);
        themeFullPanel.gameObject.SetActive(false);
        themeSelectionPanel.gameObject.SetActive(false);
        themeFullPanelScrollbar.SetViewAtTop();
        themeSelectionPanelScrollbar.SetViewAtTop();
    }

    private void Start()
    {
        // we process the legacy names list
        ProcessLegacyNames();
        
        // first we load the default theme
        defaultTheme = GetOrLoadTheme(defaultThemeFileName);

        // and then we load our current one (which might or might not be the same one)
        LoadCurrentTheme();
    }

    private void ProcessLegacyNames()
    {
        foreach (string str in legacyNamesList)
        {
            string key = str.Split(';')[0];
            string value = str.Split(';')[1];
            legacyNames[key] = value;
        }
    }

    public string GetLegacyName(string str)
    {
        return (!string.IsNullOrEmpty(str) && legacyNames.ContainsKey(str)) ? legacyNames[str] : null;
    }

    public List<ThemeColor> GetTextColors()
    {
        if (textColors is null)
        {
            textColors = new List<ThemeColor>();
            // we generate it. we just use all colors that have "text" in the name lol
            foreach (ThemeColor themeColor in Enum.GetValues(typeof(ThemeColor)))
            {
                if (themeColor.ToString().ToLowerInvariant().Contains("text"))
                {
                    textColors.Add(themeColor);
                }
            }
        }

        return textColors;
    }

    private void Update()
    {
        if (isInEditMode && Input.GetMouseButtonDown(0) && IsMenuClosed() && !IsInsideButtons())
        {
            OpenSelection();
        }
        
        if (currentTheme != null)
        {
            currentTheme.UpdateGifs();
        }
    }

    private bool IsMenuClosed()
    {
        return !themeFullPanel.gameObject.activeSelf && !themeSelectionPanel.gameObject.activeSelf;
    }

    private bool IsInsideButtons()
    {
        Vector2 pos = CameraController._instanceOverlay.GetWorldPosition(Input.mousePosition);
        return StaticUtils.IsInsideTransform(pencilButton, pos, offset) ||
               StaticUtils.IsInsideTransform(handButton, pos, offset) ||
               StaticUtils.IsInsideTransform(openEditorButton, pos, offset);
    }

    /*private void ReinstallDefaultTheme()
    {
        if (!SaveManager.IsThemeInDisk(defaultThemeFileName))
        {
            SaveManager.SaveDefaultTheme(defaultTheme);
        }
    }*/

    public void LoadCurrentTheme(bool reloadAudio = true)
    {
        // we load the theme whose name is saved on the settings
        Theme theme = GetOrLoadTheme(PREFS.CurrentTheme.GetString());
        if (theme != null)
        {
            SetNewTheme(theme, reloadAudio);
        }
        else
        {
            // if we don't find it, we load the default theme
            LoadDefaultTheme();
        }
    }

    private void LoadDefaultTheme()
    {
        Debug.Log("Loading default file as fallback!");
        //ReinstallDefaultTheme();
        PREFS.UseThemeWallpaper.SetBool(false);
        SetNewTheme(defaultTheme);
    }

    public void SetNewTheme(Theme theme, bool setAsDefault = true, bool reloadAudio = true, bool unloadPrevious = true)
    {
        StartCoroutine(_SetNewTheme(theme, setAsDefault, reloadAudio, unloadPrevious));
    }

    private IEnumerator _SetNewTheme(Theme theme, bool setAsDefault, bool reloadAudio, bool unloadPrevious)
    {
        if (theme is not null)
        {
            Theme previous = currentTheme;

            // now we replace it
            theme.UpdateMaps();
            currentTheme = theme;

            yield return new WaitUntil(currentTheme.FinishedLoading);
            
            // and we change the elements already loaded
            TriggerThemeReload();
            if (reloadAudio)
            {
                AudioController.RestartBackgroundAudio();
            }
            WallpaperController._instance.ResetBackgrounds();

            // finally we clear up the resources
            Resources.UnloadUnusedAssets();
            if (unloadPrevious && previous is not null)
            {
                previous.Unload();
            }
            
            // and we save it to the playerprefs if needed
            if (setAsDefault)
            {
                PREFS.CurrentTheme.SetString(currentTheme.GetImportedFileName());
            }
        }
    }

    [Button]
    public void TriggerThemeReload()
    {
        TextController._instance.RefreshThemeDictionary();
        foreach (ThemedElement themedElement in FindObjectsOfType(typeof(ThemedElement), true))
        {
            themedElement.UpdateElement();
        }
        ShopController._instance.UpdateRotateCircle();
        onThemeReload?.Invoke();
    }

    public void TriggerAudioReload(Theme theme)
    {
        if (theme is null)
        {
            return;
        }
        
        foreach (ThemeAudio themeAudio in theme.replacementAudio)
        {
            themeAudio.Reload();
        }
    }

    public bool HasLoadedTheme()
    {
        return currentTheme != null && currentTheme.FinishedLoading();
    }

    public Theme GetOrLoadTheme(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return null;
        }

        Debug.Log("Trying to get or load theme " + fileName);
        // we return it if it's our theme
        if (currentTheme is not null && currentTheme.importedFileName.Equals(fileName))
        {
            Debug.Log("Found it as our theme! " + fileName);
            return currentTheme;
        }

        // we return it if it's the default one
        if (defaultTheme is not null && fileName.Equals(defaultTheme.importedFileName))
        {
            Debug.Log("Found it as the default theme! " + fileName);
            return defaultTheme;
        }

        // if not found, we try to load it from disk
        Debug.Log("Loading " + fileName + " from disk.");
        Theme theme = SaveManager.LoadTheme(fileName);
        return theme;
    }

    public Theme GetCurrentTheme()
    {
        return currentTheme;
    }

    public static Color GetDefaultColor(ThemeColor themeColor)
    {
        return _instance.defaultTheme.GetColor(themeColor);
    }

    public static Color GetColor(ThemeColor themeColor)
    {
        return _instance.currentTheme.GetColor(themeColor);
    }
    
    public static ThemeTransform GetThemeTransform(ThemeOffsets category)
    {
        return _instance.currentTheme.GetThemeTransform(category);
    }
    
    public static ThemeText GetThemeText(SystemLanguage language)
    {
        return _instance.currentTheme.GetThemeText(language);
    }
    
    public static ThemeTexture GetThemeTexture(string name)
    {
        return _instance.currentTheme.GetThemeTexture(name);
    }
    
    public static ThemeFont GetThemeFont(ThemeColor place)
    {
        return _instance.currentTheme.GetThemeFont(place);
    }
    
    public static ThemeFont GetThemeFont(string fontName)
    {
        return _instance.currentTheme.GetThemeFont(fontName);
    }

    public static Sprite GetTexture(Texture texture)
    {
        return _instance.currentTheme.GetTexture(texture);
    }
    
    public static Sprite GetTexture(string name)
    {
        return _instance.currentTheme.GetTexture(name);
    }

    public static AudioClip GetAudio(AudioClip audio)
    {
        return _instance.currentTheme.GetSound(audio);
    }
    
    public static TMP_FontAsset GetOriginalFont(string name)
    {
        foreach (TMP_FontAsset font in _instance.allFonts)
        {
            if (font.name.Equals(name))
            {
                return font;
            }
        }

        return null;
    }

    public bool IsOpen()
    {
        return themeOverlay.gameObject.activeSelf || themeGeneralObject.gameObject.activeSelf;
    }

    public bool IsPanelOpen()
    {
        return themeFullPanel.gameObject.activeSelf || themeSelectionPanel.gameObject.activeSelf;
    }

    [Button(ButtonSizes.Large)]
    public void LaunchCurrentEditor()
    {
        LaunchEditor(currentTheme);
    }

    public void LaunchEditor(Theme theme)
    {
        if (AppController.IsTransitioning())
        {
            return;
        }

        StartCoroutine(_LaunchEditor(theme));
    }

    private IEnumerator _LaunchEditor(Theme theme)
    {
        // first we go to the main menu
        SettingsController._instance.ExitSettings();
        yield return new WaitForSeconds(1.5f * FadeController.GetSpeedMultiplier());

        // now we change to the new theme, saving the previous one
        if (!theme.Equals(currentTheme))
        {
            previousTheme = currentTheme;
            SetNewTheme(theme, false, true, false);
        }
        else
        {
            previousTheme = null;
        }

        UpdateOverlayState();
        UpdateNameField();
        UpdateWallpaperName();
        WallpaperController._instance.ResetBackgrounds();

        // now we open the editor
        OpenEditor();
    }

    public void UpdateWallpaperName()
    {
        string wall = "None";
        if (currentTheme.wallpaper != null)
        {
            wall = currentTheme.wallpaper.imageFileName;
        }

        wallpaperName.text = wall;
    }

    public void ResetWallpaper()
    {
        PopupController.ShowPopup("popup.deletethemewallpaper", () =>
        {
            currentTheme.DeleteWallpaper();
            WallpaperController._instance.ResetBackgrounds();
            wallpaperName.text = "None";
            PopupController.ClosePopup();
        }, PopupController.ClosePopup);
    }

    private void UpdateNameField()
    {
        nameField.text = currentTheme.LatestName;
        nameField.onValueChanged.RemoveAllListeners();
        nameField.onValueChanged.AddListener(text => currentTheme.requestedNewName = text);
    }

    private void OpenEditor()
    {
        // first we open the editor
        if (currentTween is not null)
        {
            return;
        }

        StartCoroutine(_OpenEditor());
    }

    private IEnumerator _OpenEditor()
    {
        SaveAudioMute();
        PopupController.CloseBlocker();
        themeGeneralObject.SetActive(true);
        themeOverlay.gameObject.SetActive(false);
        themeOverlay.alpha = 0;
        currentTween?.Kill();
        themeFullPanel.gameObject.SetActive(true);
        currentTween = themeFullPanel.DOFade(1f, 0.15f);
        yield return new WaitForSeconds(0.15f);
        currentTween = null;
        themeFullPanel.alpha = 1f;
        themeSelectionPanel.alpha = 1f;
    }

    private void SaveAudioMute()
    {
        wasAudioMuted = AudioController._instance.backgroundAudio.mute ||
                        AudioController._instance.backgroundAudio.volume < 0.01f;
    }

    public List<OnlineInfo> GetAllOnlineInfos()
    {
        List<OnlineInfo> infos = new List<OnlineInfo>();
        foreach (string path in Directory.GetFiles(SaveManager.SAVE_FOLDER_THEMES, "*.zip"))
        {
            OnlineInfo info = SaveManager.GenerateOnlineInfo(path);
            if (info != null)
            {
                infos.Add(info);
            }
        }

        return infos;
    }

    public OnlineInfo GetOnlineInfo(string fileName)
    {
        return SaveManager.GenerateOnlineInfo(SaveManager.SAVE_FOLDER_THEMES + fileName);
    }

    public void OnClickedCloseEditor()
    {
        CloseEditor();
    }

    public void CloseEditor(bool showOverlay = true)
    {
        if (currentTween is not null)
        {
            return;
        }

        StartCoroutine(_CloseEditor(showOverlay));
        TriggerThemeReload();
    }

    private IEnumerator _CloseEditor(bool showOverlay = true)
    {
        currentTween?.Kill();
        CanvasGroup target = themeFullPanel.gameObject.activeSelf ? themeFullPanel : themeSelectionPanel;
        currentTween = target.DOFade(0f, 0.15f);
        if (isInEditMode)
        {
            PopupController.ShowBlocker();
        }

        yield return new WaitForSeconds(0.15f);
        currentTween = null;
        themeFullPanel.gameObject.SetActive(false);
        themeFullPanel.alpha = 0;
        themeSelectionPanel.gameObject.SetActive(false);
        themeSelectionPanel.alpha = 0;
        selectionBackObject.SetActive(false);

        if (showOverlay)
        {
            themeOverlay.gameObject.SetActive(true);
            currentOverlayTween?.Kill();
            currentOverlayTween = themeOverlay.DOFade(0.85f, 0.1f);
            yield return new WaitForSeconds(0.1f);
            currentOverlayTween = null;
        }

        themeGeneralObject.SetActive(showOverlay);
    }

    public void GoBackSelection()
    {
        if (isViewingTexts)
        {
            SpawnTexts(cacheLanguage, cacheBaseDictionary, cacheTextCategories);
            return;
        }
        
        themeFullPanel.gameObject.SetActive(true);
        themeFullPanel.alpha = 1f;
        themeSelectionPanel.gameObject.SetActive(false);
        themeSelectionList.DestroyChildren();
        cacheBaseDictionary = null;
        cacheTextCategories = null;
        if (!wasAudioMuted)
        {
            AudioController.UnmuteBackgroundAudio();
        }
    }

    public void CloseSelection()
    {
        CloseEditor();
    }

    public void RefreshTheme(string fileName)
    {
        // this is called when a theme finishes downloading, since we might need to reset our current one
        if (fileName.Equals(currentTheme.GetImportedFileName()))
        {
            Theme theme = GetOrLoadTheme(fileName);
            SetNewTheme(theme);
        }
    }

    public string CloneTheme(Theme theme)
    {
        // We save its previous name and ID
        string name = theme.name;
        string importedName = theme.GetImportedFileName();
        long id = theme.id;
        string forkId = theme.forkId;

        // We generate new ones
        theme.requestedNewName = name + " COPY";
        theme.forkId = theme.onlineInfo?.animationId;
        theme.CreateID();
        theme.Save(true);
        string newName = theme.GetImportedFileName();
        
        // We return it to its previous values
        theme.requestedNewName = null;
        theme.name = name;
        theme.importedFileName = importedName;
        theme.id = id;
        theme.forkId = forkId;
        
        // And we return the clone's name
        return newName;
    }

    public void DeleteTheme(Theme theme, bool deleteFile = true)
    {
        if (theme is null)
        {
            return;
        }

        theme.Unload();
        
        if (theme.Equals(currentTheme))
        {
            // we apply the default theme
            LoadDefaultTheme();
        }
        
        if (theme.Equals(previousTheme))
        {
            previousTheme = null;
        }

        if (deleteFile)
        {
            // Then we want it fully gone, not just removed from the lists.
            SaveManager.DeleteTheme(theme);
        }
    }

    public void FinishTheme()
    {
        PopupController.ShowPopup("popup.confirmthemesave", () =>
        {
            bool saved = currentTheme.Save();
            if (!saved)
            {
                return;
            }
            
            if (previousTheme is null)
            {
                FinishEditorCurrentTheme();
            }
            else
            {
                PopupController.ShowPopup("popup.switchtotheme", () =>
                {
                    FinishEditorCurrentTheme();
                    PREFS.CurrentTheme.SetString(currentTheme.GetImportedFileName());
                }, FinishEditorNewTheme);
            }
        }, () =>
        {
            PopupController.ShowPopup("popup.confirmnosave", FinishEditorNewTheme, PopupController.ClosePopup);
        });
    }

    private void FinishEditorCurrentTheme()
    {
        CloseEditor(false);
        PopupController.ClosePopup();
        PREFS.UseThemeWallpaper.SetBool(true); // we assume they want to keep the wallpaper they made
        WallpaperController._instance.ResetBackgrounds();
    }

    private void FinishEditorNewTheme()
    {
        if (previousTheme is not null)
        {
            SetNewTheme(previousTheme);
            previousTheme = null;
        }
        else
        {
            // we load the current one again from disk
            Theme theme = SaveManager.LoadTheme(currentTheme.GetImportedFileName());
            SetNewTheme(theme);
        }
                
        CloseEditor(false);
        PopupController.ClosePopup();
    }

    public void OpenSelection()
    {
        themeSelectionList.DestroyChildren();
        isViewingTexts = false;

        // we check which UI elements we have hit
        Vector2 worldPos = CameraController._instance.GetWorldPosition(Input.mousePosition);
        Dictionary<ThemeCategories, List<ThemeColor>> colors = ThemeCategories.GetEmptyCategories();
        List<ThemedImage> themedImages = new List<ThemedImage>();
        List<ThemeOffsets> transforms = new List<ThemeOffsets>();
        List<ThemeColor> specialFonts = new List<ThemeColor>();
        List<TMP_FontAsset> fonts = new List<TMP_FontAsset>();
        List<string> translationIds = new List<string>();
        
        foreach (RectTransform trans in FindObjectsOfType(typeof(RectTransform), false))
        {
            if (StaticUtils.IsInsideTransform(trans, worldPos, offset))
            {
                // we're inside this element, let's see if we can tint it
                ThemedElement[] elements = trans.gameObject.GetComponents<ThemedElement>();
                if (elements.Length > 0)
                {
                    foreach (ThemedElement element in elements)
                    {
                        // we retrieve the info
                        AddColorCategories(element, colors);
                        AddTextures(element, themedImages);
                        AddTransforms(element, transforms);
                        AddFontsAndText(element, specialFonts, fonts, translationIds);
                    }
                }
            }
        }
        
        // Now we spawn the colors
        foreach (ThemeCategories category in colors.Keys)
        {
            List<ThemeColor> list = colors[category];
            if (!list.IsNullOrEmpty())
            {
                list = GetOrderedList(category, list);
                SpawnTitle(themeSelectionList, category.GetTitleId());
                foreach (ThemeColor color in list)
                {
                    SpawnColor(themeSelectionList, color);
                }
            }
        }

        // we spawn the textures
        if (!themedImages.IsNullOrEmpty())
        {
            SpawnTitle(themeSelectionList, "themes.category.texture");
            List<string> spawnedNames = new List<string>();
            foreach (ThemedImage themedImage in themedImages)
            {
                Texture texture = themedImage.GetTexture();
                string texName = themedImage.GetTextureName();
                if (texture != null && !spawnedNames.Contains(texName))
                {
                    spawnedNames.Add(texName);
                    SpawnTexture(themeSelectionList, texture, texName);
                }
            }
        }
        
        // then the general fonts
        if (!fonts.IsNullOrEmpty())
        {
            SpawnTitle(themeSelectionList, "themes.category.fontsall");
            foreach (TMP_FontAsset asset in fonts)
            {
                SpawnFont(themeSelectionList, asset, ThemeColor.HomeBackground);
            }
        }
        
        // then the special fonts
        if (!specialFonts.IsNullOrEmpty())
        {
            SpawnTitle(themeSelectionList, "themes.category.fontsplace");
            foreach (ThemeColor color in specialFonts)
            {
                SpawnFont(themeSelectionList, null, color);
            }
        }
        
        // then we spawn the transforms
        if (!transforms.IsNullOrEmpty())
        {
            SpawnTitle(themeSelectionList, "themes.category.transforms");
            foreach (ThemeOffsets offsets in transforms)
            {
                SpawnTransform(themeSelectionList, offsets);
            }
        }
        
        // finally we spawn the texts
        if (!translationIds.IsNullOrEmpty())
        {
            ThemeText text = GetOrCreateThemeText(TextController._instance.chosenLanguage);
            SpawnTitle(themeSelectionList, "themes.category.texts");
            foreach (string id in translationIds)
            {
                SpawnLanguageText(themeSelectionList, text, id, TextController.GetTranslation(id, false));
            }
        }
        
        ShowSelectionMenu(true);
    }

    private void ShowSelectionMenu(bool resetScrollbar)
    {
        // We open the menu
        SaveAudioMute();
        themeOverlay.gameObject.SetActive(false);
        themeOverlay.alpha = 0;
        PopupController.CloseBlocker();
        ChangeToSelection(resetScrollbar);
        themeSelectionPanel.DOFade(1f, 0.075f);
    }

    private void ChangeToSelection(bool resetScrollbar = true)
    {
        selectionBackObject.SetActive(true);
        themeFullPanel.gameObject.SetActive(false);
        themeSelectionPanel.gameObject.SetActive(true);
        if (resetScrollbar)
        {
            themeSelectionPanelScrollbar.SetViewAtTop();
        }
    }

    private void AddColorCategories(ThemedElement element, Dictionary<ThemeCategories, List<ThemeColor>> dict)
    {
        foreach (ThemeColor color in element.GetAllColors())
        {
            List<ThemeColor> list = dict[ThemeCategories.GetCategory(color)];
            if (!list.Contains(color))
            {
                list.Add(color);
            }
        }
    }
    
    private void AddFontsAndText(ThemedElement element, List<ThemeColor> specialFonts, List<TMP_FontAsset> fonts, List<string> translationIds)
    {
        if (element is ThemedText text)
        {
            if (!specialFonts.Contains(text.themeColor))
            {
                specialFonts.Add(text.themeColor);
            }
            if (!fonts.Contains(text.originalFont))
            {
                fonts.Add(text.originalFont);
            }

            TranslatedText trans = element.GetComponent<TranslatedText>();
            if (trans != null)
            {
                translationIds.Add(trans.id);
            }
        }
    }
    
    private void AddTransforms(ThemedElement element, List<ThemeOffsets> list)
    {
        if (element is ThemedTransform trans)
        {
            ThemeOffsets off = trans.category;
            if (!list.Contains(off) && !skipOffsets.Contains(off))
            {
                list.Add(off);
            }
        }
    }

    public void UpdateAllTransforms()
    {
        foreach (ThemedTransform trans in FindObjectsOfType(typeof(ThemedTransform), true))
        {
            trans.UpdateElement();
        }
    }

    private List<ThemeColor> GetOrderedList(ThemeCategories category, List<ThemeColor> list)
    {
        List<ThemeColor> orderedList = new List<ThemeColor>();
        foreach (ThemeColor catColor in category.GetColors())
        {
            if (list.Contains(catColor))
            {
                orderedList.Add(catColor);
            }
        }
        return orderedList;
    }

    private void AddTextures(ThemedElement element, List<ThemedImage> textures)
    {
        if (element is ThemedImage themedImage && !(element is ThemedWallpaper)) // we skip wallpapers since they have a separate logic
        {
            textures.Add(themedImage);
        }
    }

    public void OnClickedColors()
    {
        themeSelectionList.DestroyChildren();
        foreach (ThemeCategories category in ThemeCategories.GetCategories())
        {
            SpawnTitle(themeSelectionList, category.GetTitleId());
            foreach (ThemeColor color in category.GetColors())
            {
                SpawnColor(themeSelectionList, color);
            }
        }
        ChangeToSelection();
    }

    public void OnClickedTextures()
    {
        themeSelectionList.DestroyChildren();
        List<string> spawnedTextures = new List<string>();
        SpawnTitle(themeSelectionList, "themes.category.texture");
        
        // first we save the special textures
        foreach (Texture texture in specialTextures)
        {
            spawnedTextures.Add(texture.name);
        }
        
        // now the ones currently available
        foreach (ThemedImage image in FindObjectsOfType(typeof(ThemedImage), true))
        {
            Texture texture = image.GetTexture();
            string texName = image.GetTextureName();
            if (texture != null && !string.IsNullOrEmpty(texName) && !spawnedTextures.Contains(texName) && image is not ThemedWallpaper)
            {
                spawnedTextures.Add(texName);
                SpawnTexture(themeSelectionList, texture, texName);
            }
        }

        // finally we actually spawn the special textures
        foreach (Texture texture in specialTextures)
        {
            Sprite foundTexture = GetTexture(texture);
            Texture finalTex = foundTexture is null ? texture : foundTexture.texture;
            SpawnTexture(themeSelectionList, finalTex, finalTex.name);
        }

        ChangeToSelection();
    }
    
    public void OnClickedTransforms()
    {
        themeSelectionList.DestroyChildren();
        SpawnTitle(themeSelectionList, "themes.category.transforms");
        foreach (ThemeOffsets off in Enum.GetValues(typeof(ThemeOffsets)))
        {
            if (!skipOffsets.Contains(off))
            {
                SpawnTransform(themeSelectionList, off);
            }
        }
        
        ChangeToSelection();
    }
    
    public void OnClickedTexts()
    {
        SpawnTexts(SystemLanguage.English);
    }

    private void SpawnTexts(SystemLanguage chosenLanguage, Dictionary<string, string> baseDictionary = null, Dictionary<string, int> categories = null)
    {
        isViewingTexts = false;
        themeSelectionList.DestroyChildren();
        cacheLanguage = chosenLanguage;
        SpawnTitle(themeSelectionList, "themes.category.texts");
        
        // now we spawn the language selector
        SpawnLanguageSelector(themeSelectionList, chosenLanguage);

        // we create the theme text object if it doesn't exist
        ThemeText text = GetOrCreateThemeText(chosenLanguage);
        
        // and finally we spawn the text categories
        cacheBaseDictionary = baseDictionary ?? TextController._instance.GetAllTranslations(chosenLanguage);
        cacheTextCategories = categories ?? GetTextCategories(cacheBaseDictionary);
        foreach (string cat in cacheTextCategories.Keys)
        {
            GameObject prefab = Instantiate(themeTextCategoryPrefab, themeSelectionList);
            prefab.transform.Find("TitleHolder").Find("Title").GetComponent<TextMeshProUGUI>().text = cat;
            prefab.transform.Find("NameAndChange").Find("Amount").GetComponent<TextMeshProUGUI>().text = "Nº: " + cacheTextCategories[cat];

            Button changeButton = prefab.transform.Find("NameAndChange").Find("Button").GetComponent<Button>();
            changeButton.onClick.AddListener(() =>
            {
                LoadTextCategory(text, cat, cacheBaseDictionary);
            });
        }

        ChangeToSelection();
    }

    private ThemeText GetOrCreateThemeText(SystemLanguage chosenLanguage)
    {
        ThemeText text = GetThemeText(chosenLanguage);
        if (text == null)
        {
            text = new ThemeText(chosenLanguage);
            currentTheme.ReplaceText(text);
        }

        return text;
    }

    private Dictionary<string, int> GetTextCategories(Dictionary<string, string> dict)
    {
        Dictionary<string, int> categories = new Dictionary<string, int>();
        foreach (string key in dict.Keys)
        {
            string prefix = key.Split(".")[0];
            if (!categories.ContainsKey(prefix))
            {
                categories[prefix] = 1;
            }
            else
            {
                categories[prefix] += 1;
            }
        }
        return categories;
    }

    private void LoadTextCategory(ThemeText text, string prefix, Dictionary<string, string> baseDictionary)
    {
        themeSelectionList.DestroyChildren();
        isViewingTexts = true;
        SpawnTitle(themeSelectionList, "themes.category.texts");
        foreach (string id in baseDictionary.Keys)
        {
            if (id.StartsWith(prefix))
            {
                SpawnLanguageText(themeSelectionList, text, id, baseDictionary[id], baseDictionary);
            }
        }
    }
    
    public void OnClickedFonts()
    {
        themeSelectionList.DestroyChildren();

        // first we spawn all the regular fonts
        SpawnTitle(themeSelectionList, "themes.category.fontsall");
        foreach (TMP_FontAsset font in allFonts)
        {
            SpawnFont(themeSelectionList, font, ThemeColor.HomeBar); // we don't care about theme color here, it's fine whatever it is
        }
        
        // and then we spawn the per-place fonts
        SpawnTitle(themeSelectionList, "themes.category.fontsplace");
        foreach (ThemeColor font in GetTextColors())
        {
            SpawnFont(themeSelectionList, null, font);
        }

        ChangeToSelection();
    }

    public void OnClickedSimulateWarning()
    {
        WarningController._instance.Show();
    }

    public void OnClickedAudio()
    {
        themeSelectionList.DestroyChildren();
        AudioController.MuteBackgroundAudio();
        SpawnTitle(themeSelectionList, "themes.category.audio");
        foreach (AudioClip audioClip in AudioLibrary.GetAllAudioClips())
        {
            AudioClip replacement = GetAudio(audioClip) ?? audioClip;
            SpawnAudio(themeSelectionList, audioClip, replacement);
        }
        ChangeToSelection();
    }

    public void OnClickedPencil()
    {
        isInEditMode = !isInEditMode;
        UpdateOverlayState();
    }

    private void UpdateOverlayState()
    {
        pencilButton.gameObject.SetActive(isInEditMode);
        handButton.gameObject.SetActive(!isInEditMode);

        if (pencilButton.gameObject.activeSelf)
        {
            PopupController.ShowBlocker();
        }
        else
        {
            PopupController.CloseBlocker();
        }
    }

    public void OnClickedOpenEditor()
    {
        if (themeSelectionList.childCount > 0)
        {
            ShowSelectionMenu(false);
        }
        else
        {
            OpenEditor();
        }
    }

    public void OnClickedChangeWallpaper()
    {
        WallpaperController._instance.RequestNewWallpaper(currentTheme);
    }

    private void SpawnColor(RectTransform parent, ThemeColor themeColor)
    {
        Option colorOption = Option.Create(AddSpacesToSentence(themeColor.ToString()), () => currentTheme.colorMap[themeColor], x => currentTheme.colorMap[themeColor] = x);
        GameObject prefab = OptionsSpawner.SetupOption(colorOption, parent, themeColorPrefab);

        GameObject colorPrefab = prefab.transform.Find("ColorHolder").Find("Color").gameObject;
        UICornerCut cornerCut = colorPrefab.GetComponent<UICornerCut>();
        colorPrefab.GetComponent<LongClickButton>().onLongClick.AddListener(() =>
        {
            PopupController.ShowPopup("popup.confirmcolorreset", () =>
            {
                ResetColor(themeColor, cornerCut);
            }, PopupController.ClosePopup);
        });
    }

    private void ResetColor(ThemeColor themeColor, UICornerCut cornerCut)
    {
        Color defaultColor = GetDefaultColor(themeColor);
        currentTheme.colorMap[themeColor] = defaultColor;
        TriggerThemeReload();
        cornerCut.color = defaultColor;
        PopupController.ClosePopup();
    }
    
    private string AddSpacesToSentence(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return "";
        }
        
        StringBuilder newText = new StringBuilder(text.Length * 2);
        newText.Append(text[0]);
        for (int i = 1; i < text.Length; i++)
        {
            if (char.IsUpper(text[i]) && text[i - 1] != ' ')
            {
                newText.Append(' ');
            }
            newText.Append(text[i]);
        }
        return newText.ToString();
    }
    
    private void SpawnTransform(RectTransform parent, ThemeOffsets category)
    {
        ThemeTransform trans = currentTheme.GetThemeTransform(category);
        Option offsetOption = Option.Create(AddSpacesToSentence(category.ToString()) + ": Offset", () => trans.offset, x => trans.offset = x);
        GameObject spawned = OptionsSpawner.SetupOption(offsetOption, parent, themeTransPrefab);
        Button edit = spawned.transform.Find("TitleHolder").Find("EditPosition").GetComponent<Button>();
        edit.gameObject.SetActive(true);
        edit.onClick.AddListener(() => OpenPositionsMenu(trans));
        
        Option scaleOption = Option.Create(AddSpacesToSentence(category.ToString()) + ": Scale", () => trans.scale, x => trans.scale = x);
        OptionsSpawner.SetupOption(scaleOption, parent, themeTransPrefab);
        Option rotOption = Option.Create(AddSpacesToSentence(category.ToString()) + ": Rotation", () => trans.rotation, x => trans.rotation = x);
        OptionsSpawner.SetupOption(rotOption, parent, themeTransPrefab);
    }
    
    private void SpawnLanguageSelector(RectTransform parent, SystemLanguage currentLang)
    {
        Option langOption = Option.Create("themes.editor.currentlang", () => currentLang, x =>
        {
            // we switch the theme's lang to another one
            SpawnTexts(x);
        });
        OptionsSpawner.SetupOption(langOption, parent);
    }
    
    private void SpawnLanguageText(RectTransform parent, ThemeText text, string id, string value, Dictionary<string, string> baseDictionary = null)
    {
        Dictionary<string, string> dict = text.GetDictionary();
        Option langOption = Option.Create("·"+id, () => value, x =>
        {
            // we set the new dictionary
            dict[id] = x;
            
            // and we change it in the current theme in case it's being used
            if (TextController._instance.chosenLanguage == text.language)
            {
                TextController._instance.themeTranslations[id] = x;
            }

            if (baseDictionary != null)
            {
                baseDictionary[id] = x;
            }
        });
        GameObject prefab = OptionsSpawner.SetupOption(langOption, parent, themeTextPrefab);
        
        prefab.transform.Find("TitleHolder").Find("Change").GetComponent<Button>().onClick.AddListener(() =>
            {
                PopupController.ShowPopup("popup.confirmtextreset", () =>
                {
                    // we set this text to the old one
                    string str = TextController.GetTranslation(id, text.language);
                    prefab.transform.Find("Vector").Find("Text").GetComponent<TMP_InputField>().text = str;
                    
                    // we remove this from the text controller and from the theme text
                    TextController._instance.themeTranslations.Remove(id);
                    dict.Remove(id);

                    // and we reload the theme
                    TriggerThemeReload();
                    PopupController.ClosePopup();
                }, PopupController.ClosePopup);
            });
    }

    private void OpenPositionsMenu(ThemeTransform trans)
    {
        // we update all the buttons
        themePositionOverlay.SetActive(true);
        Transform buttons = themePositionOverlay.transform.Find("Elements").Find("Buttons");
        SetupPositionsButton(buttons.Find("Row1").Find("TopLeft"), trans, ThemePositions.TopLeft);
        SetupPositionsButton(buttons.Find("Row1").Find("TopMid"), trans, ThemePositions.TopMid);
        SetupPositionsButton(buttons.Find("Row1").Find("TopRight"), trans, ThemePositions.TopRight);
        SetupPositionsButton(buttons.Find("Row2").Find("MidLeft"), trans, ThemePositions.MidLeft);
        SetupPositionsButton(buttons.Find("Row2").Find("Neutral"), trans, ThemePositions.Neutral);
        SetupPositionsButton(buttons.Find("Row2").Find("MidRight"), trans, ThemePositions.MidRight);
        SetupPositionsButton(buttons.Find("Row3").Find("BottomLeft"), trans, ThemePositions.BottomLeft);
        SetupPositionsButton(buttons.Find("Row3").Find("BottomMid"), trans, ThemePositions.BottomMid);
        SetupPositionsButton(buttons.Find("Row3").Find("BottomRight"), trans, ThemePositions.BottomRight);
    }

    private void SetupPositionsButton(Transform but, ThemeTransform trans, ThemePositions positions)
    {
        Button button = but.GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            trans.position = positions;
            themePositionOverlay.gameObject.SetActive(false);
        });
    }

    private void SpawnVoid(float height)
    {
        GameObject spawned = new GameObject("Void_" + height, typeof(RectTransform));
        spawned.transform.SetParent(themeSelectionList);
        spawned.GetComponent<RectTransform>().sizeDelta = new Vector2(100, height);
    }
    
    private void SpawnFont(RectTransform parent, TMP_FontAsset originalFont, ThemeColor specialFont)
    {
        GameObject prefab = Instantiate(themeFontPrefab, parent);
        TextMeshProUGUI title = prefab.transform.Find("TitleHolder").Find("Title").GetComponent<TextMeshProUGUI>();
        title.text = originalFont != null ? originalFont.name : AddSpacesToSentence(specialFont.ToString());

        TextMeshProUGUI status = prefab.transform.Find("NameAndChange").Find("Status").GetComponent<TextMeshProUGUI>();
        SetFontStatus(title, status, originalFont, specialFont);

        Button changeButton = prefab.transform.Find("NameAndChange").Find("Button").gameObject.GetComponent<Button>();
        changeButton.onClick.AddListener(() => RequestNewFont(title, status, originalFont, specialFont));
        changeButton.gameObject.GetComponent<LongClickButton>().onLongClick.AddListener(() =>
        {
            PopupController.ShowPopup("popup.confirmfontreset", () =>
            {
                if (originalFont is null)
                {
                    currentTheme.DeleteFont(specialFont);
                }
                else
                {
                    currentTheme.DeleteFont(originalFont.name);
                }
                TriggerThemeReload();
                SetFontStatus(title, status, originalFont, specialFont);
                PopupController.ClosePopup();
            }, PopupController.ClosePopup);
        });
    }

    private void SetFontStatus(TextMeshProUGUI title, TextMeshProUGUI status, TMP_FontAsset originalFont, ThemeColor specialFont)
    {
        if (originalFont is null)
        {
            // it's an override. we find if there's one, or exit otherwise
            ThemeFont font = GetThemeFont(specialFont);
            status.text = TextController.GetTranslation(font is null ? "themes.editor.none" : font.font.name);
            if (font != null)
            {
                title.font = font.font;
                status.font = font.font;
            }
        }
        else
        {
            // it's a general font
            ThemeFont font = GetThemeFont(originalFont.name);
            TMP_FontAsset finalFont = font is null ? originalFont : font.font;
            status.text = TextController.GetTranslation(font is null ? "themes.editor.default" : font.font.name);
            title.font = originalFont;
            status.font = finalFont;
        }
    }

    private void SpawnTexture(RectTransform parent, Texture texture, string texName)
    {
        GameObject prefab = Instantiate(themeTexturePrefab, parent);
        
        TextMeshProUGUI dim = prefab.transform.Find("NameAndChange").Find("Dimensions").GetComponent<TextMeshProUGUI>();
        SetDimensions(dim, texture);
        
        RawImage rawImage = prefab.transform.Find("Image").gameObject.GetComponent<RawImage>();
        rawImage.texture = texture;
        prefab.transform.Find("NameAndChange").Find("Name").GetComponent<TextMeshProUGUI>().text = texName;

        Button changeButton = prefab.transform.Find("NameAndChange").Find("Buttons").Find("Change").gameObject.GetComponent<Button>();
        changeButton.onClick.AddListener(() => RequestNewImage(texName, texture, prefab, dim));
        changeButton.gameObject.GetComponent<LongClickButton>().onLongClick.AddListener(() =>
        {
            PopupController.ShowPopup("popup.confirmtexturereset", () =>
            {
                currentTheme.DeleteTexture(texName);
                TriggerThemeReload();
                Texture original = FindOriginalTexture(texName);
                rawImage.texture = original;
                SetDimensions(dim, original);
                PopupController.ClosePopup();
            }, PopupController.ClosePopup);
        });

        // now we add a way to export the texture
        Button exportButton = prefab.transform.Find("NameAndChange").Find("Buttons").Find("Export").gameObject.GetComponent<Button>();
        if (texture is Texture2D tex)
        {
            exportButton.onClick.AddListener(() =>
            {
                Sprite sprite = GetTexture(texName);
                Texture finalTex = sprite is null ? FindOriginalTexture(texName) : sprite.texture;
                if (finalTex is Texture2D newTex)
                {
                    RequestExport(newTex, texName);
                }
            });
        }
        else
        {
            exportButton.gameObject.SetActive(false);
        }
    }

    private void RequestExport(Texture2D texture, string texName)
    {
        string tempPath = SaveManager.TEMP_TEXTURES + texName + ".png";
        Texture2D finalTex = texture.DuplicateTexture();
        File.WriteAllBytes(tempPath,finalTex.EncodeToPNG());
        Destroy(finalTex);
        
        FileManager.ExportFile("Save texture", FILETYPE.IMAGES_ONLY, tempPath, texName + ".png", done =>
        {
            if (done)
            {
                PopupController.ShowPopup("popup.exportedtexture", replacementArray: new [] { texName });
            }
        });
        
        /*if (Application.isMobilePlatform)
        {
            NativeFilePicker.ExportFile(tempPath, success =>
            {
                if (!success)
                {
                    Debug.Log( "Operation cancelled" );
                }
                else
                {
                    PopupController.ShowPopup("popup.exportedtexture", replacementArray: new [] { texName });
                }
            });
        }
        else
        {
            var extensionList = new [] {
                new ExtensionFilter("Texture", "png")
            };
            string fileName = texName;
            var path = FileManager.ExportFilesPc("Save texture", "", fileName, extensionList);
            if (!string.IsNullOrEmpty(path))
            {
                File.Copy(tempPath,path,true);
                PopupController.ShowPopup("popup.exportedtexture", replacementArray: new [] { texName });
            }
        }*/
    }

    private void SetDimensions(TextMeshProUGUI dim, Texture texture)
    {
        dim.text = texture.width + " x " + texture.height;
    }

    private Texture FindOriginalTexture(string texName)
    {
        // first we try to find it within the special textures
        foreach (Texture texture in specialTextures)
        {
            if (texture.name.Equals(texName))
            {
                return texture;
            }
        }
        
        // if not found, then we go for the themes
        foreach (ThemedImage image in FindObjectsOfType(typeof(ThemedImage), true))
        {
            string imageName = image.GetTextureName();
            if (!string.IsNullOrEmpty(imageName) && imageName.Equals(texName))
            {
                return image.GetOriginalTexture();
            }
        }
        return null;
    }
    
    private void RequestNewImage(string texName, Texture originalTexture, GameObject settingsIcon, TextMeshProUGUI dimensions)
    {
        FileManager.RequestFile("Choose a new texture",FILETYPE.IMAGES_WITH_GIF, path => SetImage(settingsIcon, originalTexture, texName, path, dimensions));
    }

    private void SetImage(GameObject settingsIcon, Texture originalTexture, string texName, string path, TextMeshProUGUI dimensions)
    {
        if (!string.IsNullOrEmpty(path))
        {
            StartCoroutine(_SetImage(settingsIcon, originalTexture, texName, path, dimensions));
        }
    }

    private IEnumerator _SetImage(GameObject settingsIcon, Texture originalTexture, string texName, string path, TextMeshProUGUI dimensions)
    {
        bool isGif = Path.GetExtension(path).Equals(".gif");
        if (!isGif)
        {
            /*
            // We load the image data
            byte[] rawData = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2); // Empty texture
            tex.LoadImage(rawData, true);
            tex.name = originalTexture.name;*/

            // Then we apply it to the image
            currentTheme.ReplaceTexture(texName, path);
        }
        else
        {
            currentTheme.ReplaceTexture(texName, path, isGif: true);
        }

        // We wait until the theme is over
        yield return new WaitUntil(currentTheme.FinishedLoading);
        
        // We update the icon
        settingsIcon.transform.Find("Image").gameObject.GetComponent<RawImage>().texture = GetTexture(texName).texture;
        
        // We set the new dimensions
        SetDimensions(dimensions, GetThemeTexture(texName).texture.texture);
        
        // Finally we reload the theme
        TriggerThemeReload();
    }

    private void SpawnTitle(RectTransform parent, string titleId)
    {
        SpawnVoid(50f);
        GameObject prefab = Instantiate(themeTitlePrefab, parent);
        prefab.transform.Find("Title").GetComponent<TextMeshProUGUI>().text = TextController.GetTranslation(titleId);
    }
    
    private void SpawnAudio(RectTransform parent, AudioClip originalClip, AudioClip audioClip)
    {
        GameObject prefab = Instantiate(themeAudioPrefab, parent);
        
        TextMeshProUGUI audioLength = prefab.transform.Find("NameAndChange").Find("AudioLength").GetComponent<TextMeshProUGUI>();
        SetAudioLength(audioLength, audioClip);
        
        prefab.transform.Find("NameAndChange").Find("Button").gameObject.GetComponent<Button>().onClick.AddListener(() => RequestNewAudio(audioLength, originalClip.name));
        prefab.transform.Find("NameAndChange").Find("Name").GetComponent<TextMeshProUGUI>().text = audioClip.name;

        prefab.transform.Find("NameAndChange").Find("Button").gameObject.GetComponent<LongClickButton>().onLongClick.AddListener(() =>
        {
            PopupController.ShowPopup("popup.confirmaudioreset", () =>
            {
                currentTheme.DeleteAudio(audioClip.name);
                SetAudioLength(audioLength, originalClip);
                PopupController.ClosePopup();
            }, PopupController.ClosePopup);
        });
        
        prefab.transform.Find("PlayButton").GetComponent<Button>().onClick.AddListener(() =>
        {
            AudioClip currentClip = GetAudio(originalClip) ?? originalClip;
            if (AudioController._instance.mainAudio.isPlaying)
            {
                AudioController.StopMainAudio();
            }
            else
            {
                AudioController.PlayMainAudio(currentClip);
            }
        });
    }

    private void SetAudioLength(TextMeshProUGUI text, AudioClip audioClip)
    {
        if (audioClip is null)
        {
            return;
        }
        text.text = Math.Floor(audioClip.length * 100f) / 100f + " (s)";
    }
    
    private void RequestNewAudio(TextMeshProUGUI length, string originalName)
    {
        FileManager.RequestFile("Choose a new sound", FILETYPE.AUDIO, path =>
        {
            if (path == null)
            {
                Debug.Log( "Operation cancelled" );
            }
            else
            {
                Debug.Log($"Chosen path! {path}");
                SetAudio(path,length,originalName);
            }
        });
    }

    private void SetAudio(string path, TextMeshProUGUI length, string originalName)
    {
        StartCoroutine(_SetAudio(path, length, originalName));
    }

    private IEnumerator _SetAudio(string path, TextMeshProUGUI length, string originalName)
    {
        Debug.Log("Loaded replacement audio.");
        
        // Then we apply it to the image
        currentTheme.ReplaceAudio(path, originalName);

        // We wait until the load is over
        yield return new WaitUntil(currentTheme.FinishedLoading);
        
        // We set the new dimensions
        SetAudioLength(length, currentTheme.GetThemeAudio(originalName)?.audioClip);
    }
    
    private void RequestNewFont(TextMeshProUGUI name, TextMeshProUGUI status, TMP_FontAsset originalFont, ThemeColor specialPlace)
    {
        FileManager.RequestFile("Choose a new font", FILETYPE.FONT, path =>
        {
            if (!string.IsNullOrEmpty(path))
            {
                Debug.Log($"Chosen path! {path}");
                SetFont(path,name,status,originalFont,specialPlace);
            }
        });
        
        
        /*if (Application.isMobilePlatform)
        {
            string[] fileTypes = { NativeFilePicker.ConvertExtensionToFileType("otf"), NativeFilePicker.ConvertExtensionToFileType("ttf") };
            NativeFilePicker.PickFile( ( path ) =>
            {
                if (path == null)
                {
                    Debug.Log( "Operation cancelled" );
                }
                else
                {
                    Debug.Log($"Chosen path! {path}");
                    SetFont(path,name,status,originalFont,specialPlace);
                }
            }, fileTypes);
        }
        else
        {
            var extensions = new [] {
                new ExtensionFilter("Font Files", "otf", "ttf" )
            };
            string[] paths = FileManager.ChooseFilesPc("Choose a new font.", "", extensions, false);
            if (!paths.IsNullOrEmpty())
            {
                SetFont(paths[0],name,status,originalFont,specialPlace);
            }
        }*/
    }

    private void SetFont(string path, TextMeshProUGUI name, TextMeshProUGUI status, TMP_FontAsset originalFont, ThemeColor specialPlace)
    {
        // no need for coroutine this time, there's no async loading in fonts i think
        Debug.Log("Loading replacement font.");
        
        // Then we apply it to the image
        if (originalFont is null)
        {
            // it's an special font
            currentTheme.ReplaceSpecialFont(path, specialPlace);
        }
        else
        {
            currentTheme.ReplaceFont(path, originalFont);
        }
        SetFontStatus(name, status, originalFont, specialPlace);
    }
}
