using System.Collections.Generic;
using System.Globalization;
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
    public class QueryFiltersView : ShopView
    {
        private RectTransform content;
        private RectTransform contentThemes;
        private bool hasSetupFilters = false;
        public List<QueryFilter> filterList = new List<QueryFilter>();
        private ContentType contentType;

        public QueryFiltersView(GameObject view) : base(view, () => { ShopController._instance.searchCategoriesView.Show(); })
        {
            content = view.transform.Find("Content").Find("AnimFilters").GetComponent<RectTransform>();
            contentThemes = view.transform.Find("Content").Find("ThemeFilters").GetComponent<RectTransform>();
        }

        public void Show(ContentType type)
        {
            base.Show();
            this.contentType = type;
            
            if (!hasSetupFilters)
            {
                hasSetupFilters = true;
                SetupFilters();
            }
            
            content.gameObject.SetActive(type == ContentType.ANIMATIONS);
            contentThemes.gameObject.SetActive(type == ContentType.THEMES);
        }

        private void SetupFilters()
        {
            SetupFilter("Static", "staticFilter", ContentType.ANIMATIONS);
            SetupFilter("App Icon", "iconFilter", ContentType.ANIMATIONS);
            SetupFilter("Video", "videoFilter", ContentType.ANIMATIONS);
            SetupFilter("Color", "colorFilter", ContentType.THEMES);
            SetupFilter("Texture", "textureFilter", ContentType.THEMES);
            SetupFilter("Audio", "audioFilter", ContentType.THEMES);
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentThemes);
        }

        private void SetupFilter(string name, string id, ContentType type)
        {
            GameObject filter = GameObject.Instantiate(_instance.filterPrefab, type == ContentType.ANIMATIONS ? content : contentThemes);
            QueryFilter queryFilter = new QueryFilter(name, id, filter, type);
            filterList.Add(queryFilter);
        }
    }
}