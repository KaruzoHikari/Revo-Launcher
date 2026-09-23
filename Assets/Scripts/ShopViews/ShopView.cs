using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ShopViews
{
    public class ShopView
    {
        protected GameObject view;
        protected UnityAction backButtonCallback;
        protected UnityAction confirmButtonCallback;
        protected bool clearOtherViews;
        protected bool isBackButtonNamedExit;
        protected ShopController _instance;

        public ShopView(GameObject view, UnityAction backButtonCallback, UnityAction confirmButtonCallback = null,
            bool clearOtherViews = true, bool isBackButtonNamedExit = false)
        {
            this.view = view;
            this.backButtonCallback = backButtonCallback;
            this.confirmButtonCallback = confirmButtonCallback;
            this.clearOtherViews = clearOtherViews;
            this.isBackButtonNamedExit = isBackButtonNamedExit;
            _instance = ShopController._instance;
        }

        public virtual void Show()
        {
            if (clearOtherViews)
            {
                _instance.ClearViews();
            }

            view.SetActive(true);
            _instance.ChangeBackButtonName(isBackButtonNamedExit ? "ap.button.quit" : "ap.button.back");
            _instance.backButton.SetActive(backButtonCallback != null);
            _instance.confirmButton.SetActive(confirmButtonCallback != null);
        }

        public virtual void ClickedBack()
        {
            if (!_instance.IsHandlingRequest())
            {
                backButtonCallback?.Invoke();
            }
        }

        public void ClickedConfirm()
        {
            if (!_instance.IsHandlingRequest())
            {
                confirmButtonCallback?.Invoke();
            }
        }

        public virtual void Hide()
        {
            view.SetActive(false);
        }

        public bool IsActive()
        {
            return view.activeInHierarchy;
        }

        public GameObject GetView()
        {
            return view;
        }
        
        protected string FormatDownloads(int downloads)
        {
            if (downloads < 1000)
            {
                return downloads.ToString();
            }

            float number = downloads / 1000f;
            number = ((int) (number * 10)) / 10f;
            return number.ToString(CultureInfo.InvariantCulture) + "k";
        }
        
        public void RecolorInstalledListButton(Transform prefab)
        {
            if (prefab is null)
            {
                return;
            }
            
            RecolorButton(prefab.GetComponent<Button>(), ThemeColor.HubAnimationListInstalled);
            //RecolorButtonOld(button, _instance.installedColor, _instance.installedHoverColor, _instance.installedSelectedColor, _instance.installedClickColor);
        }
        
        protected void RecolorUninstallListButton(Transform prefab)
        {
            if (prefab is null)
            {
                return;
            }
            
            RecolorButton(prefab.GetComponent<Button>(), ThemeColor.HubAnimationListUninstalled);
            //RecolorButtonOld(button, _instance.uninstalledColor, _instance.uninstalledHoverColor, _instance.uninstalledSelectedColor, _instance.uninstalledClickColor);
        }

        private void RecolorButtonOld(Button button, Color normal, Color highlight, Color selected, Color pressed)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = normal;
            colors.highlightedColor = highlight;
            colors.selectedColor = selected;
            colors.pressedColor = pressed;
            button.colors = colors;
        }

        private void RecolorButton(Button button, ThemeColor color)
        {
            if (button is not null && button.targetGraphic is not null)
            {
                ThemedCutCorners theme = button.GetComponent<ThemedCutCorners>();
                theme.themeColor = color;
                theme.UpdateElement();
            }
        }
    }
}