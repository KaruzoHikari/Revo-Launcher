using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Animations;
using Misc;
using Newtonsoft.Json.Linq;
using ShopViews;
using TMPro;
using TriInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public enum ContentType
{
    ANIMATIONS, THEMES
}

[DeclareFoldoutGroup("Debug Auth")]
public class ShopController : MonoBehaviour
{
    public static ShopController _instance;
    public GameObject shopMenu;
    public RectTransform optionsHolder;
    public GameObject backButton;
    public GameObject confirmButton;
    public GameObject loadingCircles;
    public float iconThumbnailY = 4f;
    public float iconThumbnailHeight = 2.5f;
    public GameObject animationListPrefab;
    public GameObject themeListPrefab;
    public GameObject reviewItemPrefab;
    public GameObject reviewIconSeparator;
    public GameObject reviewBannerSeparator;
    public Texture2D warningThumbnail;
    public Texture2D tempThumbnail;
    public Texture2D notFoundThumbnail;
    public List<string> verifiedAuthors = new List<string>();
    public bool rotateCircle;
    private bool isHandlingRequest;
    private bool wasLowMemoryMode = true;

    [Title("Views")]
    public GameObject shopLoadingView;
    public GameObject shopMainView;
    public GameObject shopUserAuthView;
    public GameObject shopRegisterView;
    public GameObject shopLoginView;
    public GameObject shopEmailCodeView;
    public GameObject shopAnimationUploadView;
    public GameObject shopAnimationNewUploadView;
    public GameObject shopBrowseCategoriesView;
    public GameObject shopBrowseFiltersView;
    public GameObject shopBrowseQueryView;
    public GameObject shopBrowseAnimationView;
    public GameObject shopAnimationReviewView;
    public GameObject shopUpdateView;
    public GameObject shopRatingsView;
    public GameObject shopAddRatingsView;
    public GameObject shopBrowseShopView;
    public GameObject shopBrowseUploadShopView;
    public GameObject shopHubSettingsView;
    public ShopView loadingView;
    public ShopView mainView;
    public ShopView userAuthView;
    public ShopView registerView;
    public ShopView loginView;
    public ShopView emailCodeView;
    public BrowseShopView browseShopView;
    public BrowseUploadShopView browseUploadShopView;
    public UploadShopView animationNewUploadView;
    public SearchCategoriesView searchCategoriesView;
    public QueryFiltersView browseFiltersView;
    public QueryShopView browseQueryView;
    public AnimationShopView browseContentView;
    public ReviewShopView contentReviewView;
    public UpdateShopView updateView;
    public RatingShopView ratingsView;
    public AddRatingShopView addRatingsView;
    public UploadCategoryShopView uploadCategoryView;
    public HubSettingsView hubSettingsView;

    [Title("Query Objects")]
    public TextMeshProUGUI pageText;
    public GameObject onlyFavoritesPrefab;
    public GameObject filterPrefab;
    public List<string> filtersNameList = new List<string>();
    public TextMeshProUGUI filterNumberText;

    [Title("Review Objects")]
    public GameObject reviewMenuButton;
    public GameObject ratingsReviewMenuButton;

    [Title("Rating Objects")]
    public GameObject addReviewButton;
    public GameObject editReviewButton;
    public GameObject reviewPrefab;
    public GameObject starPrefab;
    /*public Color starClickedColor;
    public Color starUnclickedColor;*/
    public TMP_InputField reviewDescription;
    public GameObject deleteReviewButton;
    public GameObject checkReviewsButton;
    public TextMeshProUGUI reviewPageText;

    [Title("Preview Objects")]
    public TextMeshProUGUI animationTitle;
    public TextMeshProUGUI animationAuthor;
    public TextMeshProUGUI animationDescription;
    public TextMeshProUGUI animationCreation;
    public TextMeshProUGUI animationUpdate;
    public TextMeshProUGUI animationStars;
    public TextMeshProUGUI animationDownloads;
    public GameObject animationHeartButton;
    public Image animationHeart;
    public Image animationDownloadRegular;
    public Image animationDownloadVideo;
    public RawImage animationIconThumbnail;
    public RawImage animationIconThemeThumbnail;
    public RawImage animationBannerThumbnail;
    public RawImage animationIconPreview;
    public RawImage animationBannerPreview;
    public GameObject animationDownloadButton;
    public GameObject animationPreviewButton;
    public GameObject animationUninstallButton;
    public GameObject animationDeleteButton;
    public GameObject animationPreviewButtons;
    public GameObject themePreviewInfo;
    public GameObject themePreviewColorsYes;
    public GameObject themePreviewColorsNo;
    public GameObject themePreviewTexturesYes;
    public GameObject themePreviewTexturesNo;
    public GameObject themePreviewAudioYes;
    public GameObject themePreviewAudioNo;

