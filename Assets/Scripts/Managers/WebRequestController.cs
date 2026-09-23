using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Animations;
using Misc;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

public class WebRequestController : MonoBehaviour
{
    public static WebRequestController _instance;
    public string host = "localhost:5001";
    public string altHost = null;
    public static string networkErrorString = "Error while sending to server.";
    public int maxNumberOfThumbnailRequests = 2;
    private int numberOfThumbnailRequests = 0;
    public int downloadTimeout = 50;

    void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        UpdateAltHost();
        UpdateDownloadTimeout();
    }

    public void UpdateAltHost()
    {
        altHost = PREFS.AltHubUrl.GetString();
    }

    public void UpdateDownloadTimeout()
    {
        downloadTimeout = PREFS.DownloadTimeout.GetInt();
    }

    public void Logout()
    {
        // we reset everything
        PREFS.UserId.Reset();
        PREFS.UserAuth.Reset();
        PREFS.UserName.Reset();
    }

    public static string GetUrl(string target)
    {
        return (string.IsNullOrEmpty(_instance.altHost) ? _instance.host : _instance.altHost) + target;
    }

    private static Dictionary<string,string> GetAuthHeader()
    {
        return new Dictionary<string, string> 
        {
            ["UserId"] = ShopController.GetUserId() ?? "",
            ["UserAuth"] = ShopController.GetUserAuth() ?? ""
        };
    }

    public static async Task<UnityWebRequest> SendCreateUser(string requestName, string requestEmail)
    {
        string json = JObject.FromObject(new
        {
            username = requestName,
            email = requestEmail
        }).ToString();

        return await SendJsonWebRequest(RequestType.POST, "/users/create", json);
    }

    public static async Task<UnityWebRequest> SendLoginUser(string requestEmail)
    {
        string json = JObject.FromObject(new
        {
            email = requestEmail
        }).ToString();

        return await SendJsonWebRequest(RequestType.POST, "/users/login", json);
    }

    public static async Task<UnityWebRequest> SendAuthUser(string requestedCode)
    {
        string json = JObject.FromObject(new
        {
            emailCode = requestedCode
        }).ToString();

        return await SendJsonWebRequest(RequestType.POST, $"/users/auth/{ShopController.GetUserId()}", json);
    }

    public static async Task<UnityWebRequest> SendCheckLogin()
    {
        return await SendEmptyWebRequest(RequestType.GET, $"/users/checklogin", GetAuthHeader());
    }

    public static async Task<UnityWebRequest> SendCreateAnimation(string requestName, string requestDescription,
        string requestBannerPath, string requestBannerThumbnailPath, string requestIconPath, string requestIconThumbnailPath, string updateId = null)
    {
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        formData.Add(new MultipartFormDataSection("Name",requestName));
        formData.Add(new MultipartFormDataSection("Description",requestDescription));
        if (!string.IsNullOrEmpty(updateId))
        {
            formData.Add(new MultipartFormDataSection("OnlineAnimationId",updateId));
        }
        if (!string.IsNullOrEmpty(requestIconPath))
        {
            formData.Add(new MultipartFormFileSection("IconFile", File.ReadAllBytes(requestIconPath), FileManager.GetFileName(requestIconPath), "application/zip"));
            formData.Add(new MultipartFormFileSection("IconThumbnail", File.ReadAllBytes(requestIconThumbnailPath), FileManager.GetFileName(requestIconThumbnailPath), "image/png"));
        }
        if (!string.IsNullOrEmpty(requestBannerPath))
        {
            formData.Add(new MultipartFormFileSection("BannerFile", File.ReadAllBytes(requestBannerPath), FileManager.GetFileName(requestBannerPath), "application/zip"));
            formData.Add(new MultipartFormFileSection("BannerThumbnail", File.ReadAllBytes(requestBannerThumbnailPath), FileManager.GetFileName(requestBannerThumbnailPath), "image/png"));
        }

        return await SendFormWebRequest("/animations/" + (string.IsNullOrEmpty(updateId) ? "create" : "update"), formData, GetAuthHeader());
    }
    
    public static async Task<UnityWebRequest> SendCreateTheme(string requestName, string requestDescription, string accentColor, string requestThemePath,
        string requestHomeThumbnailPath, string requestSettingsThumbnailPath, bool hasColors, bool hasTextures, bool hasAudio, string updateId = null)
    {
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        formData.Add(new MultipartFormDataSection("Name",requestName));
        formData.Add(new MultipartFormDataSection("Description",requestDescription));
        formData.Add(new MultipartFormDataSection("AccentColor",accentColor));
        formData.Add(new MultipartFormDataSection("ChangesColor", hasColors.ToString()));
        formData.Add(new MultipartFormDataSection("ChangesTexture", hasTextures.ToString()));
        formData.Add(new MultipartFormDataSection("ChangesAudio", hasAudio.ToString()));
        if (!string.IsNullOrEmpty(updateId))
        {
            formData.Add(new MultipartFormDataSection("OnlineAnimationId",updateId));
        }
        if (!string.IsNullOrEmpty(requestThemePath))
        {
            formData.Add(new MultipartFormFileSection("ThemeFile", File.ReadAllBytes(requestThemePath), FileManager.GetFileName(requestThemePath), "application/zip"));
            formData.Add(new MultipartFormFileSection("ThemeHomeThumbnail", File.ReadAllBytes(requestHomeThumbnailPath), FileManager.GetFileName(requestHomeThumbnailPath), "image/png"));
            formData.Add(new MultipartFormFileSection("ThemeSettingsThumbnail", File.ReadAllBytes(requestSettingsThumbnailPath), FileManager.GetFileName(requestSettingsThumbnailPath), "image/png"));
        }

        return await SendFormWebRequest("/themes/" + (string.IsNullOrEmpty(updateId) ? "create" : "update"), formData, GetAuthHeader());
    }

    public static async Task<UnityWebRequest> SendDownloadAnimation(string animationId, CHANNELTYPE channeltype)
    {
        return await SendEmptyWebRequest(RequestType.GET, $"/animations/download/{animationId}/{channeltype.ToString().ToLowerInvariant()}", GetAuthHeader(), _instance.downloadTimeout);
    }
    
    public static async Task<UnityWebRequest> SendDownloadTheme(string animationId)
    {
        return await SendEmptyWebRequest(RequestType.GET, $"/themes/download/{animationId}", GetAuthHeader(), _instance.downloadTimeout);
    }

    public static async Task<UnityWebRequest> SendIncreaseDownloadAnon(ContentType contentType, string animationId)
    {
        string origin = GetOrigin(contentType);
        return await SendEmptyWebRequest(RequestType.POST, $"/{origin}/increasedownload/anon/{animationId}");
    }

    public static async Task<UnityWebRequest> SendIncreaseDownloadRegistered(ContentType contentType, string animationId)
    {
        string origin = GetOrigin(contentType);
        return await SendEmptyWebRequest(RequestType.POST, $"/{origin}/increasedownload/registered/{animationId}", GetAuthHeader());
    }

    public static void SendDownloadThumbnail(string contentId, ContentType contentType, CHANNELTYPE channeltype, RawImage targetTexture, Texture2D replacement, UnityAction<Texture2D> action = null)
    {
        string origin = contentType == ContentType.ANIMATIONS ? "animations" : "themes";
        string id = contentType == ContentType.ANIMATIONS ? channeltype.ToString().ToLowerInvariant() : (channeltype == CHANNELTYPE.ICON ? "home" : "settings");
        string url = GetUrl($"/{origin}/thumbnail/{contentId}/{id}");
        _instance.StartCoroutine(_SendDownloadThumbnail(url, targetTexture, replacement,true,action));
    }

    private static IEnumerator _SendDownloadThumbnail(string url, RawImage targetTexture, Texture2D replacement, bool sendHeaders, UnityAction<Texture2D> action)
    {
        // We queue the image download requests so they don't appear all at once together
        yield return new WaitUntil(() => _instance.numberOfThumbnailRequests < _instance.maxNumberOfThumbnailRequests);
        _instance.numberOfThumbnailRequests++;
        yield return _SendDownloadImage(url, targetTexture, replacement, sendHeaders, action);
        _instance.numberOfThumbnailRequests--;
    }

    private static async Task _SendDownloadImage(string url, RawImage targetTexture, Texture2D replacement, bool sendHeaders, UnityAction<Texture2D> action, bool askThroughHttp = false)
    {
        using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(url, true))
        {
            if (sendHeaders)
            {
                Dictionary<string, string> headers = GetAuthHeader();
                foreach (string key in headers.Keys)
                {
                    req.SetRequestHeader(key, headers[key]);
                }
            }

            if (askThroughHttp)
            {
                // hopefully just temp from Retro
                req.certificateHandler = new CertificateHttp();
            }

            await req.SendWebRequest();
            
            if (!req.isHttpError && !req.isNetworkError)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(req);
                
                if (targetTexture != null)
                {
                    targetTexture.texture = texture;
                    targetTexture.gameObject.SetActive(true);
                }
                action?.Invoke(texture);
            }
            else
            {
                if (targetTexture != null)
                {
                    if (replacement is null)
                    {
                        targetTexture.gameObject.SetActive(false);
                    }
                    else
                    {
                        targetTexture.texture = replacement;
                        targetTexture.gameObject.SetActive(true);
                    }
                }

                action?.Invoke(null);
            }
        }
    }

    public static async Task<UnityWebRequest> SendSearchRequest(ContentType contentType, string search, string author, string sort, int page,
        List<QueryFilter> filters = null, string overrideId = null, bool overrideReview = false, bool onlyFavorites = false)
    {
        string query = $"sort={sort}";
        if (!string.IsNullOrEmpty(overrideId))
        {
            query += $"&authorId={overrideId}";
        }
        else if (overrideReview)
        {
            query += "&needsReview=1";
        }
        else
        {
            if (!string.IsNullOrEmpty(search))
            {
                query += $"&search={search}";
            }
            if (!string.IsNullOrEmpty(author))
            {
                query += $"&author={author}";
            }
            if (onlyFavorites)
            {
                query += "&onlyFavorites=1";
            }

            if (filters != null)
            {
                foreach (QueryFilter filter in filters)
                {
                    if (filter.GetContentType().Equals(contentType))
                    {
                        query += $"&{filter.id}={filter.GetStatus()}";
                    }
                }
            }
        }

        string origin = GetOrigin(contentType);
        return await SendEmptyWebRequest(RequestType.GET, $"/{origin}/search/{query}/{page}", GetAuthHeader());
    }

    private static string GetOrigin(ContentType type)
    {
        return type == ContentType.ANIMATIONS ? "animations" : "themes";
    }

    public static async Task<UnityWebRequest> SendGetContent(ContentType contentType, string id)
    {
        string origin = GetOrigin(contentType);
        return await SendEmptyWebRequest(RequestType.GET, $"/{origin}/get/{id}", GetAuthHeader());
    }

    public static async Task<UnityWebRequest> SendHeartAnimation(ContentType contentType, int target, string id)
    {
        string origin = GetOrigin(contentType);
        return await SendEmptyWebRequest(RequestType.POST, $"/{origin}/likes/{id}/{target}", GetAuthHeader());
    }

    public static async Task<UnityWebRequest> SendReviewAnimation(ContentType contentType, bool approve, string id)
    {
        string origin = GetOrigin(contentType);
        string endpoint = approve ? "approve" : "reject";
        return await SendEmptyWebRequest(RequestType.POST, $"/{origin}/{endpoint}/{id}", GetAuthHeader());
    }

    public static async Task<UnityWebRequest> SendDeleteAnimation(ContentType contentType, string id)
    {
        string origin = GetOrigin(contentType);
        return await SendEmptyWebRequest(RequestType.DELETE, $"/{origin}/delete/{id}", GetAuthHeader());
    }

    public static async Task<UnityWebRequest> SendGetReviews(ContentType contentType, string id, int page)
    {
        string origin = contentType == ContentType.ANIMATIONS ? "reviews" : "reviewsthemes";
        return await SendEmptyWebRequest(RequestType.GET, $"/{origin}/search/{id}/{page}", GetAuthHeader());
    }

    public static async Task<UnityWebRequest> SendGetFlaggedReviews(int page)
    {
        return await SendEmptyWebRequest(RequestType.GET, $"/reviews/reported/{page}", GetAuthHeader());
    }

    public static async Task<UnityWebRequest> SendApproveRating(string id)
    {
        return await SendEmptyWebRequest(RequestType.POST, $"/reviews/approve/{id}", GetAuthHeader());
    }

    public static async Task<UnityWebRequest> SendDeleteReview(string id)
    {
        return await SendEmptyWebRequest(RequestType.DELETE, $"/reviews/delete/{id}", GetAuthHeader());
    }

    public static async Task<UnityWebRequest> SendReportReview(string id)
    {
        return await SendEmptyWebRequest(RequestType.POST, $"/reviews/report/{id}", GetAuthHeader());
    }

    public static async Task<UnityWebRequest> SendGetReview(ContentType contentType, string animationId, string userId)
    {
        string origin = contentType == ContentType.ANIMATIONS ? "reviews" : "reviewsthemes";
        return await SendEmptyWebRequest(RequestType.GET, $"/{origin}/get/{animationId}/{userId}", GetAuthHeader());
    }

    public static async Task<UnityWebRequest> SendGetTranslationsInfo()
    {
        return await SendEmptyWebRequest(RequestType.GET,$"/translations/info", timeout: 5);
    }

    public static async Task<UnityWebRequest> SendDownloadTranslations()
    {
        return await SendEmptyWebRequest(RequestType.GET, "/translations/download");
    }

    public static async Task<string> SendCheckVersion()
    {
        UnityWebRequest check = await SendEmptyWebRequest(RequestType.GET,$"/general/version", timeout: 5);
        if (!check.isNetworkError && !check.isHttpError)
        {
            return (string)check.GetJsonResponse()["version"];
        }
        return null;
    }

    public static async Task<UserRank> SendGetRank()
    {
        UnityWebRequest rankCheck = await SendCheckLogin();
        if (!rankCheck.isNetworkError && !rankCheck.isHttpError)
        {
            return (UserRank)((int)rankCheck.GetJsonResponse()["rank"]);
        }
        return UserRank.USER;
    }

    public static async Task<UnityWebRequest> SendAddReview(ContentType contentType, string animationId, string description, int rating)
    {
        string origin = contentType == ContentType.ANIMATIONS ? "reviews" : "reviewsthemes";
        string json = JObject.FromObject(new
        {
            animationId,
            description,
            rating
        }).ToString();

        return await SendJsonWebRequest(RequestType.POST, $"/{origin}/create", json, GetAuthHeader());
    }

    public static async Task<UnityWebRequest> SendGetRating(ContentType contentType, string animationId)
    {
        string origin = contentType == ContentType.ANIMATIONS ? "reviews" : "reviewsthemes";
        return await SendEmptyWebRequest(RequestType.GET, $"/{origin}/rating/{animationId}", GetAuthHeader());
    }

    public static async Task<UnityWebRequest> SendGetMessages()
    {
        return await SendEmptyWebRequest(RequestType.GET, $"/messages/get/{ShopController.GetUserId()}", GetAuthHeader(), 8);
    }

    public static async Task<UnityWebRequest> SendGetVerifiedUsers()
    {
        return await SendEmptyWebRequest(RequestType.GET, $"/users/verifiedlist", GetAuthHeader(), 8);
    }

    public static async Task<UnityWebRequest> SendDeleteMessage(string id)
    {
        return await SendEmptyWebRequest(RequestType.DELETE, $"/messages/delete/{id}", GetAuthHeader());
    }

    private static async Task<UnityWebRequest> SendEmptyWebRequest(RequestType type, string target, Dictionary<string,string> headers = null, int timeout = 15)
    {
        return await SendJsonWebRequest(type, target, null, headers, timeout);
    }

    public static async Task<UnityWebRequest> SendJsonWebRequest(RequestType type, string target, string body = null, Dictionary<string,string> headers = null, int timeout = 15, bool overrideHost = false)
    {
        var req = new UnityWebRequest(overrideHost ? target : GetUrl(target), type.ToString());
        if (body != null)
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(body);
            req.uploadHandler = new UploadHandlerRaw(jsonToSend);
            req.SetRequestHeader("Content-Type", "application/json");
        }
        req.downloadHandler = new DownloadHandlerBuffer();
        req.timeout = timeout;
        return await HandleWebRequest(req,headers);
    }

    private static async Task<UnityWebRequest> SendFormWebRequest(string target, List<IMultipartFormSection> formData, Dictionary<string,string> headers = null)
    {
        UnityWebRequest req = UnityWebRequest.Post(GetUrl(target), formData);
        return await HandleWebRequest(req,headers);
    }

    private static async Task<UnityWebRequest> HandleWebRequest(UnityWebRequest request, Dictionary<string,string> headers = null)
    {
        if (headers != null)
        {
            try
            {
                foreach (string key in headers.Keys)
                {
                    request.SetRequestHeader(key,headers[key]);
                } 
            }
            catch (Exception e)
            {
                Debug.Log("There was an issue setting up headers! Sending no header");
                Debug.LogError(e);
            }
        }

        // Hotfix for people whose device throws a network error
        request.useHttpContinue = false;
        
        await request.SendWebRequest();

        if (request.isNetworkError)
        {
            Debug.Log($"Error while sending to server: " + request.error);
            return request;
        }

        Debug.Log($"Received from server: " + request.downloadHandler.text);
        return request;
    }

    public static async Task<Texture2D> SendDownloadCover(string url, bool askThroughHttp, UnityAction<Texture2D> action = null)
    {
        Texture2D texture = null;
        await _SendDownloadImage(url, null, null, false, tex =>
        {
            texture = tex;
            action?.Invoke(tex);
        }, askThroughHttp);
        return texture;
    }

    public static async Task<UnityWebRequest> SendDownloadText(string url, bool askThroughHttp = false)
    {
        var req = new UnityWebRequest(url, RequestType.GET.ToString());
        req.downloadHandler = new DownloadHandlerBuffer();
        req.timeout = 20;

        if (askThroughHttp)
        {
            // hopefully just temporary, LibRetro seems to have an issue with it rn
            req.certificateHandler = new CertificateHttp();
        }

        // Hotfix for people whose device throws a network error
        req.useHttpContinue = false;
        
        await req.SendWebRequest();

        if (req.isNetworkError)
        {
            Debug.Log("Error while sending to server: " + req.error);
            return req;
        }
        
        return req;
    }

    private class CertificateHttp : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }

    public enum RequestType
    {
        POST, GET, DELETE, PUT
    }
}

