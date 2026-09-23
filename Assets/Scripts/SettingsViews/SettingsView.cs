using System.Globalization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
    public class SettingsView
    {
        protected SettingsController.SETTINGSMENU menu;
        protected SettingsController _instance;
        protected bool showCreateButton;
        protected string title;
        protected ThemeColor background;

        public SettingsView(SettingsController.SETTINGSMENU menu, string title, ThemeColor background, bool showCreateButton = false)
        {
            this.menu = menu;
            this.showCreateButton = showCreateButton;
            this.title = title;
            this.background = background;
        }

        public virtual void Show()
        {
            _instance = SettingsController._instance;
            _instance.ClearOtherMenus(menu);
            _instance.settingsView.SetActive(true);
            _instance.ClearActiveList();
            _instance.currentMenu = this;
            _instance.createButton.gameObject.SetActive(showCreateButton);
            CameraController.LockHorizontalMode();
            
            SetTitle(title);
            SetBackgroundColor(background);
        }

        public virtual void ClickedBack()
        {
            // to override
        }

        public virtual void ClickedCreate()
        {
            // to override
        }
        
        protected void SetTitle(string title)
        {
            _instance.title.text = TextController.GetTranslation(title);
        }

        protected void EnableMainGameObject(GameObject obj)
        {
            _instance.EnableMainGameObject(obj);
        }

        protected void SetBackgroundColor(ThemeColor themeColor)
        {
            _instance.SetBackgroundColor(themeColor);
        }

        protected void SetContentPivot(float pivot)
        {
            _instance.SetContentPivot(pivot);
        }

        public SettingsController.SETTINGSMENU GetMenu()
        {
            return menu;
        }
    }
}