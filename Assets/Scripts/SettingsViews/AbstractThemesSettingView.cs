using Misc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
    public abstract class AbstractThemesSettingView : SettingsView
    {
        public AbstractThemesSettingView(SettingsController.SETTINGSMENU menu, bool showCreate = false) : base(menu,
            "settings.title.themes", ThemeColor.SettingsBackgroundThemes, showCreate)
        {
        }

        public override void Show()
        {
            base.Show();
            EnableMainGameObject(_instance.selectionMenu);
        }

        public override void ClickedBack()
        {
            _instance.themesSettingView.Show();
        }
    }
}