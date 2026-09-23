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
    public class HubSettingsView : ShopView
    {
        private RectTransform content;
        private bool hasDoneFirstSetup = false;
        private TMP_InputField altHub;
        private Button logoutButton;

        public HubSettingsView(GameObject view) : base(view, () => { ShopController._instance.mainView.Show();})
        {
            content = view.transform.Find("Content").GetComponent<RectTransform>();
            altHub = content.Find("Althub").Find("Vector").Find("Text").GetComponent<TMP_InputField>();
            logoutButton = content.Find("Logout").Find("Button").GetComponent<Button>();
        }

        public override void Show()
        {
            base.Show();

            if (!hasDoneFirstSetup)
            {
                hasDoneFirstSetup = true;

                // we setup the circle
                Option<bool> circleOption = Option.Create("ap.title.rotatecircle",
                    () => ShopController._instance.rotateCircle, x =>
                    {
                        PREFS.RotateLoadingCircle.SetBool(x);
                        ShopController._instance.UpdateRotateCircle();
                    });
                GameObject spawned = OptionsSpawner.SummonOption_Boolean(circleOption, this.content, _instance.onlyFavoritesPrefab);
                Transform parent = spawned.transform.parent;
                spawned.transform.SetSiblingIndex(parent.childCount - 2);

                // we setup the timeout option
                Option<int> timeoutOption = Option.Create("ap.title.timeout", () => WebRequestController._instance.downloadTimeout,
                    x =>
                    {
                        if (x <= 0) return;
                        PREFS.DownloadTimeout.SetInt(x);
                        WebRequestController._instance.UpdateDownloadTimeout();
                    });
                GameObject spawnedTimeout = OptionsSpawner.SummonOption_Int(timeoutOption, this.content, _instance.hubPreferencesIntPrefab);
                spawnedTimeout.transform.SetSiblingIndex(parent.childCount - 2);
                LayoutRebuilder.ForceRebuildLayoutImmediate(parent.GetComponent<RectTransform>());

                // logout button
                logoutButton.onClick.AddListener(ClickedLogout);

                // althub url
                altHub.onValueChanged.AddListener(value =>
                {
                    // we logout when changing the althub URL! we don't want people to leak their main Hub's token around
                    WebRequestController._instance.Logout();
                    
                    PREFS.AltHubUrl.SetString(string.IsNullOrEmpty(value) ? null : value);
                    WebRequestController._instance.UpdateAltHost();
                });
                altHub.text = PREFS.AltHubUrl.GetString();
            }
        }

        public override void ClickedBack()
        {
            ShopController._instance.mainView.Show();
        }

        private void ClickedLogout()
        {
            string id = PREFS.UserId.GetString();
            if (string.IsNullOrEmpty(id))
            {
                PopupController.ShowPopup("popup.logoutnoaccount");
            }
            else
            {
                PopupController.ShowPopup("popup.shouldlogout", () =>
                {
                    WebRequestController._instance.Logout();
                }, PopupController.ClosePopup);
            }
        }
    }
}