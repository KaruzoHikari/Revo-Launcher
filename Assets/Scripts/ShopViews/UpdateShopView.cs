using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Animations;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace ShopViews
{
    public class UpdateShopView : ShopView
    {
        public UpdateShopView(GameObject view) : base(view, () => { ShopController._instance.mainView.Show();})
        {
        }

        public void ClickedUpdateMine()
        {
            if (_instance.IsHandlingRequest())
            {
                return;
            }
            
            // We update animations that are currently loaded by the app
            List<OnlineInfo> animationIds = new List<OnlineInfo>();
            foreach (string anim in ChannelController._instance.GetUsedChannelAnimations())
            {
                OnlineInfo info = ChannelController._instance.GetOnlineInfo(anim,true,false);
                if (info != null)
                {
                    animationIds.Add(info);
                }
            }
            List<OnlineInfo> themeIds = new List<OnlineInfo>();
            if (ThemeController._instance.currentTheme.IsOnlineTheme())
            {
                themeIds.Add(ThemeController._instance.currentTheme.onlineInfo);
            }
            
            Update(animationIds, themeIds);
        }

        public void ClickedUpdateAll()
        {
            if (_instance.IsHandlingRequest())
            {
                return;
            }
            
            Update(ChannelController._instance.GetAllOnlineInfos(), ThemeController._instance.GetAllOnlineInfos());
        }

        private async void Update(List<OnlineInfo> animationInfos, List<OnlineInfo> themeInfos)
        {
            int updatedAnimations = 0;
            int updatedThemes = 0;
            foreach (OnlineInfo info in animationInfos)
            {
                UnityWebRequest response = await _instance.SendRequest(WebRequestController.SendGetContent(ContentType.ANIMATIONS, info.animationId));
                JObject json = response.GetJsonResponse();
                if (!response.isNetworkError && !response.isHttpError)
                {
                    // In this case the query was successful
                    int animationBundle = (int)json["bundle"];
                    bool hasIcon = (bool)json["hasIcon"];
                    bool hasBanner = (bool)json["hasBanner"];
                    bool success = await _instance.TryDownloadAnimation(info.animationId,animationBundle,hasIcon,hasBanner);
                    if (success)
                    {
                        updatedAnimations++;
                    }
                }
            }
            
            foreach (OnlineInfo info in themeInfos)
            {
                UnityWebRequest response = await _instance.SendRequest(WebRequestController.SendGetContent(ContentType.THEMES, info.animationId));
                JObject json = response.GetJsonResponse();
                if (!response.isNetworkError && !response.isHttpError)
                {
                    // In this case the query was successful
                    int animationBundle = (int)json["bundle"];
                    bool success = await _instance.TryDownloadTheme(info.animationId,animationBundle);
                    if (success)
                    {
                        updatedThemes++;
                    }
                }
            }

            if (updatedAnimations > 0 || updatedThemes > 0)
            {
                PopupController.ShowPopup("popup.updatedanims", replacementArray: new[] {updatedAnimations.ToString(), updatedThemes.ToString()});
            }
            else
            {
                PopupController.ShowPopup("popup.noupdatedanims");
            }
        }
    }
}