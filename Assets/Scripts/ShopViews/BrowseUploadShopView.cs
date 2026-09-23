using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
    public class BrowseUploadShopView : ShopView
    {
        public BrowseUploadShopView(GameObject view) : base(view, () => { ShopController._instance.mainView.Show();})
        {
        }

        public override void Show()
        {
            base.Show();
            EnterUploadMenu();
        }

        public void ClickedUploadAnimations()
        {
            _instance.uploadCategoryView.Show(ContentType.ANIMATIONS);
        }

        public void ClickedUploadThemes()
        {
            _instance.uploadCategoryView.Show(ContentType.THEMES);
        }
        
        public async void EnterUploadMenu()
        {
            // If the user is logged in, we go straight to the animation upload
            UnityWebRequest response = await _instance.SendRequest(WebRequestController.SendCheckLogin());
            if (!response.isNetworkError && !response.isHttpError)
            {
                bool areWeMod = (UserRank) ((int) response.GetJsonResponse()["rank"]) >= UserRank.MOD;
                _instance.reviewMenuButton.SetActive(areWeMod);
                _instance.ratingsReviewMenuButton.SetActive(areWeMod);
            }
            else
            {
                if (response.isNetworkError)
                {
                    // Servers are probably down
                    _instance.SendDownMessage();
                }
                else
                {
                    // Otherwise, we need to log in
                    _instance.SendNoAccountMessage(() => _instance.userAuthView.Show());
                }
            }
            _instance.ChangeBackButtonName();
        }
    }
}