    [Title("Preview Objects")]
    public GameObject screenshotHolder;
    public RawImage screenshot;
    public float borderOffset = 0.2f;

    [GroupNext("Debug Auth")]
    public string debugUsername;
    public string debugId;
    public string debugAuth;
    public bool shouldUseDebug;
    [UnGroupNext]

    public GameObject hubPreferencesIntPrefab;
    public List<GameObject> loadingCircleShadows = new List<GameObject>();
    private Dictionary<string, string> pendingMessages = new Dictionary<string, string>();
    private bool fromMainMenu = false;

    private void GenerateViews()
    {
        loadingView = new ShopView(shopLoadingView, null);
        mainView = new ShopView(shopMainView, ExitShop, isBackButtonNamedExit:true);
        userAuthView = new ShopView(shopUserAuthView, mainView.Show);
        registerView = new ShopView(shopRegisterView, userAuthView.Show);
        loginView = new ShopView(shopLoginView, userAuthView.Show);
        emailCodeView = new ShopView(shopEmailCodeView, userAuthView.Show);
        animationNewUploadView = new UploadShopView(shopAnimationNewUploadView);
        searchCategoriesView = new SearchCategoriesView(shopBrowseCategoriesView);
        browseFiltersView = new QueryFiltersView(shopBrowseFiltersView);
        browseQueryView = new QueryShopView(shopBrowseQueryView);
        browseContentView = new AnimationShopView(shopBrowseAnimationView);
        contentReviewView = new ReviewShopView(shopAnimationReviewView);
        updateView = new UpdateShopView(shopUpdateView);
        ratingsView = new RatingShopView(shopRatingsView);
        addRatingsView = new AddRatingShopView(shopAddRatingsView);
        browseShopView = new BrowseShopView(shopBrowseShopView);
        browseUploadShopView = new BrowseUploadShopView(shopBrowseUploadShopView);
        uploadCategoryView = new UploadCategoryShopView(shopAnimationUploadView);
        hubSettingsView = new HubSettingsView(shopHubSettingsView);
    }
    
    private List<ShopView> GetViews()
    {
        return new List<ShopView>
        {
            loadingView, mainView, userAuthView, registerView, loginView, emailCodeView, uploadCategoryView,
            animationNewUploadView, searchCategoriesView, browseQueryView, browseContentView, contentReviewView,
            updateView, ratingsView, addRatingsView, browseFiltersView, browseShopView, browseUploadShopView,
            hubSettingsView
        };
    }

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        GenerateViews();

