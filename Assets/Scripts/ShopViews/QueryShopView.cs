using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Animations;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Image = UnityEngine.UIElements.Image;

namespace ShopViews
{
    public class QueryShopView : ShopView
    {
        public int currentPage = 1;
        private int currentMaxPage;
        private string overrideGuidSearch;
        private string overrideAuthor;
        public SearchMode searchMode;
        public ContentType contentType;
        private Transform holder;
        private List<Texture2D> cachedTextures = new List<Texture2D>();

        public QueryShopView(GameObject view) : base(view, () => { })
        {
            holder = view.transform.Find("Content").Find("Scroll View").Find("Viewport").Find("Content");
        }

        public async void Show(SearchMode searchMode, ContentType contentType, bool reset = true, int newPage = 1, string overrideGuidSearch = null, string overrideAuthor = null)
        {
            base.Show();
            
            // we restore the low memory mode and unload the just-shown animations
            _instance.RestoreLowMemoryMode();
            _instance.browseContentView.UnloadShownAnimations();
            
            // if we don't need to reset anything, we return
            if (!reset)
            {
                return;
            }

            this.searchMode = searchMode;
            this.contentType = contentType;
            currentPage = newPage;
            this.overrideGuidSearch = overrideGuidSearch;
            this.overrideAuthor = overrideAuthor;

            foreach (Transform child in holder)
            {
                GameObject.Destroy(child.gameObject);
            }
            
            Resources.UnloadUnusedAssets();
            
            // We need to launch a query
            string search = _instance.searchCategoriesView.GetView().transform.Find("Content").Find("Search")
                .Find("Vector").Find("Text").GetComponent<TMP_InputField>().text.Trim();
            string author = _instance.searchCategoriesView.GetView().transform.Find("Content").Find("Author")
                .Find("Vector").Find("Text").GetComponent<TMP_InputField>().text.Trim();
            int sortInt = _instance.searchCategoriesView.GetView().transform.Find("Content").Find("Sort")
                .Find("Dropdown").GetComponent<TMP_Dropdown>().value;
            string sort = "popular";
            switch (sortInt)
            {
                case 1:
                    sort = "newest";
                    break;
                case 2:
                    sort = "oldest";
                    break;
            }

            UnityWebRequest response;
            if (searchMode == SearchMode.REGULAR || searchMode == SearchMode.MY_CONTENT)
            {
                bool fav = _instance.searchCategoriesView.onlyFavorites && searchMode == SearchMode.REGULAR;
                response = await _instance.SendRequest(WebRequestController.SendSearchRequest(contentType, search, author, sort, currentPage,
                    _instance.browseFiltersView.filterList, overrideId: overrideGuidSearch, onlyFavorites: fav));
            }
            else if (searchMode == SearchMode.REVIEW)
            {
                response = await _instance.SendRequest(
                    WebRequestController.SendSearchRequest(contentType, null, null, "oldest", currentPage, overrideReview: true));
            }
            else
            {
                response = await _instance.SendRequest(
                    WebRequestController.SendSearchRequest(contentType, null, overrideAuthor, "popular", currentPage));
            }

            JObject json = response.GetJsonResponse();
            if (!response.isNetworkError && !response.isHttpError)
            {
                // In this case the query was successful
                SetMaxPage((int) json["pages"]);
                JArray array = (JArray) json[IsAnimation() ? "animations" : "themes"];
                foreach (JToken anim in array)
                {
                    SpawnElement(anim);
                }
            }
            else
            {
                switch (response.responseCode)
                {
                    case 404:
                    {
                        // No animation was found
                        string name = IsAnimation() ? "popup.querynotfound" : "popup.querynotfoundtheme"; 
                        PopupController.ShowPopup(name, ClickedBack);
                        break;
                    }
                }
            }
        }

        private void SetMaxPage(int maxPage)
        {
            ShopController._instance.pageText.text = $"{currentPage}/{maxPage}";
            this.currentMaxPage = maxPage;
        }

