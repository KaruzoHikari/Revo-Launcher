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
    public class SearchCategoriesView : ShopView
    {
        public ContentType contentType;
        public bool onlyFavorites = false;
        private RectTransform content;
        private bool hasSetupFavorites = false;
        private TextMeshProUGUI title;
        private TMP_InputField name;
        private TMP_InputField author;

        public SearchCategoriesView(GameObject view) : base(view, () => { ShopController._instance.browseShopView.Show(); })
        {
            content = view.transform.Find("Content").GetComponent<RectTransform>();
            title = content.Find("Title").Find("Title").GetComponent<TextMeshProUGUI>();
            name = content.Find("Search").Find("Vector").Find("Text").GetComponent<TMP_InputField>();
            author = content.Find("Author").Find("Vector").Find("Text").GetComponent<TMP_InputField>();
        }

        public override void Show()
        {
            base.Show();
            SetFiltersNumber();
        }

        public void Show(ContentType type)
        {
            base.Show();
            if (!contentType.Equals(type))
            {
                ResetElements();
            }
            
            contentType = type;
            title.text = TextController.GetTranslation(contentType == ContentType.ANIMATIONS ? "ap.title.findanims" : "ap.title.findthemes");
            
            if (!hasSetupFavorites)
            {
                hasSetupFavorites = true;
                Option<bool> option = Option.Create("ap.title.onlyfavs", () => onlyFavorites, x => onlyFavorites = x);
                GameObject spawned = OptionsSpawner.SummonOption_Boolean(option,this.content,_instance.onlyFavoritesPrefab);
                
                Transform parent = spawned.transform.parent;
                spawned.transform.SetSiblingIndex(parent.childCount-2);
                LayoutRebuilder.ForceRebuildLayoutImmediate(parent.GetComponent<RectTransform>());
            }

            SetFiltersNumber();
        }

        private void ResetElements()
        {
            name.text = null;
            author.text = null;
            foreach (QueryFilter filter in _instance.browseFiltersView.filterList)
            {
                filter.ClickedNeutral();
            }
        }

        private void SetFiltersNumber()
        {
            int number = 0;
            foreach (QueryFilter filter in _instance.browseFiltersView.filterList)
            {
                if (filter.GetContentType().Equals(contentType) &&  filter.status != FilterStatus.NEUTRAL)
                {
                    number++;
                }
            }

            _instance.filterNumberText.text = $"({number})";
        }

        public void ClickedSearch()
        {
            _instance.browseQueryView.Show(SearchMode.REGULAR, contentType);
        }
    }
}