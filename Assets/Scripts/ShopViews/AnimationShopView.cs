using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Animations;
using Misc;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace ShopViews
{
    public class AnimationShopView : ShopView
    {
        private string animationId;
        private int animationBundle;
        private bool isLoadingContent;
        private bool isHearted;
        private bool isVideo;
        private Transform prefab;

        private bool hasIcon;
        private bool hasBanner;

        private bool previewedIcon;
        private bool previewedBanner;
        private bool isReview;
        private bool canReview;
        private bool previewedTheme; // this actually means whether we've downloaded it
        
        private string author;
        private string description;

        private ContentType contentType;
        private GameObject iconObject;
        private GameObject themeIconObject;
        private ThemedTextSprite verifiedBadge;

        private ChannelAnimation iconAnimation;
        private ChannelAnimation bannerAnimation;

        public AnimationShopView(GameObject view) : base(view, () => { })
        {
            ApplyScreenshotButton(_instance.animationBannerThumbnail);
            ApplyScreenshotButton(_instance.animationIconThemeThumbnail);
            ApplyScreenshotButton(_instance.animationIconThumbnail);
            verifiedBadge = _instance.animationAuthor.transform.Find("Checkmark").GetComponent<ThemedTextSprite>();
        }

        public async void ShowWithoutChanging()
        {
            Show(null,null,null,null,0, contentType, false, isReview);
        }

        public async void Show(Transform prefab, string id, RawImage iconSource, RawImage bannerSource, double rating, ContentType type, bool reset = true, bool isReview = false)
        {
            base.Show();
            if (!reset)
            {
                return;
            }
            isHearted = false;
            animationId = null;
            animationBundle = 0;
            isLoadingContent = false;
            hasIcon = false;
            hasBanner = false;
            previewedBanner = false;
            previewedIcon = false;
            isVideo = false;
            contentType = type;
            verifiedBadge.gameObject.SetActive(false);

            this.prefab = prefab;
            this.isReview = isReview;
            
            // Disable low memory mode if we're reviewing 
            if (isReview)
            {
                AppController._instance.SetLowMemoryMode(false);
            }
            
            // We change the download button for review if it's a review
            _instance.animationDownloadButton.SetActive(!isReview);
            _instance.animationPreviewButton.SetActive(isReview);
            _instance.checkReviewsButton.SetActive(!isReview);
            _instance.animationHeartButton.SetActive(!isReview);
            
            // And we fix the icon button
            _instance.animationIconThumbnail.gameObject.SetActive(contentType == ContentType.ANIMATIONS);
            _instance.animationIconThemeThumbnail.gameObject.SetActive(contentType == ContentType.THEMES);
            
            // We change the type
            _instance.animationPreviewButtons.SetActive(IsAnimation());
            _instance.themePreviewInfo.SetActive(!IsAnimation());
            
            // And if it's a theme we disable the info for now
            if (!IsAnimation())
            {
                _instance.themePreviewColorsYes.SetActive(false);
                _instance.themePreviewColorsNo.SetActive(false);
                _instance.themePreviewTexturesYes.SetActive(false);
                _instance.themePreviewTexturesNo.SetActive(false);
                _instance.themePreviewAudioYes.SetActive(false);
                _instance.themePreviewAudioNo.SetActive(false);
            }

            // We need to launch a query
            UnityWebRequest response = await _instance.SendRequest(WebRequestController.SendGetContent(type, id));
            JObject json = response.GetJsonResponse();
            if (!response.isNetworkError && !response.isHttpError)
            {
                // In this case the query was successful
                await SpawnAnimation(json, iconSource, bannerSource, rating);
            }
            else
            {
                switch (response.responseCode)
                {
                    case 404:
                    {
                        // No animation was found
                        PopupController.ShowPopup("popup.cantaccessanim", () =>
                        {
                            _instance.browseQueryView.Show(SearchMode.REGULAR, ContentType.ANIMATIONS, false);
                        });
                        break;
                    }
                }
            }
        }

        private bool IsAnimation()
        {
            return contentType == ContentType.ANIMATIONS;
        }
        
        private IEnumerator SpawnCheckmark(ThemedTextSprite sprite)
        {
            // this is stupid i hate that i have to do this
            // stupid TextMeshPro
            yield return new WaitForEndOfFrame();
            sprite.gameObject.SetActive(true);
            sprite.UpdateTexture();
        }

        private async Task SpawnAnimation(JObject jToken, RawImage iconSource, RawImage bannerSource, double rating)
        {
            // We spawn the texts
            animationId = jToken[IsAnimation() ? "animationId" : "themeId"].ToString();
            animationBundle = (int)jToken["bundle"];
            _instance.animationTitle.text = jToken["name"].ToString();
            description = jToken["description"].ToString();
            _instance.animationDescription.text = description;
            
            // we spawn the author and handle the badge otherwise
            author = jToken["author"].ToString();
            _instance.animationAuthor.text = string.Format(TextController.GetTranslation("ap.title.byauthor"),author);
            ThemedTextSprite checkMark = _instance.animationAuthor.transform.Find("Checkmark").GetComponent<ThemedTextSprite>();
            if (_instance.verifiedAuthors.Contains(author))
            {
                // we spawn the badge
                _instance.animationAuthor.text += " <link=check><sprite index=0></link>";
                _instance.animationAuthor.ForceMeshUpdate(true, true);
                _instance.StartCoroutine(SpawnCheckmark(checkMark));
            }

            DateTimeOffset createdDate = DateTimeOffset.Parse(jToken["createdDate"].ToString());
            DateTimeOffset updatedDate = DateTimeOffset.Parse(jToken["updatedDate"].ToString());
            _instance.animationCreation.text = string.Format(TextController.GetTranslation("ap.title.created"),createdDate.ToString(PREFS.IsAmericanDate.GetBool() ? "MM/dd/yyyy" : "dd/MM/yyyy"));
            _instance.animationUpdate.text = string.Format(TextController.GetTranslation("ap.title.updated"),updatedDate.ToString(PREFS.IsAmericanDate.GetBool() ? "MM/dd/yyyy" : "dd/MM/yyyy"));

            // We update the downloads and ratings
            _instance.animationDownloads.text = FormatDownloads((int)jToken["downloads"]);
            _instance.animationStars.text = rating.ToString("0.0", CultureInfo.InvariantCulture);

            // And whether it has icon+banner or not
            if (IsAnimation())
            {
                hasIcon = (bool)jToken["hasIcon"];
                hasBanner = (bool)jToken["hasBanner"];
            }
            
            // And update the heart status
            isHearted = (bool)jToken["isLiked"];
            ColorHeart();
            
            // And whether we could review it (mods+)
            UserRank rank = await _instance.SendRequest(WebRequestController.SendGetRank());
            canReview = rank >= UserRank.MOD;
            
            // And whether we can delete it or not
            ColorAnimationDeleteButton(ShopController.GetUserName().Equals(author) || rank >= UserRank.ADMIN);
            
            // And the icon colors
            ResetIconsColors();

            // And we change the download image to a warning if it's a video
            try
            {
                JArray tags = (JArray) jToken["tags"];
                foreach (JToken tag in tags)
                {
                    if (tag.ToString().Equals("video"))
                    {
                        isVideo = true;
                        break;
                    }
                }
            }
            catch (Exception _) { /*ignored*/ }
            _instance.animationDownloadRegular.gameObject.SetActive(!isVideo);
            _instance.animationDownloadVideo.gameObject.SetActive(isVideo);

            // We temporarily set the image to "loading" in the anim view
            _instance.animationIconThumbnail.texture = _instance.tempThumbnail;
            _instance.animationIconThemeThumbnail.texture = _instance.tempThumbnail;
            _instance.animationBannerThumbnail.texture = _instance.tempThumbnail;
            
            // If it's a theme, we load the info
            if (!IsAnimation())
            {
                bool changesColors = (bool)jToken["changesColor"];
                bool changesTexture = (bool)jToken["changesTexture"];
                bool changesAudio = (bool)jToken["changesAudio"];
                GameObject colorObj = changesColors ? _instance.themePreviewColorsYes : _instance.themePreviewColorsNo;
                GameObject textureObj = changesTexture ? _instance.themePreviewTexturesYes : _instance.themePreviewTexturesNo;
                GameObject audioObj = changesAudio ? _instance.themePreviewAudioYes : _instance.themePreviewAudioNo;
                colorObj.SetActive(true);
                textureObj.SetActive(true);
                audioObj.SetActive(true);
            }
            
            // Then we try to retrieve the images from the list (or we download it again if null)
            if (iconSource is not null && iconSource.texture != null && iconSource.texture != _instance.tempThumbnail && iconSource.gameObject.activeSelf)
            {
                _instance.animationIconThumbnail.gameObject.SetActive(IsAnimation());
                _instance.animationIconPreview.gameObject.SetActive(IsAnimation());
                _instance.animationIconThemeThumbnail.gameObject.SetActive(!IsAnimation());
                if (IsAnimation())
                {
                    _instance.animationIconThumbnail.texture = iconSource.texture;
                    _instance.animationIconPreview.texture = iconSource.texture;
                }
                else
                {
                    _instance.animationIconThemeThumbnail.texture = iconSource.texture;
                }
            }
            else
            {
                LoadThumbnail(IsAnimation() ? _instance.animationIconThumbnail : _instance.animationIconThemeThumbnail,animationId,CHANNELTYPE.ICON,false);
                if (IsAnimation())
                {
                    LoadThumbnail(_instance.animationIconPreview,animationId,CHANNELTYPE.ICON,true, true);
                }
            }
            if (bannerSource is not null && bannerSource.texture != null && bannerSource.texture != _instance.tempThumbnail && bannerSource.gameObject.activeSelf)
            {
                _instance.animationBannerThumbnail.gameObject.SetActive(true);
                _instance.animationBannerPreview.gameObject.SetActive(IsAnimation());
                _instance.animationBannerThumbnail.texture = bannerSource.texture;
                if (IsAnimation())
                {
                    _instance.animationBannerPreview.texture = bannerSource.texture;
                }
            }
            else
            {
                LoadThumbnail(_instance.animationBannerThumbnail,animationId,CHANNELTYPE.BANNER,false);
                if (IsAnimation())
                {
                    LoadThumbnail(_instance.animationBannerPreview,animationId,CHANNELTYPE.BANNER,true, true);
                }
            }
            
            // We change the uninstall button availability
            ColorFileUninstallButton();
        }

        private void ResetIconsColors()
        {
            _instance.animationBannerPreview.color = Color.white;
            _instance.animationIconPreview.color = Color.white;
        }

        private void LoadThumbnail(RawImage target, string animId, CHANNELTYPE type, bool sendReplacement, bool shouldTint = false)
        {
            WebRequestController.SendDownloadThumbnail(animId,contentType,type,target,sendReplacement ? _instance.notFoundThumbnail : null,
                tex =>
                {
                    if (shouldTint && tex is null)
                    {
                        target.color = ThemeController.GetColor(ThemeColor.HubAnimationButtonsIcons);
                    }
                });
        }

        private void ApplyScreenshotButton(RawImage image)
        {
            Button button = image.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() =>
                {
                    _instance.ShowScreenshot(image.texture);
                });
            }
        }

        public override void ClickedBack()
        {
            if (!IsPreviewing() && !_instance.IsHandlingRequest())
            {
                Hide();
                _instance.browseQueryView.Show(SearchMode.REGULAR, contentType, false);
            }
        }

        public override void Hide()
        {
            base.Hide();
            ResetIconsColors();
        }

        public void UnloadShownAnimations()
        {
            iconAnimation?.UnloadIfUnused();
            bannerAnimation?.UnloadIfUnused();
        }

        public async void ClickedPreviewIcon()
        {
            await Preview(CHANNELTYPE.ICON);
        }

        public async void ClickedPreviewBanner()
        {
            await Preview(CHANNELTYPE.BANNER);
        }

        private async Task Preview(CHANNELTYPE type)
        {
            if (_instance.IsHandlingRequest() || IsPreviewing())
            {
                return;
            }

            // First we check if we have it in our files
            string fileName = $"{type.ToString().ToLowerInvariant()}_{animationId}.zip";
            bool needsDownload = false;
            if (!SaveManager.HasTempChannel(animationId,type) && !SaveManager.HasOnlineChannel(animationId,type))
            {
                // In this case, we don't have already temp-downloaded it and we don't actually have it in our files
                Debug.Log($"Downloading {animationId} to preview it!");
                needsDownload = true;
            }
            else
            {
                // Then we check if our animation is updated
                OnlineInfo onlineInfo = ChannelController._instance.GetOnlineInfo(fileName,true,true);
                if (onlineInfo == null || animationBundle > onlineInfo.bundle)
                {
                    // Our anim is outdated, we need to re-download it (or it wasn't found, in that case we need to download it anyway)
                    Debug.Log($"Updating {animationId} to preview it!");
                    needsDownload = true;
                }
            }

            if (needsDownload)
            {
                await _instance.DownloadAnimation(type, animationId, true);
            }
            
            // We set the proper values to true
            if (type == CHANNELTYPE.ICON)
            {
                previewedIcon = true;
            }
            else
            {
                previewedBanner = true;
            }

            // And we load the preview
            isLoadingContent = true;
            _instance.StartCoroutine(LoadPreviewAnimation(fileName));
        }

        private IEnumerator LoadPreviewAnimation(string fileName)
        {
            ChannelAnimation channelAnimation = ChannelController._instance.GetOrLoadChannelAnimation(fileName,true,true);
            if (channelAnimation != null)
            {
                yield return new WaitUntil(channelAnimation.FinishedLoading);
                if (channelAnimation.type == CHANNELTYPE.ICON)
                {
                    iconAnimation = channelAnimation;
                }
                else
                {
                    bannerAnimation = channelAnimation;
                }
                
                PreviewController._instance.ShowDecoy(channelAnimation,true);
                CheckDebugText(channelAnimation);
            }
            else
            {
                PopupController.ShowPopup("popup.typeunavailable");
            }
            isLoadingContent = false;
        }
        
        private async Task PreviewTheme()
        {
            if (_instance.IsHandlingRequest() || IsAnimation())
            {
                return;
            }

            // First we check if we have it in our files
            string fileName = $"theme_{animationId}.zip";
            bool needsDownload = false;
            if (!SaveManager.HasOnlineTheme(animationId))
            {
                // In this case, we don't have already downloaded it and we don't actually have it in our files
                Debug.Log($"Downloading {animationId} to preview it!");
                needsDownload = true;
            }
            else
            {
                // Then we check if our theme is updated
                OnlineInfo onlineInfo = ThemeController._instance.GetOnlineInfo(fileName);
                if (onlineInfo == null || animationBundle > onlineInfo.bundle)
                {
                    // Our theme is outdated, we need to re-download it (or it wasn't found, in that case we need to download it anyway)
                    Debug.Log($"Updating {animationId} to preview it!");
                    needsDownload = true;
                }
            }

            if (needsDownload)
            {
                await _instance.DownloadTheme(animationId);
                
            }

            // And we load the preview
            isLoadingContent = true;
            _instance.StartCoroutine(LoadPreviewTheme(fileName));
        }
        
        private IEnumerator LoadPreviewTheme(string fileName)
        {
            Theme theme = ThemeController._instance.GetOrLoadTheme(fileName);
            if (theme != null)
            {
                // we wait until it loads and run the channel
                yield return new WaitUntil(theme.FinishedLoading);
                _instance.contentReviewView.Show(animationId,theme);
            }
            else
            {
                PopupController.ShowPopup("popup.themeunavailable");
            }
            isLoadingContent = false;
        }

        private void CheckDebugText(ChannelAnimation anim)
        {
            if (anim.type == CHANNELTYPE.ICON && _instance.browseQueryView.searchMode == SearchMode.REVIEW)
            {
                PreviewController._instance.maxDebugText.gameObject.SetActive(true);
                PreviewController._instance.maxDebugText.text = "MAX: " + (Math.Floor(anim.duration * 100) / 100f);
            }
        }

        private bool IsPreviewing()
        {
            return isLoadingContent || PreviewController._instance.isOpen;
        }

        public void ClickedDownload()
        {
            if (animationId == null || IsPreviewing() || _instance.IsHandlingRequest())
            {
                return;
            }

            if (isVideo)
            {
                PopupController.ShowPopup("popup.videoanim", () =>
                {
                    DownloadElement();
                    PopupController.ClosePopup();
                }, PopupController.ClosePopup);
            }
            else
            {
                DownloadElement();
            }
        }

        private void DownloadElement()
        {
            Debug.Log("Starting download of element!");
            if (IsAnimation())
            {
                _instance.TryDownloadAnimation(animationId,animationBundle,hasIcon,hasBanner,true,prefab);
            }
            else
            {
                _instance.TryDownloadTheme(animationId,animationBundle,true,prefab);
            }
        }

        public async void ClickedReview()
        {
            if (canReview && !IsPreviewing())
            {
                if (IsAnimation())
                {
                    if ((!hasIcon || previewedIcon) && (!hasBanner || previewedBanner))
                    {
                        // We load the animations and launch the review
                        // IMPORTANT: Previewing an anim already makes sure that we have the latest version downloaded, so no need to check it here too
                        string iconPath = $"icon_{animationId}.zip";
                        string bannerPath = $"banner_{animationId}.zip";
                        ChannelAnimation icon = ChannelController._instance.GetOrLoadChannelAnimation(iconPath,true,true);
                        ChannelAnimation banner = ChannelController._instance.GetOrLoadChannelAnimation(bannerPath,true,true);
                        _instance.contentReviewView.Show(animationId,icon,banner);
                    }
                    else
                    {
                        // We force mods to preview them, not only for safety but also for them to temp the animation
                        PopupController.ShowPopup("popup.mod.previewboth");
                    }
                }
                else if(!previewedTheme)
                {
                    // we fully download it and them go for it
                    await PreviewTheme();
                }
            }
        }

        private void ColorHeart()
        {
            _instance.animationHeart.color = ThemeController.GetColor(isHearted ? ThemeColor.HubHeartIcon : ThemeColor.HubUnheartIcon);
        }

        public async void ClickedHeart()
        {
            if (_instance.IsHandlingRequest() || isReview)
            {
                return;
            }
            
            isHearted = !isHearted;
            int target = isHearted ? 1 : 0;
            if (await _instance.IsLoggedIn())
            {
                UnityWebRequest response = await _instance.SendRequest(WebRequestController.SendHeartAnimation(contentType, target, animationId));
                if (!response.isNetworkError && !response.isHttpError)
                {
                    ColorHeart();
                }
            }
        }

        private void ColorFileUninstallButton()
        {
            bool isInstalled;
            if (IsAnimation())
            {
                isInstalled = SaveManager.HasOnlineChannel(animationId, CHANNELTYPE.ICON) || SaveManager.HasOnlineChannel(animationId, CHANNELTYPE.BANNER);
            }
            else
            {
                isInstalled = SaveManager.HasOnlineTheme(animationId);
            }
            
            _instance.animationUninstallButton.SetActive(isInstalled);
        }
        
        private void ColorAnimationDeleteButton(bool isMine)
        {
            _instance.animationDeleteButton.SetActive(isMine);
        }

        public void ClickedUninstall()
        {
            if (isReview)
            {
                return;
            }

            if (IsAnimation())
            {
                if(SaveManager.HasOnlineChannel(animationId,CHANNELTYPE.ICON) || SaveManager.HasOnlineChannel(animationId,CHANNELTYPE.BANNER))
                {
                    // We have the animation
                    PopupController.ShowPopup("popup.uninstallanim", () =>
                    {
                        string iconName = $"icon_{animationId}.zip";
                        string bannerName = $"banner_{animationId}.zip";
                        ChannelAnimation icon = ChannelController._instance.GetOrLoadChannelAnimation(iconName);
                        ChannelAnimation banner = ChannelController._instance.GetOrLoadChannelAnimation(bannerName);
                        ChannelController._instance.DeleteChannelAnimation(icon);
                        ChannelController._instance.DeleteChannelAnimation(banner);
                
                        RecolorUninstallListButton(prefab);
                        ColorFileUninstallButton();
                        PopupController.ShowPopup("popup.animuninstalled", ClickedBack);
                    }, PopupController.ClosePopup);
                }
            }
            else
            {
                if(SaveManager.HasOnlineTheme(animationId))
                {
                    // We have the animation
                    PopupController.ShowPopup("popup.uninstalltheme", () =>
                    {
                        string path = $"theme_{animationId}.zip";
                        Theme theme = ThemeController._instance.GetOrLoadTheme(path);
                        ThemeController._instance.DeleteTheme(theme);
                
                        RecolorUninstallListButton(prefab);
                        ColorFileUninstallButton();
                        PopupController.ShowPopup("popup.themeuninstalled", ClickedBack);
                    }, PopupController.ClosePopup);
                }
            }

        }

        public void ClickedDelete()
        {
            if (_instance.IsHandlingRequest() || isReview)
            {
                return;
            }
            
            // This removes the animation FROM THE STORE
            string name1 = IsAnimation() ? "popup.deleteonlineanim.1" : "popup.deleteonlinetheme.1";
            string name2 = IsAnimation() ? "popup.deleteonlineanim.2" : "popup.deleteonlinetheme.2";
            PopupController.ShowPopup(name1,
                () =>
                {
                    PopupController.ShowPopup(name2, DeleteAnimation, PopupController.ClosePopup);
                }, PopupController.ClosePopup);
        }

        private async void DeleteAnimation()
        {
            if (_instance.IsHandlingRequest())
            {
                return;
            }
            
            await _instance.SendRequest(WebRequestController.SendDeleteAnimation(contentType, animationId));

            string name = IsAnimation() ? "popup.deletedonlineanim" : "popup.deletedonlinetheme";
            PopupController.ShowPopup(name, () =>
            {
                if (prefab is not null)
                {
                    prefab.gameObject.SetActive(false);
                }
                ClickedBack();
            });
        }

        public void ClickedRating()
        {
            if (isReview)
            {
                return;
            }
            
            _instance.ratingsView.Show(animationId, contentType);
        }

        public void ClickedDescription()
        {
            if (_instance.IsHandlingRequest())
            {
                return;
            }

            PopupController.ShowPopup(description);
        }

        public void ClickedAuthor()
        {
            if (_instance.IsHandlingRequest())
            {
                return;
            }
            
            _instance.browseQueryView.Show(SearchMode.AUTHOR_SEARCH, contentType, overrideAuthor:author);
        }
    }
}