        public void ClickedLeftArrow()
        {
            if (currentPage > 1)
            {
                Show(searchMode, contentType, true, currentPage - 1, overrideGuidSearch, overrideAuthor);
            }
        }

        public void ClickedRightArrow()
        {
            if (currentPage < currentMaxPage)
            {
                Show(searchMode, contentType, true, currentPage + 1, overrideGuidSearch, overrideAuthor);
            }
        }

        private bool HasElementInstalled(string id)
        {
            if (contentType == ContentType.ANIMATIONS)
            {
                return SaveManager.HasOnlineChannel(id, CHANNELTYPE.ICON) || SaveManager.HasOnlineChannel(id, CHANNELTYPE.BANNER);
            }

            return SaveManager.HasOnlineTheme(id);
        }

        private IEnumerator SpawnCheckmark(ThemedTextSprite sprite)
        {
            // this is stupid i hate that i have to do this
            // stupid TextMeshPro
            yield return new WaitForEndOfFrame();
            sprite.UpdateTexture();
            yield return new WaitForEndOfFrame();
            sprite.gameObject.SetActive(true);
        }

        private async void SpawnElement(JToken jToken)
        {
            // We spawn the name and the author
            string id = IsAnimation() ? jToken["animationId"].ToString() : jToken["themeId"].ToString();
            string name = jToken["name"].ToString();
            Transform prefab = GameObject.Instantiate(IsAnimation() ? _instance.animationListPrefab : _instance.themeListPrefab, holder).transform;
            prefab.Find("Text").Find("Title").Find("Title").GetComponent<TextMeshProUGUI>().text = name;
            
            // we spawn the author and set the verified badge if needed
            TextMeshProUGUI authorText = prefab.Find("Text").Find("Author").Find("Title").GetComponent<TextMeshProUGUI>();
            authorText.text = jToken["author"].ToString();
            if (_instance.verifiedAuthors.Contains(authorText.text))
            {
                authorText.text += " <link=check><sprite index=0></link>";
                authorText.ForceMeshUpdate(true, true);
                ThemedTextSprite checkMark = prefab.Find("Text").Find("Author").Find("Title").Find("Checkmark").GetComponent<ThemedTextSprite>();
                _instance.StartCoroutine(SpawnCheckmark(checkMark));
            }

            // We change its color depending on whether we have it or not
            if (HasElementInstalled(id))
            {
                RecolorInstalledListButton(prefab);
            }

            // We update the downloads and ratings
            prefab.Find("Text").Find("Data").Find("Downloads").Find("Title").GetComponent<TextMeshProUGUI>().text = FormatDownloads((int) jToken["downloads"]);

            // We launch a query to retrieve the rating
            UnityWebRequest request = await _instance.SendRequest(WebRequestController.SendGetRating(contentType,id), playSound: false);
            if (prefab == null)
            {
                // It means that while we were getting the rating, the user already moved on to another screen.
                // So this object is now dead, not worth wasting our time setting it up.
                return;
            }
            
            double rating = 0;
            if (!request.isNetworkError && !request.isHttpError)
            {
                rating = (double)request.GetJsonResponse()["rating"];
            }
            prefab.Find("Text").Find("Data").Find("Stars").Find("Title").GetComponent<TextMeshProUGUI>().text = rating.ToString("0.0", CultureInfo.InvariantCulture);

            // Then we try to download the images (no need to await - they should load as they get downloaded)
            RawImage iconThumbnail = prefab.Find("Thumbnails").Find("Icon").GetComponent<RawImage>();
            RawImage bannerThumbnail = prefab.Find("Thumbnails").Find("Banner").GetComponent<RawImage>();
            LoadThumbnail(iconThumbnail, id, CHANNELTYPE.ICON);
            LoadThumbnail(bannerThumbnail, id, CHANNELTYPE.BANNER);
            if (!IsAnimation())
            {
                // we also load the accent
                UICornerCut accent = prefab.Find("Thumbnails").Find("Accent").GetComponent<UICornerCut>();
                string color = "#" + jToken["accent"];
                Color accentColor;
                ColorUtility.TryParseHtmlString(color, out accentColor);
                accent.color = accentColor;
            }

            // Finally we setup the button
            prefab.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (_instance.IsHandlingRequest())
                {
                    return;
                }
                
                if (searchMode == SearchMode.MY_CONTENT && !string.IsNullOrEmpty(overrideGuidSearch))
                {
                    LoadContentToUpdate(id);
                }
                else if (searchMode == SearchMode.REGULAR || searchMode == SearchMode.AUTHOR_SEARCH)
                {
                    _instance.browseContentView.Show(prefab, id, iconThumbnail, bannerThumbnail, rating, contentType);
                }
                else
                {
                    _instance.browseContentView.Show(prefab, id, iconThumbnail, bannerThumbnail, rating, contentType, isReview: true);
                }
            });
            