/// <summary>
/// Async awaitable UnityWebRequest <br/><br/>
/// Usage example: <br/><br/>
/// UnityWebRequest www = new UnityWebRequest(); <br/>
/// // do unitywebrequest setup here here... <br/>
/// await www.SendWebRequest(); <br/>
/// Debug.Log(req.downloadHandler.text); <br/>
/// </summary>
public struct UnityWebRequestAwaiter : INotifyCompletion
{
    private UnityWebRequestAsyncOperation asyncOp;
    private Action continuation;

    public UnityWebRequestAwaiter(UnityWebRequestAsyncOperation asyncOp)
    {
        this.asyncOp = asyncOp;
        continuation = null;
    }

    public bool IsCompleted { get { return asyncOp.isDone; } }

    public void GetResult() { }

    public void OnCompleted(Action continuation)
    {
        this.continuation = continuation;
        asyncOp.completed += OnRequestCompleted;
    }

    private void OnRequestCompleted(AsyncOperation obj)
    {
        continuation?.Invoke();
    }
}

public static class ExtensionMethods
{
    public static UnityWebRequestAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOp)
    {
        return new UnityWebRequestAwaiter(asyncOp);
    }

    public static JObject GetJsonResponse(this UnityWebRequest request)
    {
        try
        {
            return JObject.Parse(request.downloadHandler.text);
        }
        catch (Exception e)
        {
            return new JObject();
        }
    }
    
    public static JArray GetJsonArrayResponse(this UnityWebRequest request)
    {
        try
        {
            return JArray.Parse(request.downloadHandler.text);
        }
        catch (Exception e)
        {
            return new JArray();
        }
    }
}
