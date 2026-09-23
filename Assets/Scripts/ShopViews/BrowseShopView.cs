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
    public class BrowseShopView : ShopView
    {
        public BrowseShopView(GameObject view) : base(view, () => { ShopController._instance.mainView.Show();})
        {
        }

        public void ClickedBrowseAnimations()
        {
            _instance.searchCategoriesView.Show(ContentType.ANIMATIONS);
        }

        public void ClickedBrowseThemes()
        {
            _instance.searchCategoriesView.Show(ContentType.THEMES);
        }
    }
}