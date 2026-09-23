using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Animations;
using Misc;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace ShopViews
{
    public class ReviewShopView : ShopView
    {
        private Transform content;
        private string animId;
        public ChannelAnimation previewIcon;
        public ChannelAnimation previewBanner;
        public ContentType contentType;
        public Theme previewTheme;
        private Theme myTheme;
        private bool wasAudioMuted;
        private bool wasUsingWallpaper;

        public ReviewShopView(GameObject view) : base(view, () => { })
        {
            content = view.transform.Find("Content").Find("Scroll View").Find("Viewport").Find("Content");
            previewIcon = new ChannelAnimation(CHANNELTYPE.ICON);
            previewIcon.MarkAsFinishedLoading();
            previewBanner = new ChannelAnimation(CHANNELTYPE.BANNER);
            previewBanner.MarkAsFinishedLoading();
        }

        public void Show(string themeId, Theme theme)
        {
            animId = themeId;
            contentType = ContentType.THEMES;
            DeletePreviousContent();
            
            // We save the audio status
            wasAudioMuted = AudioController._instance.backgroundAudio.mute || AudioController._instance.backgroundAudio.volume < 0.01f;
            AudioController.MuteBackgroundAudio();
            
            // We launch the theme and close this
            SettingsController._instance.ExitSettings(transition: false);
            _instance.shopMenu.SetActive(false);
            ThemeController._instance.themeReviewOverlay.SetActive(true);
            
            // Now we load the theme
            previewTheme = theme;
            myTheme = ThemeController._instance.GetCurrentTheme();
            wasUsingWallpaper = PREFS.UseThemeWallpaper.GetBool();
            PREFS.UseThemeWallpaper.SetBool(true);
            _instance.StartCoroutine(_Show());
        }

        private IEnumerator _Show()
        {
            ThemeController._instance.SetNewTheme(previewTheme, false, true, false);
            yield return new WaitUntil(previewTheme.FinishedLoading);
            AudioController.UnmuteBackgroundAudio();
            AudioController.PlayBackgroundMenuMusic();
        }

        private void ShowThemeElements()
        {
            base.Show();
            LoadContent(theme: previewTheme);
        }

        public void Show(string animId, ChannelAnimation banner, ChannelAnimation icon)
        {
            base.Show();
            this.animId = animId;
            contentType = ContentType.ANIMATIONS;
            DeletePreviousContent();
            
            // Then we load the animation info
            LoadContent(banner);
            LoadContent(icon);
        }
        
        private bool IsAnimation()
        {
            return contentType == ContentType.ANIMATIONS;
        }

        private void DeletePreviousContent()
        {
            // We delete all the previous animation contents, if any
            for (int i = 0; i < content.childCount-5; i++)
            {
                GameObject.Destroy(content.GetChild(i).gameObject);
            }
            
            previewTheme = null;
        }

        private void LoadContent(ChannelAnimation anim = null, Theme theme = null)
        {
            if (anim == null && theme == null)
            {
                return;
            }

            // We list all the files in the zip, and load the images if possible
            string zipPath = anim != null ? _instance.GetAnimationFilePath(anim) : _instance.GetThemeFilePath(theme);
            if (File.Exists(zipPath))
            {
                string directory = anim != null ?
                    SaveManager.TEMP_REVIEW + anim.type.ToString() + "_" + anim.onlineInfo.animationId :
                    SaveManager.TEMP_REVIEW + "theme_" + theme.onlineInfo.animationId;
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory,true);
                }
                Directory.CreateDirectory(directory);
                SaveManager.UnzipFile(zipPath,directory);

                string[] files = Directory.GetFiles(directory);
                foreach (string path in files)
                {
                    if (IsAnimation())
                    {
                        SpawnItem(anim, path);
                    }
                    else
                    {
                        SpawnThemeItem(theme, path);
                    }
                }
            }

            if (IsAnimation())
            {
                GameObject separator = GameObject.Instantiate(anim.type == CHANNELTYPE.ICON ? _instance.reviewIconSeparator : _instance.reviewBannerSeparator, content);
                separator.transform.SetAsFirstSibling();
            }
        }

        private void SpawnItem(ChannelAnimation animation, string path)
        {
            GameObject item = GameObject.Instantiate(_instance.reviewItemPrefab,content);
            item.transform.SetAsFirstSibling();
            Texture2D texture = null;

            string lowerPath = path.ToLowerInvariant();
            bool isImageOrVideo = false;
            string[] validImage = {".jpg", ".png", ".jpeg", ".webm", ".gif", ".mp4"};
            foreach (string ext in validImage)
            {
                if (lowerPath.EndsWith(ext))
                {
                    isImageOrVideo = true;
                    break;
                }
            }
            
            string fileName = FileManager.GetFileName(path);
            if (isImageOrVideo)
            {
                // We retrieve it from the channel
                AnimatedImage image = null;
                foreach (AnimatedImage im in animation.images)
                {
                    if (im.GetFileName() != null && im.GetFileName().Equals(fileName))
                    {
                        image = im;
                    }
                }

                if (image != null)
                {
                    item.transform.Find("Image").GetComponent<Button>().onClick.AddListener(() => OnClickImage(image));
                    LongClickButton button = item.transform.Find("Image").GetComponent<LongClickButton>();
                    button.onLongClick.AddListener(() => { OnLongClick(image); });
                    texture = image.GetThumbnail();
                }
            }

            TextMeshProUGUI title = item.transform.Find("Title").GetComponent<TextMeshProUGUI>();
            bool isMissing = false;
            if(texture is null)
            {
                // We couldn't find the image or video at the zip. We check the animation misc files
                List<string> validAudioFiles = new List<string>(new []
                {
                    "audio.bin", "audio.wav", "audio.mp3", "audio.ogg"
                });
                List<string> validMiscFiles = new List<string>(new []
                {
                    "onlineinfo.json","animation.json"
                });

                string lower = fileName.ToLowerInvariant();
                bool invalidFile = !validMiscFiles.Contains(lower) && !validAudioFiles.Contains(lower);
                if(invalidFile)
                {
                    // Something is fishy - the file is in the zip, but not being used by the animation
                    // The zip is tampered?
                    // They should contact me to check it myself
                    title.color = Color.red;
                    title.fontStyle = FontStyles.Bold;
                    GameObject.Destroy(title.GetComponent<ThemedText>());
                    texture = ShopController._instance.warningThumbnail;
                    PopupController.ShowPopup("popup.mod.wrongzip");
                }
                else
                {
                    // Not an illegal file
                    texture = ShopController._instance.notFoundThumbnail;
                    isMissing = true;

                    if (validAudioFiles.Contains(lower) && animation.audioClip != null)
                    {
                        // We add info about the audio length
                        fileName += $"\n<color=#54e468>MAX: {Math.Floor(animation.audioClip.length * 100) / 100f}";
                    }
                }
            }

            RawImage rawImage = item.transform.Find("Image").Find("Texture").GetComponent<RawImage>();
            rawImage.texture = texture;
            if (isMissing)
            {
                rawImage.color = Color.black;
            }
            title.text = fileName;
        }

        private void SpawnThemeItem(Theme theme, string path)
        {
            string lowerPath = path.ToLowerInvariant();
            bool isImage = false;
            bool isAudio = false;
            bool isText = false;
            string[] validImage = {".jpg", ".png", ".jpeg", ".gif", ".webm" };
            string[] validAudio = {".wav", ".mp3", ".ogg"};
            List<string> validText = new List<string>();
            foreach (string lang in TextController._instance.availableLanguages.Keys)
            {
                // we add the lang files
                validText.Add(lang.ToLowerInvariant() + ".json");
            }
            foreach (string ext in validImage)
            {
                if (lowerPath.EndsWith(ext))
                {
                    isImage = true;
                    break;
                }
            }
            foreach (string ext in validAudio)
            {
                if (lowerPath.EndsWith(ext))
                {
                    isAudio = true;
                    break;
                }
            }
            foreach (string ext in validText)
            {
                if (lowerPath.EndsWith(ext))
                {
                    isText = true;
                    break;
                }
            }

            // if it's text, we spawn all the items now and ignore it
            if (isText)
            {
                Dictionary<string, string> texts = new Dictionary<string, string>();
                TextController._instance.ProcessLanguage(path, texts);
                RectTransform rect = content.GetComponent<RectTransform>();
                foreach (string id in texts.Keys)
                {
                    string value = texts[id];
                    Option langOption = Option.Create("·"+id, () => value, x =>
                    {
                        // we don't want mods to edit this, so we kinda ignore it
                    });
                    GameObject prefab = OptionsSpawner.SetupOption(langOption, rect, ThemeController._instance.themeTextPrefab);
                    prefab.transform.SetAsFirstSibling(); // honestly rn i don't remember why we set them as first siblings instead of letting them go downwards...
                }
            }
            
            // we spawn the proper prefab
            GameObject item = GameObject.Instantiate(_instance.reviewItemPrefab,content);
            item.transform.SetAsFirstSibling();
            Texture2D texture = null;
            
            string fileName = FileManager.GetFileName(path);
            if (isImage)
            {
                // We generate it
                AnimatedImage animatedImage = new AnimatedImage();
                animatedImage.filePath = path;
                bool gif = Path.GetExtension(path).ToLowerInvariant().Equals(".gif");
                if (gif)
                {
                    animatedImage.isGif = true;
                    animatedImage.image = null;
                    animatedImage.LoadGif();
                }
                else
                {
                    // We load the image data
                    byte[] rawData = File.ReadAllBytes(path);
                    texture = new Texture2D(2, 2); // Empty texture
                    texture.LoadImage(rawData, true);
                    // And then we apply it to the image
                    animatedImage.isGif = false;
                    animatedImage.SetImageManually(texture);
                }

                animatedImage.finishedLoading = true;
                animatedImage.imageName = StaticUtils.SanitizeString(FileManager.GetFileName(path));
                item.transform.Find("Image").GetComponent<Button>().onClick
                    .AddListener(() => OnClickImage(animatedImage));
                LongClickButton button = item.transform.Find("Image").GetComponent<LongClickButton>();
                button.onLongClick.AddListener(() => { OnLongClick(animatedImage); });
                texture = animatedImage.GetThumbnail();
            }

            // if it's an image, we have the prefab ready. now let's check whether it's misc
            TextMeshProUGUI title = item.transform.Find("Title").GetComponent<TextMeshProUGUI>();
            bool isMissing = false;
            if(!isAudio && texture is null)
            {
                // We couldn't find the image or video at the zip. We check the animation misc files
                List<string> validMiscFiles = new List<string>(new[]
                {
                    "onlineinfo.json", "theme.json"
                });
                List<string> validExtensions = new List<string>(new []
                {
                    ".ttf", ".otf"
                });

                string lower = fileName.ToLowerInvariant();
                bool invalidFile = !validMiscFiles.Contains(lower) && !validExtensions.Contains(Path.GetExtension(lower)) && !isText;
                if(invalidFile)
                {
                    // Something is fishy - the file is in the zip, but not being used by the animation
                    // The zip is tampered?
                    // They should contact me to check it myself
                    title.color = Color.red;
                    title.fontStyle = FontStyles.Bold;
                    GameObject.Destroy(title.GetComponent<ThemedText>());
                    texture = ShopController._instance.warningThumbnail;
                    PopupController.ShowPopup("popup.mod.wrongzip");
                }
                else
                {
                    // Not an illegal file
                    texture = ShopController._instance.notFoundThumbnail;
                    isMissing = true;
                }
            }

            if (!isAudio)
            {
                // We spawn the image + misc
                RawImage rawImage = item.transform.Find("Image").Find("Texture").GetComponent<RawImage>();
                rawImage.texture = texture;
                if (isMissing)
                {
                    rawImage.color = Color.black;
                }
            }
            else
            {
                // We spawn the audio
                AudioController.LoadAudio(path, audio =>
                {
                    item.transform.Find("Image").gameObject.SetActive(false);
                    item.transform.Find("PlayHolder").gameObject.SetActive(true);
                    Button button = item.transform.Find("PlayHolder").Find("PlayButton").GetComponent<Button>();
                    button.onClick.AddListener(() =>
                    {
                        bool wasPlaying = AudioController._instance.mainAudio.isPlaying;
                        AudioController.StopMainAudio();
                        if (!wasPlaying || !audio.Equals(AudioController._instance.mainAudio.clip))
                        {
                            AudioController.PlayMainAudio(audio);
                        }
                    });

                    // We add info about the audio length
                    fileName += $"\n<color=#54e468>MAX: {Math.Floor(audio.length * 100) / 100f}";
                    title.text = fileName;
                });
            }

            title.text = fileName;
        }

        private void OnClickImage(AnimatedImage image)
        {
            ChannelAnimation anim = previewBanner;
            if (IsAnimation())
            {
                anim = image.channelAnimation.type == CHANNELTYPE.ICON ? previewIcon : previewBanner;   
            }
            anim.ClearImages();
            AnimatedImage newImage = image;
            if (IsAnimation())
            {
                newImage = image.DeepClone();
            }
            newImage.Reset();
            
            // Now we need to calculate in order to make the image at least 85% of our screen
            if (!newImage.isVideo)
            {
                Texture2D thumbnail = newImage.GetThumbnail();
                if (thumbnail == null)
                {
                    return;
                }
                
                float maxWidth = Screen.width * 0.85f;
                float multiplier = maxWidth / thumbnail.width;
                newImage.initialLocalScale = new Vector3(multiplier, multiplier, 0);
            }
            
            anim.AddImage(newImage);
            PreviewController._instance.SetupDebugText(newImage);
            PreviewController._instance.ShowDecoy(anim, true);
        }

        private void OnLongClick(AnimatedImage image)
        {
            ChannelAnimation anim = image.channelAnimation.type == CHANNELTYPE.ICON ? previewIcon : previewBanner;
            anim.ClearImages();
            AnimatedImage clone = image;
            if (IsAnimation())
            {
                clone = image.DeepClone();
            }
            anim.AddImage(clone);
            PreviewController._instance.SetupDebugText(clone);
            PreviewController._instance.ShowDecoy(anim, true);
        }

        public override void ClickedBack()
        {
            if (!wasAudioMuted && previewTheme != null)
            {
                AudioController.PlayBackgroundShopMusic(skipStart: true);
            }
            _instance.browseContentView.ShowWithoutChanging();
            if (myTheme != null && previewTheme != null && !myTheme.Equals(previewTheme))
            {
                ThemeController._instance.DeleteTheme(previewTheme);
            }
        }
        
        public void ClickedPreapprove(string id = null)
        {
            ClearOtherMenus();
            _instance.StartCoroutine(_GoBackPreapprove());
        }

        private IEnumerator _GoBackPreapprove()
        {
            Debug.Log("Preapproved, loading back my own theme!");
            AudioController.MuteBackgroundAudio();
            ThemeController._instance.LoadCurrentTheme(false);
            yield return new WaitUntil(ThemeController._instance.currentTheme.FinishedLoading);
            ShowThemeElements();
        }

        public void ClickedApprove(string id = null)
        {
            string name = IsAnimation() ? "popup.mod.approveanim" : "popup.mod.approvetheme";
            PopupController.ShowPopup(name, () =>
            {
                ReviewAnimation(true, id, IsAnimation());
            }, PopupController.ClosePopup, customPrefab: PopupController._instance.unthemedPopupPrefab);
        }

        public void ClickedReject(ContentType type, string id = null)
        {
            if (type == ContentType.ANIMATIONS)
            {
                PopupController.ShowPopup("popup.mod.rejectanim", () =>
                {
                    ReviewAnimation(false, id, true);
                }, PopupController.ClosePopup, customPrefab: PopupController._instance.unthemedPopupPrefab);
            }
            else
            {
                PopupController.ShowPopup("popup.mod.rejecttheme", () =>
                {
                    ReviewAnimation(false, id, false);
                }, PopupController.ClosePopup, customPrefab: PopupController._instance.unthemedPopupPrefab);
            }
        }

        private async void ReviewAnimation(bool approve, string specialId, bool isAnimation)
        {
            // We send the request
            string requestId = string.IsNullOrEmpty(specialId) ? animId : specialId;
            string text;
            if (isAnimation)
            {
                text = approve ? "popup.mod.animapproved" : "popup.mod.animrejected";
            }
            else
            {
                text = approve ? "popup.mod.themeapproved" : "popup.mod.themerejected";
            }

            await _instance.SendRequest(WebRequestController.SendReviewAnimation(isAnimation ? ContentType.ANIMATIONS : ContentType.THEMES, approve, requestId));
            PopupController.ShowPopup(text, () =>
            {
                if (!isAnimation && string.IsNullOrEmpty(specialId))
                {
                    // we clear other menus and return to ours
                    ClearOtherMenus();
                    AudioController.MuteBackgroundAudio();
                    RecoverPreviousTheme();
                }
                _instance.browseQueryView.Show(SearchMode.REVIEW, isAnimation ? ContentType.ANIMATIONS : ContentType.THEMES, newPage: _instance.browseQueryView.currentPage);
            }, customPrefab: PopupController._instance.unthemedPopupPrefab);
        }

        private void RecoverPreviousTheme()
        {
            // we also delete the local temp animation
            if (!previewTheme.GetImportedFileName().Equals(PREFS.CurrentTheme.GetString()))
            {
                Debug.Log("Recovering previous theme!");
                PREFS.UseThemeWallpaper.SetBool(wasUsingWallpaper);
                _instance.StartCoroutine(_RecoverTheme());
            }
            AudioController.StopMainAudio();
        }

        private IEnumerator _RecoverTheme()
        {
            ThemeController._instance.SetNewTheme(myTheme);
            yield return new WaitUntil(myTheme.FinishedLoading);
            if (!myTheme.Equals(previewTheme))
            {
                ThemeController._instance.DeleteTheme(previewTheme);
            }
            if (!wasAudioMuted)
            {
                AudioController.UnmuteBackgroundAudio();
                AudioController.PlayBackgroundShopMusic(skipStart: true);
            }
        }

        private void ClearOtherMenus()
        {
            _instance.shopMenu.SetActive(true);
            AppController._instance.PauseMainMenu();
            EditorController._instance.ClearOptions();
            EditorController._instance.channelEditor.SetActive(false);
            ThemeController._instance.themeReviewOverlay.SetActive(false);
            SettingsController._instance.ExitSettings(false, transition: false);
        }
    }
}