            // And for mods, we setup a quick-reject method
            if (searchMode == SearchMode.REVIEW)
            {
                prefab.GetComponent<LongClickButton>().onLongClick.AddListener(() =>
                {
                    _instance.contentReviewView.ClickedReject(contentType, id);
                });
            }
        }

        private bool IsAnimation()
        {
            return contentType == ContentType.ANIMATIONS;
        }

        private async void LoadContentToUpdate(string id)
        {
            if (_instance.IsHandlingRequest())
            {
                return;
            }
            
            UnityWebRequest response = await _instance.SendRequest(WebRequestController.SendGetContent(contentType, id));
            if (!response.isNetworkError && !response.isHttpError)
            {
                JObject jToken = response.GetJsonResponse();
                string name = jToken["name"]?.ToString();
                string description = jToken["description"]?.ToString();
                int animationBundle = (int)jToken["bundle"];
                
                // now we separate by theme or animation
                if (IsAnimation())
                {
                    bool hasIcon = (bool)jToken["hasIcon"];
                    bool hasBanner = (bool)jToken["hasBanner"];

                    if (hasIcon)
                    {
                        await _instance.DownloadAnimation(CHANNELTYPE.ICON, id, true);
                    }
                    if (hasBanner)
                    {
                        await _instance.DownloadAnimation(CHANNELTYPE.BANNER, id, true);
                    }
                
                    var icon = ChannelController._instance.GetOrLoadChannelAnimation($"icon_{id}.zip", false, true);
                    var banner = ChannelController._instance.GetOrLoadChannelAnimation($"banner_{id}.zip", false, true);

                    _instance.animationNewUploadView.SetupOnlineAnimation(id, name, description, icon, banner);
                    _instance.animationNewUploadView.Show(ContentType.ANIMATIONS, false);
                }
                else
                {
                    await _instance.DownloadTheme(id);
                    var theme = ThemeController._instance.GetOrLoadTheme($"theme_{id}.zip");
                    _instance.animationNewUploadView.SetupOnlineTheme(id, name, description, theme);
                    _instance.animationNewUploadView.Show(ContentType.THEMES, false);
                }

            }
        }

        private void LoadThumbnail(RawImage target, string animId, CHANNELTYPE type)
        {
            WebRequestController.SendDownloadThumbnail(animId, contentType, type, target, null, tex =>
            {
                cachedTextures.Add(tex);
            });
        }

        public override void ClickedBack()
        {
            if (searchMode == SearchMode.REGULAR || searchMode == SearchMode.AUTHOR_SEARCH)
            {
                _instance.searchCategoriesView.Show();
            }
            else
            {
                // Both updating an anim, and reviewing an anim, lead back to this menu
                _instance.uploadCategoryView.Show();
            }

            cachedTextures.ForEach(GameObject.Destroy);
            cachedTextures.Clear();
            Resources.UnloadUnusedAssets();
        }
    }
    
    public enum SearchMode
    {
        REGULAR, MY_CONTENT, REVIEW, AUTHOR_SEARCH
    }
}