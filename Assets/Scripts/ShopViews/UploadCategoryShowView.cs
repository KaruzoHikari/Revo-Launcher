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
    public class UploadCategoryShopView : ShopView
    {
        public ContentType contentType;
        private TextMeshProUGUI uploadText;
        private TextMeshProUGUI updateText;
        
        public UploadCategoryShopView(GameObject view) : base(view, () => { ShopController._instance.browseUploadShopView.Show();})
        {
            uploadText = view.transform.Find("Content").Find("Upload").Find("TextHolder").Find("Title").GetComponent<TextMeshProUGUI>();
            updateText = view.transform.Find("Content").Find("Update").Find("TextHolder").Find("Title").GetComponent<TextMeshProUGUI>();
        }

        public void Show(ContentType content)
        {
            base.Show();
            contentType = content;
            uploadText.text = TextController.GetTranslation("ap.title.upload" + (content == ContentType.THEMES ? "theme" : "anim"));
            updateText.text = TextController.GetTranslation("ap.title.update" + (content == ContentType.THEMES ? "theme" : "anim"));
        }

        public void ClickedUpload()
        {
            if (contentType == ContentType.THEMES && ThemeController._instance.IsOpen())
            {
                PopupController.ShowPopup("popup.nouploadtheme");
            }
            else
            {
                _instance.animationNewUploadView.Show(contentType, true);
            }
        }

        public void ClickedUpdate()
        {
            if (contentType == ContentType.THEMES && ThemeController._instance.IsOpen())
            {
                PopupController.ShowPopup("popup.noupdatetheme");
            }
            else
            {
                // We need to check which content we want to update from the query menu
                _instance.browseQueryView.Show(SearchMode.MY_CONTENT, contentType, overrideGuidSearch:ShopController.GetUserId());
            }
        }
    }
}