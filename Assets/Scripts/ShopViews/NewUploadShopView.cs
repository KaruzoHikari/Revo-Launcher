using System.Collections;
using System.IO;
using Animations;
using Misc;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace ShopViews
{
    public class UploadShopView : ShopView
    {
        private string onlineId;
        private bool isTakingScreenshot;

        public ContentType contentType;

        // Items
        private Transform content;
        private TextMeshProUGUI iconAnimationTitle;
        private TextMeshProUGUI bannerAnimationTitle;
        private TextMeshProUGUI themeTitle;
        private RawImage iconAnimationThumbnail;
        private RawImage bannerAnimationThumbnail;
        private TMP_InputField animationName;
        private TMP_InputField animationDescription;
        public GameObject iconSelector;
        public GameObject bannerSelector;
        public GameObject themeSelector;
        public GameObject themeThumbnails;
        public RawImage homeThemeThumbnail;
        public RawImage settingsThemeThumbnail;

        // Channel upload variables
        public CHANNELTYPE selectionChannelType;
        public ChannelAnimation selectedIcon;
        public ChannelAnimation selectedBanner;
        public Texture2D iconThumbnailTexture;
        public string iconThumbnailPath;
        public Texture2D bannerThumbnailTexture;
        public string bannerThumbnailPath;
        
        // Theme upload variables
        public Theme selectedTheme;
        public Texture2D homeThumbnailTexture;
        public string homeThumbnailPath;
        public Texture2D settingsThumbnailTexture;
        public string settingsThumbnailPath;

        public UploadShopView(GameObject view) : base(view, () => { },
            () => ShopController._instance.animationNewUploadView.ClickedUpload())
        {
            content = view.transform.Find("Content").Find("Scroll View").Find("Viewport").Find("Content");
            iconAnimationTitle = content.Find("IconAnimation").Find("Name").Find("Title").GetComponent<TextMeshProUGUI>();
            bannerAnimationTitle = content.Find("BannerAnimation").Find("Name").Find("Title").GetComponent<TextMeshProUGUI>();
            themeTitle = content.Find("Theme").Find("Name").Find("Title").GetComponent<TextMeshProUGUI>();
            iconAnimationThumbnail = content.Find("IconAnimation").Find("Data").Find("Image").GetComponent<RawImage>();
            bannerAnimationThumbnail = content.Find("BannerAnimation").Find("Data").Find("Image").GetComponent<RawImage>();
            animationName = content.Find("Name").Find("Vector").Find("Text").GetComponent<TMP_InputField>();
            animationDescription = content.Find("Description").Find("Vector").Find("Text").GetComponent<TMP_InputField>();
            iconSelector = content.Find("IconAnimation").gameObject;
            bannerSelector = content.Find("BannerAnimation").gameObject;
            themeSelector = content.Find("Theme").gameObject;
            themeThumbnails = content.Find("ThemeThumbnails").gameObject;
            homeThemeThumbnail = content.Find("ThemeThumbnails").Find("Home").GetComponent<RawImage>();
            settingsThemeThumbnail = content.Find("ThemeThumbnails").Find("Settings").GetComponent<RawImage>();
        }

        public void Show(ContentType contentType, bool clearValues = false)
        {
            base.Show();
            this.contentType = contentType;
            
            if (clearValues)
            {
                ClearValues();
            }
            else
            {
                // We need to setup the currently chosen channel anim names and thumbnails
                iconAnimationTitle.text = selectedIcon != null ? selectedIcon.name : TextController.GetTranslation("ap.title.uploadanim");
                bannerAnimationTitle.text = selectedBanner != null ? selectedBanner.name : TextController.GetTranslation("ap.title.uploadanim");
                themeTitle.text = selectedTheme != null ? selectedTheme.name : TextController.GetTranslation("ap.title.uploadtheme");

                if (iconThumbnailPath != null)
                {
                    iconAnimationThumbnail.texture = iconThumbnailTexture;
                }
                if (bannerThumbnailPath != null)
                {
                    bannerAnimationThumbnail.texture = bannerThumbnailTexture;
                }
                if (homeThumbnailPath != null)
                {
                    homeThemeThumbnail.texture = homeThumbnailTexture;
                }
                if (settingsThumbnailPath != null)
                {
                    settingsThemeThumbnail.texture = settingsThumbnailTexture;
                }
            }
            
            iconSelector.gameObject.SetActive(IsAnimation());
            bannerSelector.gameObject.SetActive(IsAnimation());
            themeSelector.gameObject.SetActive(!IsAnimation());
            themeThumbnails.gameObject.SetActive(!IsAnimation());
        }

        private bool IsAnimation()
        {
            return contentType == ContentType.ANIMATIONS;
        }

        public void ClearValues()
        {
            if (IsAnimation())
            {
                SetupOnlineAnimation(null, "", "", null, null);
            }
            else
            {
                SetupOnlineTheme(null, "", "", null);
            }
            selectedIcon = null;
            selectedBanner = null;
            selectedTheme = null;
            iconThumbnailPath = null;
            bannerThumbnailPath = null;
            iconAnimationThumbnail.texture = _instance.tempThumbnail;
            bannerAnimationThumbnail.texture = _instance.tempThumbnail;
            homeThemeThumbnail.texture = _instance.tempThumbnail;
            settingsThemeThumbnail.texture = _instance.tempThumbnail;
            onlineId = null;
            iconSelector.gameObject.SetActive(false);
            bannerSelector.gameObject.SetActive(false);
            themeSelector.gameObject.SetActive(false);
            themeThumbnails.gameObject.SetActive(false);
        }

        public void SetupOnlineAnimation(string id, string name, string description, ChannelAnimation icon, ChannelAnimation banner)
        {
            // We setup the initial values
            onlineId = id;
            animationName.text = name;
            animationDescription.text = description;
            iconAnimationTitle.text = icon == null ? TextController.GetTranslation("ap.title.uploadanim") : icon.name;
            bannerAnimationTitle.text = banner == null ? TextController.GetTranslation("ap.title.uploadanim") : banner.name;
            selectedIcon = icon;
            selectedBanner = banner;
            
            // We download the thumbnails
            if (icon != null)
            {
                LoadOnlineThumbnail(CHANNELTYPE.ICON, false);
            }
            if (banner != null)
            {
                LoadOnlineThumbnail(CHANNELTYPE.BANNER, false);
            }
        }

        public void SetupOnlineTheme(string id, string name, string description, Theme theme)
        {
            // We setup the initial values
            onlineId = id;
            animationName.text = name;
            animationDescription.text = description;
            themeTitle.text = theme == null ? TextController.GetTranslation("ap.title.uploadtheme") : theme.name;
            selectedTheme = theme;
            
            // We download the thumbnails (we set it to ICON and BANNER cause server-side it handles us both)
            if (theme != null)
            {
                LoadOnlineThumbnail(CHANNELTYPE.ICON, true);
                LoadOnlineThumbnail(CHANNELTYPE.BANNER, true);
            }
        }

        private void LoadOnlineThumbnail(CHANNELTYPE type, bool isTheme)
        {
            RawImage target;
            if (isTheme)
            {
                target = type == CHANNELTYPE.ICON ? homeThemeThumbnail : settingsThemeThumbnail;
            }
            else
            {
                target = type == CHANNELTYPE.ICON ? iconAnimationThumbnail : bannerAnimationThumbnail;
            }
            
            string path = SaveManager.TEMP_THUMBNAIL + $"{type.ToString().ToLowerInvariant()}_{onlineId}.png";
            UnityAction<Texture2D> action = texture =>
            {
                // We save it as our local texture
                if (type == CHANNELTYPE.ICON)
                {
                    if (isTheme)
                    {
                        homeThumbnailTexture = texture;
                        homeThumbnailPath = path;
                    }
                    else
                    {
                        iconThumbnailTexture = texture;
                        iconThumbnailPath = path;
                    }
                }
                else
                {
                    if (isTheme)
                    {
                        settingsThumbnailTexture = texture;
                        settingsThumbnailPath = path;
                    }
                    else
                    {
                        bannerThumbnailTexture = texture;
                        bannerThumbnailPath = path;
                    }
                }
                
                // And we write it to the file so we can send it again
                if (!(texture is null))
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }

                    Texture2D clone = texture.DuplicateTexture();
                    File.WriteAllBytes(path,clone.EncodeToPNG());
                    GameObject.Destroy(clone);
                }
            };
            
            WebRequestController.SendDownloadThumbnail(onlineId, contentType,type,target,_instance.notFoundThumbnail,action);
        }

        public override void ClickedBack()
        {
            if (onlineId != null)
            {
                _instance.browseQueryView.Show(SearchMode.MY_CONTENT, contentType, false);
            }
            else
            {
                _instance.uploadCategoryView.Show(contentType);
            }
        }

        public void ClickedSelectAnimation(CHANNELTYPE channeltype)
        {
            selectionChannelType = channeltype;
            
            SettingsController._instance.channelUploadSettingView.SetChannelType(channeltype);
            SettingsController._instance.OpenSettings(SettingsController._instance.channelUploadSettingView);
        }
        
        public void ClickedSelectTheme()
        {
            SettingsController._instance.OpenSettings(SettingsController._instance.themeUploadSettingView);
        }

        public void OnSelectedAnimation(ChannelAnimation channelAnimation)
        {
            PopupController.ShowPopup("popup.takescreenshot",
                () => { _instance.StartCoroutine(_OnSelectedAnimation(channelAnimation)); });
        }

        private IEnumerator _OnSelectedAnimation(ChannelAnimation channelAnimation)
        {
            // We exit the settings menu
            SettingsController._instance.ExitSettings(false);
            yield return new WaitForSeconds(1f);

            // We prepare to retrieve the thumbnail from a preview channel
            if (selectionChannelType == CHANNELTYPE.ICON)
            {
                selectedIcon = channelAnimation;
            }
            else
            {
                selectedBanner = channelAnimation;
            }

            PreviewController._instance.ShowDecoy(channelAnimation, false, true, false);
        }

        public void OnSelectedTheme(Theme theme)
        {
            PopupController.ShowPopup("popup.themescreenshots",
                () => { _instance.StartCoroutine(_OnSelectedTheme(theme)); }, false);
        }
        
        private IEnumerator _OnSelectedTheme(Theme theme)
        {
            // We save the new one and the current one
            selectedTheme = theme;
            Theme usingTheme = null;
            if (selectedTheme != ThemeController._instance.GetCurrentTheme())
            {
                usingTheme = ThemeController._instance.GetCurrentTheme();
            }
            bool wasUsingWallpaper = PREFS.UseThemeWallpaper.GetBool();
            PREFS.UseThemeWallpaper.SetBool(true);
            
            // We exit the settings menu
            SettingsController._instance.ExitSettings();
            yield return new WaitForSeconds(1.25f * FadeController.GetSpeedMultiplier());
            PopupController.DestroyCurrentPopup();
            PopupController.ResetBackgroundTransparency();
            PopupController.ShowBlocker();
            InputController._instance.cursorHolder.gameObject.SetActive(false);
            AppController._instance.SetDebugConsole(false);
            _instance.shopMenu.SetActive(false);
            if (usingTheme != null)
            {
                ThemeController._instance.SetNewTheme(selectedTheme, false, false, false);
            }
            
            // We take a screenshot automatically
            yield return new WaitForSeconds(1.75f * FadeController.GetSpeedMultiplier() + 0.5f);
            yield return new WaitUntil(selectedTheme.FinishedLoading);
            yield return new WaitForEndOfFrame();
            Debug.Log("Taking screenshot of home!");
            homeThumbnailTexture = GetFullscreenScreenshot();

            // And now we go to settings and snap another one in there
            SettingsController._instance.OpenSettings(SettingsController._instance.preferencesSettingView);
            SettingsController._instance.preferencesScrollbar.value = 0;
            
            yield return new WaitForSeconds(3.5f * FadeController.GetSpeedMultiplier());
            yield return new WaitForEndOfFrame();
            Debug.Log("Taking screenshot of settings!");
            settingsThumbnailTexture = GetFullscreenScreenshot();
            
            // We save the textures
            SaveThemeThumbnail(true);
            SaveThemeThumbnail(false);

            // We show this menu again
            Show(contentType, false);
            SettingsController._instance.ExitSettings(false);
            _instance.shopMenu.SetActive(true);
            PopupController.CloseBlocker();
            
            // And we put our theme back
            yield return new WaitForSeconds(1.25f * FadeController.GetSpeedMultiplier());
            PREFS.UseThemeWallpaper.SetBool(wasUsingWallpaper);
            ThemeController._instance.SetNewTheme(usingTheme, false, false, true);
            InputController._instance.cursorHolder.gameObject.SetActive(true);
            AppController._instance.SetDebugConsole(PREFS.ShowDebugConsole.GetBool());
        }

        private Texture2D GetFullscreenScreenshot()
        {
            Texture2D tex = new Texture2D(Screen.width,Screen.height);
            tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            tex.Apply();
            SetMaxAlpha(tex);
            return tex;
        }

        private void SaveThemeThumbnail(bool isHome)
        {
            string path = SaveManager.TEMP_THUMBNAIL + Path.GetRandomFileName() + ".png";
            byte[] imageBytes = isHome ? homeThumbnailTexture.EncodeToPNG() : settingsThumbnailTexture.EncodeToPNG();
            File.WriteAllBytes(path, imageBytes);

            if (isHome)
            {
                homeThumbnailPath = path;
            }
            else
            {
                settingsThumbnailPath = path;
            }
        }

        public void ClickedThumbnailBack()
        {
            isTakingScreenshot = false;
            PreviewController._instance.HideDecoy(false);
            if (selectionChannelType == CHANNELTYPE.ICON)
            {
                selectedIcon = null;
            }
            else
            {
                selectedBanner = null;
            }
        }

        public void ClickedThumbnailScreenshot()
        {
            if (!isTakingScreenshot)
            {
                isTakingScreenshot = true;
                AnimationController._instance.Pause();
                PreviewController._instance.decoyGlobalOverlay.SetActive(false);
                PreviewController._instance.decoyWhiteBottom.SetActive(true);
                InputController._instance.cursorHolder.gameObject.SetActive(false);
                EditorController._instance.SetGridStatus(false);
                AppController._instance.SetDebugConsole(false);
                _instance.StartCoroutine(_CaptureChannelScreenshot());
            }
        }

        private IEnumerator _CaptureChannelScreenshot()
        {
            yield return new WaitForEndOfFrame();

            // We calculate the different positions
            GameObject bottomCorner = PreviewController._instance.GetBottomCorner(selectionChannelType);
            GameObject topCorner = PreviewController._instance.GetTopCorner(selectionChannelType);
            Vector2 bottomPixel = CameraController._instance.GetScreenPosition(bottomCorner.transform.position);
            Vector2 topPixel = CameraController._instance.GetScreenPosition(topCorner.transform.position);
            int height = (int) (topPixel.y - bottomPixel.y);
            int width = (int) (topPixel.x - bottomPixel.x);

            Texture2D screenImage = new Texture2D(width,height);
            screenImage.ReadPixels(new Rect(bottomPixel.x, bottomPixel.y, width, height), 0, 0);
            screenImage.Apply();
            SetMaxAlpha(screenImage);

            PreviewController._instance.decoyGlobalOverlay.SetActive(true);
            PreviewController._instance.decoyWhiteBottom.SetActive(false);
            InputController._instance.cursorHolder.gameObject.SetActive(true);
            AppController._instance.SetDebugConsole(PREFS.ShowDebugConsole.GetBool());
            PreviewController._instance.ShowThumbnail(screenImage);
        }

        private void SetMaxAlpha(Texture2D screenImage)
        {
            for (int i = 0; i < screenImage.width; i++)
            {
                for (int j = 0; j < screenImage.height; j++)
                {
                    Color pixel = screenImage.GetPixel(i, j);
                    pixel.a = 1f;
                    screenImage.SetPixel(i, j, pixel);
                }
            }

            screenImage.Apply();
        }

        public void ClickedAcceptScreenshot()
        {
            // We retrieve the temp thumbnail path
            string path = SaveManager.TEMP_THUMBNAIL + Path.GetRandomFileName() + ".png";

            // We save the thumbnail
            byte[] imageBytes = PreviewController._instance.currentThumbnail.EncodeToPNG();
            File.WriteAllBytes(path, imageBytes);

            // We reset variables and save the thumbnail path
            isTakingScreenshot = false;
            if (selectionChannelType == CHANNELTYPE.ICON)
            {
                iconThumbnailPath = path;
                iconThumbnailTexture = PreviewController._instance.currentThumbnail;
            }
            else
            {
                bannerThumbnailPath = path;
                bannerThumbnailTexture = PreviewController._instance.currentThumbnail;
            }

            PreviewController._instance.HideDecoy(false);

            // We show this menu again
            Show(contentType, false);
        }

        public void ClickedRetryScreenshot()
        {
            PreviewController._instance.HideThumbnail();
            isTakingScreenshot = false;
        }

        private bool IsValidAnimation()
        {
            if (!IsAnimation())
            {
                return true;
            }
            
            if (selectedIcon == null && selectedBanner == null)
            {
                PopupController.ShowPopup("popup.needsbanneroricon");
                return false;
            }

            string iconPath = _instance.GetAnimationFilePath(selectedIcon);
            string bannerPath = _instance.GetAnimationFilePath(selectedBanner);
            
            if (!string.IsNullOrEmpty(iconPath) && new FileInfo(iconPath).Length > 8388608)
            {
                PopupController.ShowPopup("popup.icontoobig");
                return false;
            }
            
            if (!string.IsNullOrEmpty(bannerPath) && new FileInfo(bannerPath).Length > 12582912)
            {
                PopupController.ShowPopup("popup.bannertoobig");
                return false;
            }

            return true;
        }

        private bool IsValidTheme()
        {
            if (IsAnimation())
            {
                return true;
            }
            
            string path = _instance.GetThemeFilePath(selectedTheme);
            if (selectedTheme == null || string.IsNullOrEmpty(path))
            {
                PopupController.ShowPopup("popup.needstheme");
                return false;
            }

            if (new FileInfo(path).Length > 36700160)
            {
                PopupController.ShowPopup("popup.themetoobig");
                return false;
            }

            return true;
        }

        public async void ClickedUpload()
        {
            if (!IsValidAnimation() || !IsValidTheme())
            {
                // the error message is sent by either of those methods
                return;
            }

            string name = animationName.text.Trim();
            string description = animationDescription.text.Trim();
            bool updateAnimation = onlineId != null;

            if (string.IsNullOrEmpty(name))
            {
                PopupController.ShowPopup("popup.noemptyname");
                return;
            }
            if (string.IsNullOrEmpty(description))
            {
                PopupController.ShowPopup("popup.noemptydescription");
                return;
            }

            // First we check our rank
            bool areWeVerified = await _instance.SendRequest(WebRequestController.SendGetRank()) >= UserRank.VERIFIED;
            UnityWebRequest response;
            if (IsAnimation())
            {
                string iconPath = _instance.GetAnimationFilePath(selectedIcon);
                string bannerPath = _instance.GetAnimationFilePath(selectedBanner);
                response = await _instance.SendRequest(WebRequestController.SendCreateAnimation(name, description,
                    bannerPath, bannerThumbnailPath, 
                    iconPath, iconThumbnailPath, onlineId));
            }
            else
            {
                string themePath = _instance.GetThemeFilePath(selectedTheme);
                string accentColor = ColorUtility.ToHtmlStringRGBA(selectedTheme.GetColor(ThemeColor.MainAccent));
                response = await _instance.SendRequest(WebRequestController.SendCreateTheme(name, description, accentColor,
                    themePath, homeThumbnailPath, settingsThumbnailPath, selectedTheme.HasDifferentColors(), selectedTheme.HasDifferentTextures(),
                    selectedTheme.HasDifferentAudio(), onlineId));
            }
            

            if (!response.isNetworkError && !response.isHttpError)
            {
                JObject json = response.GetJsonResponse();
                Debug.Log(json);
                string popupText = updateAnimation ? "popup.updated" : "popup.uploaded";
                popupText += IsAnimation() ? "animation" : "theme";
                popupText = TextController.GetTranslation(popupText);
                
                if (areWeVerified)
                {
                    popupText += "\n\n" + TextController.GetTranslation("popup.verifiedcreator");
                }
                else
                {
                    popupText += "\n\n" + TextController.GetTranslation("popup.unverifiedcreator");
                }
                
                PopupController.ShowPopup(popupText, () => { _instance.mainView.Show(); });
            }
            else
            {
                switch (response.responseCode)
                {
                    case 409:
                    {
                        PopupController.ShowPopup(IsAnimation() ? "popup.cantuploadanim" : "popup.cantuploadtheme");
                        break;
                    }
                    default:
                    {
                        PopupController.ShowPopup("popup.genericerror");
                        break;
                    }
                }
            }
        }
    }
}