        if (WarningController._instance.debugLockScreen)
        {
            OpenShop();
        }
    }

    public void UpdateRotateCircle()
    {
        // we won't rotate the circle if it's a GIF (or if they disabled the option to do so)
        ThemeTexture tex = ThemeController.GetThemeTexture("loading_circle");
        bool isCircleAGif = tex != null && tex.isGif;
        rotateCircle = !isCircleAGif && PREFS.RotateLoadingCircle.GetBool();
        loadingCircleShadows.ForEach(shadow => shadow.SetActive(rotateCircle));
    }

    private void BackupLowMemoryMode()
    {
        // when reviewing we want to be in not low memory mode
        wasLowMemoryMode = AppController._instance.IsLowMemoryMode();
    }

    public void RestoreLowMemoryMode()
    {
        // we restore it to the previous value
        AppController._instance.SetLowMemoryMode(wasLowMemoryMode);
    }

    private void ExitShop()
    {
        // we save the preferences
        PreferencesSerializer.Save();
        
        // we need to see if we come from settings or from a channel!
        if (fromMainMenu)
        {
            SettingsController._instance.OpenSettings(null);
        }
        else
        {
            SettingsController._instance.OpenSettings();
        }
    }

    [Button]
    public void OpenShop(bool fromMainMenu = false)
    {
        this.fromMainMenu = fromMainMenu;
        BackupLowMemoryMode();
        StartCoroutine(_OpenShop());
    }

    private IEnumerator _OpenShop()
    {
        if (SettingsController._instance.settingsView.activeInHierarchy)
        {
            // we're entering from the settings
            SettingsController._instance.ExitSettings(false);
            if (!animationNewUploadView.GetView().activeSelf)
            {
                AudioController.MuteBackgroundAudio(true);
            }
            yield return new WaitForSeconds(1.5f * FadeController.GetSpeedMultiplier());
        }
        if (!shopMenu.activeInHierarchy || WarningController._instance.debugLockScreen)
        {
            ClearViews();
            CameraController.LockHorizontalMode();
            shopMenu.SetActive(true);
            loadingView.Show();

            Task load = LoadMessages();
            StartCoroutine(_PlayFirstLoadingSound());
            yield return new WaitForSeconds(2f);
            if (!load.IsCompleted)
            {
                yield return new WaitUntil(() => load.IsCompleted);
            }
            AppController._instance.PauseMainMenu();
            AudioController.StopShopLoadingAudio();
            AudioController.UnmuteBackgroundAudio();
            AudioController.PlayBackgroundShopMusic();
            mainView.Show();
            ShowMessages();
            FindVerifiedUsers();
        }
    }

    private async void FindVerifiedUsers()
    {
        UnityWebRequest response = await WebRequestController.SendGetVerifiedUsers();
        if (!response.isNetworkError && !response.isHttpError)
        {
            JObject obj = response.GetJsonResponse();
            JArray jArray = (JArray) obj["users"];
            foreach (JToken token in jArray)
            {
                verifiedAuthors.Add(token.ToString());
            }
        }
    }

    private IEnumerator _PlayFirstLoadingSound()
    {
        AudioClip clip = ThemeController.GetAudio(AudioLibrary._instance.SHOP_LOADING);
        if (clip is null)
        {
            // we play the original loading example
            AudioController.PlaySoundEffect(AudioLibrary._instance.SHOP_LOADING);
            yield return new WaitForSeconds(1.2f);
            AudioController.PlaySoundEffect(AudioLibrary._instance.SHOP_LOADING);
        }
        else
        {
            // we play the regular sound
            AudioController.PlayShopLoadingAudio();
        }
    }

    private async Task LoadMessages()
    {
        UnityWebRequest response = await SendRequest(WebRequestController.SendGetMessages(), false, false);
        if (!response.isNetworkError && !response.isHttpError)
        {
            JArray jArray = response.GetJsonArrayResponse();
            foreach (JToken token in jArray)
            {
                pendingMessages[token["id"].ToString()] = token["text"].ToString();
            }
        }
    }

    private void ShowMessages()
    {
        if (pendingMessages.Count > 0)
        {
            PopupController.ShowPopup("popup.welcomemessages", ShowNextMessage, replacementArray: new [] { GetUserName(), pendingMessages.Count.ToString() });
        }
    }

    private async void ShowNextMessage()
    {
        if (pendingMessages.Count > 0)
        {
            string id = pendingMessages.Keys.First();
            string text = pendingMessages[id];
            
            pendingMessages.Remove(id);
            await SendRequest(WebRequestController.SendDeleteMessage(id));
            
            PopupController.ShowPopup(text, ShowNextMessage, false);
        }
        else
        {
            PopupController.ClosePopup();
        }
    }

    [Button]
    public void ClearShop()
    {
        if (shopMenu.activeInHierarchy)
        {
            AudioController.MuteBackgroundAudio(true);
            ClearViews();
            shopMenu.SetActive(false);
            StartCoroutine(RestartMainMenuMusic());
        }
    }

    private IEnumerator RestartMainMenuMusic()
    {
        yield return new WaitForSeconds(0.35f);
        AudioController.UnmuteBackgroundAudio();
        AudioController.PlayBackgroundMenuMusic(2.3f, false);
    }
    
    public void ClickedSettings()
    {
        hubSettingsView.Show();
    }

    public void ClickedBrowseAnimations()
    {
        if (!IsHandlingRequest())
        {
            browseShopView.ClickedBrowseAnimations();
        }
    }
    
    public void ClickedBrowseThemes()
    {
        if (!IsHandlingRequest())
        {
            browseShopView.ClickedBrowseThemes();
        }
    }

    public void ClickedUpload()
    {
        if (!IsHandlingRequest())
        {
            browseUploadShopView.Show();
        }
    }

    public void SendDownMessage()
    {
        PopupController.ShowPopup("popup.shopdown");
    }
    
    public void SendNoAccountMessage(UnityAction okCallBack = null)
    {
        string popup = string.IsNullOrEmpty(GetUserId()) || string.IsNullOrEmpty(GetUserAuth())
            ? "popup.accountneeded" : "popup.accountexpired";

        if (okCallBack != null)
        {
            PopupController.ShowPopup(popup, okCallBack);
        }
        else
        {
            PopupController.ShowPopup(popup);
        }
    }

    public void ClickedRegister()
    {
        registerView.Show();
    }

    public void ClickedLogin()
    {
        loginView.Show();
    }

    public void ClickedSend()
    {
        if (shopRegisterView.activeInHierarchy)
        {
            SentRegister();
        }
        else if (shopLoginView.activeInHierarchy)
        {
            SentLogin();
        }
        else if (shopEmailCodeView.activeInHierarchy)
        {
            SentEmailCode();
        }
    }

    private bool IsValidEmail(string email)
    {
        var trimmedEmail = email.Trim();

        if (trimmedEmail.EndsWith(".")) {
            return false; // suggested by @TK-421
        }
        try {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == trimmedEmail;
        }
        catch {
            return false;
        }
    }

    private async void SentRegister()
    {
        // First we retrieve the username and email from the fields
        Transform registerTransform = shopRegisterView.transform.Find("Content").Find("Register");
        string username = registerTransform.Find("Username").Find("Vector").Find("Text").GetComponent<TMP_InputField>().text.Trim();
        string email = registerTransform.Find("Email").Find("Vector").Find("Text").GetComponent<TMP_InputField>().text.Trim();
        
        // We check both username and email are filled
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email))
        {
            PopupController.ShowPopup("popup.bothuseremail");
            return;
        }
        
        // We check if the email is valid
        if (!IsValidEmail(email) || email.Length > 320)
        {
            PopupController.ShowPopup("popup.invalidemail");
            return;
        }
        
        // We check if the username is too long
        if (username.Length > 16)
        {
            PopupController.ShowPopup("popup.invalidusername");
            return;
        }
        
        // Then we send the request
        UnityWebRequest response = await SendRequest(WebRequestController.SendCreateUser(username, email));
        JObject json = response.GetJsonResponse();
        if (!response.isNetworkError && !response.isHttpError)
        {
            // Everything went well
            PREFS.UserName.SetString(username);
            String userId = json["userId"]?.ToString();
            PREFS.UserId.SetString(userId);
            emailCodeView.Show();
        }
        else
        {
            if (response.responseCode == 409)
            {
                // Either the email is being used, or the username is
                string returnedUsername = json["username"]?.ToString();
                if (username.Equals(returnedUsername))
                {
                    // Username is being used
                    PopupController.ShowPopup("popup.usernameused");
                }
                else
                {
                    // Email is being used
                    PopupController.ShowPopup("popup.emailused");
                }
            }
        }
    }

    private async void SentLogin()
    {
        // First we retrieve the email from the fields
        Transform registerTransform = shopLoginView.transform.Find("Content").Find("Login");
        string email = registerTransform.Find("Email").Find("Vector").Find("Text").GetComponent<TMP_InputField>().text.Trim();
        
        // We check both username and email are filled
        if (string.IsNullOrEmpty(email))
        {
            PopupController.ShowPopup("popup.emailneeded");
            return;
        }
        
        // We check if the email is valid
        if (!IsValidEmail(email))
        {
            PopupController.ShowPopup("popup.invalidemail");
            return;
        }
        
        // Then we send the request
        UnityWebRequest response = await SendRequest(WebRequestController.SendLoginUser(email));
        JObject json = response.GetJsonResponse();
        if (!response.isNetworkError && !response.isHttpError)
        {
            // Everything went well
            String username = json["username"]?.ToString();
            PREFS.UserName.SetString(username);
            String userId = json["userId"]?.ToString();
            PREFS.UserId.SetString(userId);
            emailCodeView.Show();
        }
        else
        {
            if (response.responseCode == 404)
            {
                // No account with that email was found.
                PopupController.ShowPopup("popup.notfoundemail");
            }
        }
    }

    private async void SentEmailCode()
    {
        // First we retrieve the email from the fields
        Transform registerTransform = shopEmailCodeView.transform.Find("Content").Find("EmailCode");
        string emailCode = registerTransform.Find("Code").Find("Vector").Find("Text").GetComponent<TMP_InputField>().text.Trim();
        
        // We check both username and email are filled
        if (string.IsNullOrEmpty(emailCode))
        {
            PopupController.ShowPopup("popup.codeneeded");
            return;
        }
        
        // We check if the email code is valid
        if (!int.TryParse(emailCode, out _))
        {
            PopupController.ShowPopup("popup.invalidcode");
            return;
        }
        
        // Then we send the request
        UnityWebRequest response = await SendRequest(WebRequestController.SendAuthUser(emailCode));
        JObject json = response.GetJsonResponse();
        if (!response.isNetworkError && !response.isHttpError)
        {
            // In this case the auth was successful
            string auth = json["auth"]?.ToString();
            if (!string.IsNullOrEmpty(auth))
            {
                // we convert to base64
                PREFS.UserAuth.SetString(auth.ToBase64());
            }
            RefreshDownloadedContent();
            PopupController.ShowPopup("popup.welcomelogin", mainView.Show, replacementArray: new[] {GetUserName()});
        }
        else
        {
            switch (response.responseCode)
            {
                case 404:
                {
                    // No account with that email was found.
                    PopupController.ShowPopup("popup.cachednotfound");
                    break;
                }
                case 401:
                {
                    // Not a valid code
                    PopupController.ShowPopup("popup.wrongcode");
                    break;
                }
                case 403:
                {
                    // Forbidden, code too old
                    PopupController.ShowPopup("popup.expiredcode", ClickedBack);
                    break;
                }
            }
        }
    }

    private async void RefreshDownloadedContent()
    {
        // this sends a registered download increase in case you weren't logged in before
        foreach (OnlineInfo onlineInfo in ChannelController._instance.GetAllOnlineInfos())
        {
            await _instance.SendRequest(WebRequestController.SendIncreaseDownloadRegistered(ContentType.ANIMATIONS, onlineInfo.animationId));
        }
        foreach (OnlineInfo onlineInfo in ThemeController._instance.GetAllOnlineInfos())
        {
            await _instance.SendRequest(WebRequestController.SendIncreaseDownloadRegistered(ContentType.THEMES, onlineInfo.animationId));
        }
    }

    public void ClickedUploadAnimations()
    {
        browseUploadShopView.ClickedUploadAnimations();
    }
    
    public void ClickedUploadThemes()
    {
        browseUploadShopView.ClickedUploadThemes();
    }

    public void ClickedUploadCategory()
    {
        uploadCategoryView.ClickedUpload();
    }

    public void ClickedUpdateCategory()
    {
        uploadCategoryView.ClickedUpdate();
    }

    public void ClickedReviewMenu()
    {
        // We need to load the needs-review animations
        browseQueryView.Show(SearchMode.REVIEW, uploadCategoryView.contentType);
    }

    public void ClickedRatingsReviewMenu()
    {
        ratingsView.Show(null, uploadCategoryView.contentType,true);
    }

    public void ClickedReviewButton()
    {
        // We need to load the needs-review animations
        browseContentView.ClickedReview();
    }
    
    public void ClickedPreapproveButton()
    {
        contentReviewView.ClickedPreapprove();
    }

    public void ClickedApproveButton()
    {
        contentReviewView.ClickedApprove();
    }

    public void ClickedRejectButton()
    {
        contentReviewView.ClickedReject(contentReviewView.contentType);
    }

    public string GetAnimationFilePath(ChannelAnimation channelAnimation)
    {
        if (channelAnimation == null)
        {
            return null;
        }

        string directory = channelAnimation.isTemp ? SaveManager.TEMP_ANIMATIONS : SaveManager.GetChannelSaveFolder(channelAnimation.type);
        directory += channelAnimation.GetImportedFileName();

        return File.Exists(directory) ? directory : null;
    }
    
    public string GetThemeFilePath(Theme theme)
    {
        if (theme == null)
        {
            return null;
        }

        string directory = SaveManager.SAVE_FOLDER_THEMES + theme.GetImportedFileName();
        return File.Exists(directory) ? directory : null;
    }

    public void ClickedSelectBanner()
    {
        animationNewUploadView.ClickedSelectAnimation(CHANNELTYPE.BANNER);
    }

    public void ClickedSelectIcon()
    {
        animationNewUploadView.ClickedSelectAnimation(CHANNELTYPE.ICON);
    }
    
    public void ClickedSelectTheme()
    {
        animationNewUploadView.ClickedSelectTheme();
    }

    public CHANNELTYPE GetSelectionType()
    {
        return animationNewUploadView.selectionChannelType;
    }

    public void OnSelectedAnimation(ChannelAnimation channelAnimation)
    {
        animationNewUploadView.OnSelectedAnimation(channelAnimation);
    }
    
    public void OnSelectedTheme(Theme theme)
    {
        animationNewUploadView.OnSelectedTheme(theme);
    }

    public void ClickedThumbnailBack()
    {
        animationNewUploadView.ClickedThumbnailBack();
    }

    public void ClickedThumbnailScreenshot()
    { 
        animationNewUploadView.ClickedThumbnailScreenshot();
    }

    public void ClickedAcceptScreenshot()
    {
        animationNewUploadView.ClickedAcceptScreenshot();
    }

    public void ClickedRetryScreenshot()
    {
        animationNewUploadView.ClickedRetryScreenshot();
    }

    public async Task<bool> IsLoggedIn(bool sendActionMessage = true)
    {
        UnityWebRequest response = await _instance.SendRequest(WebRequestController.SendCheckLogin());
        if (!response.isNetworkError && !response.isHttpError)
        {
            return true;
        }

        if (sendActionMessage)
        {
            if (response.isNetworkError)
            {
                SendDownMessage();
            }
            else
            {
                SendNoAccountMessage();
            }
        }

        return false;
    }

    public void ClearViews()
    {
        foreach (ShopView view in GetViews())
        {
            view.Hide();
        }
    }

    public void ChangeBackButtonName(string newId = "ap.button.back")
    {
        TranslatedText button = backButton.transform.Find("TitleHolder").Find("Title").gameObject.GetComponent<TranslatedText>();
        button.id = newId;
        button.Refresh();
    }

    public static string GetUserId()
    {
        return _instance.shouldUseDebug ? _instance.debugId : PREFS.UserId.GetString();
    }

    public static string GetUserAuth()
    {
        string auth = _instance.shouldUseDebug ? _instance.debugAuth : PREFS.UserAuth.GetString();
        return string.IsNullOrEmpty(auth) ? auth : auth.FromBase64();
    }

    public static string GetUserName()
    {
        return _instance.shouldUseDebug ? _instance.debugUsername : PREFS.UserName.GetString();
    }

    private void SetContentPivot(float pivot)
    {
        optionsHolder.pivot = new Vector2(optionsHolder.pivot.x, pivot);
    }

    public void ClickedBack()
    {
        List<ShopView> activeViews = GetActiveViews();
        foreach (ShopView shopView in activeViews)
        {
            shopView.ClickedBack();
        }
    }

    public void ClickedConfirm()
    {
        List<ShopView> activeViews = GetActiveViews();
        foreach (ShopView shopView in activeViews)
        {
            shopView.ClickedConfirm();
        }
    }

    public void ClickedBrowse()
    {
        browseShopView.Show();
    }

    public void ClickedSearch()
    {
        searchCategoriesView.ClickedSearch();
    }

    public void ClickedFilters()
    {
        browseFiltersView.Show(searchCategoriesView.contentType);
    }

    public void ClickedDownload()
    {
        browseContentView.ClickedDownload();
    }

    public void ClickedAuthor()
    {
        browseContentView.ClickedAuthor();
    }

    public void ClickedPreviewIcon()
    {
        browseContentView.ClickedPreviewIcon();
    }

    public void ClickedPreviewBanner()
    {
        browseContentView.ClickedPreviewBanner();
    }

    public void ClickedLeftArrow()
    {
        if (browseQueryView.GetView().activeInHierarchy)
        {
            browseQueryView.ClickedLeftArrow();
        }
        else if (ratingsView.GetView().activeInHierarchy)
        {
            ratingsView.ClickedLeftArrow();
        }
    }

    public void ClickedRightArrow()
    {
        if (browseQueryView.GetView().activeInHierarchy)
        {
            browseQueryView.ClickedRightArrow();
        }
        else if (ratingsView.GetView().activeInHierarchy)
        {
            ratingsView.ClickedRightArrow();
        }
    }

    public void ClickedHeart()
    {
        browseContentView.ClickedHeart();
    }

    public void ClickedUninstall()
    {
        browseContentView.ClickedUninstall();
    }

    public void ClickedDelete()
    {
        browseContentView.ClickedDelete();
    }

    public void ClickedUpdateView()
    {
        if (!IsHandlingRequest())
        {
            updateView.Show();
        }
    }

    public void ClickedUpdateMine()
    {
        updateView.ClickedUpdateMine();
    }

    public void ClickedUpdateAll()
    {
        updateView.ClickedUpdateAll();
    }

    public void ClickedRatings()
    {
        browseContentView.ClickedRating();
    }

    public void ClickedAddReview()
    {
        ratingsView.ClickedRating();
    }

    public void ClickedDeleteReview()
    {
        addRatingsView.ClickedDeleteReview();
    }

    public void ClickedSendReview()
    {
        addRatingsView.ClickedSendReview();
    }

    public void ClickedDescription()
    {
        browseContentView.ClickedDescription();
    }

    private List<ShopView> GetActiveViews()
    {
        return GetViews().Where(views => views.IsActive()).ToList();
    }
    
    private void ShowLoadingCircle()
    {
        loadingCircles.SetActive(true);
    }

    private void HideLoadingCircle()
    {
        loadingCircles.SetActive(false);
    }

    public bool IsHandlingRequest()
    {
        return isHandlingRequest;
    }

    private IEnumerator _PlayLoadingSound(Task task)
    {
        AudioController.PlayShopLoadingAudio();
        yield return new WaitUntil(() => task.IsCompleted);
        AudioController.StopShopLoadingAudio();
    }

    public async Task<T> SendRequest<T>(Task<T> request, bool showCircle = true, bool playSound = true)
    {
        T task = default(T);
        try
        {
            isHandlingRequest = true;
            if (showCircle)
            {
                ShowLoadingCircle();
            }
            if (playSound)
            {
                StartCoroutine(_PlayLoadingSound(request));
            }
            task = await request;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        
        // we disable the request
        isHandlingRequest = false;
        if (showCircle)
        {
            HideLoadingCircle();
        }
        return task;
    }

    public async Task<bool> TryDownloadAnimation(string animationId, int animationBundle, bool hasIcon, bool hasBanner, bool isMainDownload = false, Transform prefab = null)
    {
        if (animationId == null || _instance.IsHandlingRequest())
        {
            return false;
        }

        // First we check if you've already downloaded both
        string downloadedIcon = SaveManager.SAVE_FOLDER_ANIMATIONS_ICONS + $"icon_{animationId}.zip";
        string downloadedBanner = SaveManager.SAVE_FOLDER_ANIMATIONS_BANNERS + $"banner_{animationId}.zip";
        bool needsDownloadIcon = !File.Exists(downloadedIcon) && hasIcon;
        bool needsDownloadBanner = !File.Exists(downloadedBanner) && hasBanner;

        if (!needsDownloadBanner && !needsDownloadIcon)
        {
            // We have both banner and icon downloaded
            // However, they might be outdated, so we load them to check their bundle
            OnlineInfo iconInfo = ChannelController._instance.GetOnlineInfo($"icon_{animationId}.zip", true, false);
            OnlineInfo bannerInfo = ChannelController._instance.GetOnlineInfo($"banner_{animationId}.zip", true, false);
            int bundle = animationBundle;
            if (iconInfo != null && iconInfo.bundle < bundle)
            {
                bundle = iconInfo.bundle;
            }

            if (bannerInfo != null && bannerInfo.bundle < bundle)
            {
                bundle = bannerInfo.bundle;
            }

            if (bundle >= animationBundle)
            {
                // Animation updated, no need to do anything
                if (isMainDownload)
                {
                    PopupController.ShowPopup("popup.alreadydownloaded");
                }
                return false;
            }
            else
            {
                // We need to download them after all
                needsDownloadBanner = hasBanner;
                needsDownloadIcon = hasIcon;
            }
        }

        // Now we try to find them in our temp files (in case you previewed it)
        string tempIcon = SaveManager.TEMP_ANIMATIONS + $"temp_icon_{animationId}.zip";
        string tempBanner = SaveManager.TEMP_ANIMATIONS + $"temp_banner_{animationId}.zip";
        if (File.Exists(tempIcon))
        {
            // We check if it's outdated
            OnlineInfo iconInfo = ChannelController._instance.GetOnlineInfo($"temp_icon_{animationId}.zip", false, true);
            if (iconInfo.bundle >= animationBundle)
            {
                Debug.Log("Retrieving icon from temp files!");
                ChannelController._instance.ImportTempChannelAnimation(animationId, CHANNELTYPE.ICON);
                needsDownloadIcon = false;
            }
        }

        if (File.Exists(tempBanner))
        {
            // We check if it's outdated
            OnlineInfo bannerInfo =
                ChannelController._instance.GetOnlineInfo($"temp_banner_{animationId}.zip", false, true);
            if (bannerInfo.bundle >= animationBundle)
            {
                Debug.Log("Retrieving banner from temp files!");
                ChannelController._instance.ImportTempChannelAnimation(animationId, CHANNELTYPE.BANNER);
                needsDownloadBanner = false;
            }
        }

        // And finally we download the animations we need, in case we need them
        bool foundIcon = true;
        bool foundBanner = true;
        if (needsDownloadIcon)
        {
            foundIcon = await DownloadAnimation(CHANNELTYPE.ICON, animationId);
        }

        if (needsDownloadBanner)
        {
            foundBanner = await DownloadAnimation(CHANNELTYPE.BANNER, animationId);
        }

        if ((hasIcon && foundIcon) || (hasBanner && foundBanner))
        {
            if (isMainDownload)
            {
                browseContentView.RecolorInstalledListButton(prefab);
                if (await IsLoggedIn(false))
                {
                    await _instance.SendRequest(WebRequestController.SendIncreaseDownloadRegistered(ContentType.ANIMATIONS, animationId));
                }
                else
                {
                    await _instance.SendRequest(WebRequestController.SendIncreaseDownloadAnon(ContentType.ANIMATIONS, animationId));
                }
                PopupController.ShowPopup("popup.downloadedanim", ClickedBack);
            }
            return true;
        }
        else if(isMainDownload)
        {
            // Since we don't know whether it's our fault, or the animation missing one of the anims, we return a more generic message
            PopupController.ShowPopup("popup.finished", ClickedBack);
        }
        return false;
    }

    public async Task<bool> DownloadAnimation(CHANNELTYPE type, string animationId, bool isTemp = false)
    {
        UnityWebRequest response = await _instance.SendRequest(WebRequestController.SendDownloadAnimation(animationId, type));
        if (!response.isNetworkError && !response.isHttpError)
        {
            byte[] results = response.downloadHandler.data;
            string finalPath;
            if (isTemp)
            {
                finalPath = SaveManager.TEMP_ANIMATIONS;
            }
            else
            {
                finalPath = SaveManager.GetChannelSaveFolder(type);
            }
            if (!Directory.Exists(finalPath))
            {
                Directory.CreateDirectory(finalPath);
            }
            if (isTemp)
            {
                finalPath += "temp_";
            }
            finalPath += $"{type.ToString().ToLowerInvariant()}_{animationId}.zip";

            bool shouldRefresh = false;
            if (File.Exists(finalPath))
            {
                shouldRefresh = true;
                File.Delete(finalPath);
            }
            File.WriteAllBytes(finalPath,results);
            if (shouldRefresh)
            {
                ChannelController._instance.RefreshAnimation(FileManager.GetFileName(finalPath.Replace("temp_","")), isTemp, type);
            }
            return true;
        }
        return false;
    }
    
    public async Task<bool> TryDownloadTheme(string animationId, int animationBundle, bool isMainDownload = false, Transform prefab = null)
    {
        if (animationId == null || _instance.IsHandlingRequest())
        {
            return false;
        }

        // First we check if you've already downloaded both
        string isDownloaded = SaveManager.SAVE_FOLDER_THEMES + $"theme_{animationId}.zip";
        bool needsDownloadIcon = !File.Exists(isDownloaded);

        if (!needsDownloadIcon)
        {
            // We have both banner and icon downloaded
            // However, they might be outdated, so we load them to check their bundle
            OnlineInfo iconInfo = ThemeController._instance.GetOnlineInfo($"theme_{animationId}.zip");
            int bundle = animationBundle;
            if (iconInfo != null && iconInfo.bundle < bundle)
            {
                bundle = iconInfo.bundle;
            }

            if (bundle >= animationBundle)
            {
                // Animation updated, no need to do anything
                if (isMainDownload)
                {
                    PopupController.ShowPopup("popup.alreadydownloadedtheme");
                }
                return false;
            }
        }

        // And finally we download the animations we need, in case we need them
        bool foundIcon = await DownloadTheme(animationId);

        if (foundIcon)
        {
            if (isMainDownload)
            {
                browseContentView.RecolorInstalledListButton(prefab);
                if (await IsLoggedIn(false))
                {
                    await _instance.SendRequest(WebRequestController.SendIncreaseDownloadRegistered(ContentType.THEMES, animationId));
                }
                else
                {
                    await _instance.SendRequest(WebRequestController.SendIncreaseDownloadAnon(ContentType.THEMES, animationId));
                }
                PopupController.ShowPopup("popup.downloadedtheme", ClickedBack);
            }
            return true;
        }
        else if(isMainDownload)
        {
            // Since we don't know whether it's our fault, or the animation missing one of the anims, we return a more generic message
            PopupController.ShowPopup("popup.finished", ClickedBack);
        }
        return false;
    }
    
    public async Task<bool> DownloadTheme(string animationId)
    {
        UnityWebRequest response = await _instance.SendRequest(WebRequestController.SendDownloadTheme(animationId));
        if (!response.isNetworkError && !response.isHttpError)
        {
            byte[] results = response.downloadHandler.data;
            string finalPath = SaveManager.SAVE_FOLDER_THEMES;
            if (!Directory.Exists(finalPath))
            {
                Directory.CreateDirectory(finalPath);
            }
            finalPath += $"theme_{animationId}.zip";

            bool shouldRefresh = false;
            if (File.Exists(finalPath))
            {
                shouldRefresh = true;
                File.Delete(finalPath);
            }
            File.WriteAllBytes(finalPath,results);
            if (shouldRefresh)
            {
                ThemeController._instance.RefreshTheme(FileManager.GetFileName(finalPath));
            }
            return true;
        }
        return false;
    }

    public void ShowScreenshot(Texture tex)
    {
        if (tex is null || tex.Equals(_instance.tempThumbnail))
        {
            return;
        }
        
        screenshotHolder.SetActive(true);
        screenshot.texture = tex;
        screenshot.SetNativeSize();

        float width = screenshot.texture.width;
        float height = screenshot.texture.height;
        float myWidth = Screen.width - (Screen.width * borderOffset);
        float myHeight = Screen.height - (Screen.height * borderOffset);
        
        // first we try to reduce the height
        float widthConversion = myWidth / width;
        float testHeight = height * widthConversion;
        Vector2 size;
        if (testHeight < myHeight)
        {
            // it fits in our screen, we apply
            size = new Vector2(myWidth, testHeight);
        }
        else
        {
            // it doesn't fit, so we reduce by width
            float heightConversion = myHeight / height;
            float testWidth = width * heightConversion;
            size = new Vector2(testWidth, myHeight);
        }

        screenshot.rectTransform.sizeDelta = size;
    }

    public void HideScreenshot()
    {
        screenshotHolder.SetActive(false);
        screenshot.texture = null;
        screenshot.rectTransform.sizeDelta = Vector2.zero;
    }
}