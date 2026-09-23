using Animations;
using Misc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
    public abstract class AbstractChannelsSettingView : SettingsView
    {
        protected CHANNELTYPE channelType;
        
        public AbstractChannelsSettingView(SettingsController.SETTINGSMENU menu, bool showCreate = false) : base(menu,
            "", ThemeColor.SettingsBackgroundLight, showCreate)
        {
        }

        public override void Show()
        {
            base.Show();
            SetTitle(channelType == CHANNELTYPE.ICON ? "settings.title.icons" : "settings.title.banners");
            EnableMainGameObject(_instance.selectionMenu);
        }

        public virtual void Show(CHANNELTYPE type)
        {
            channelType = type;
            Show();
        }

        public override void ClickedBack()
        {
            _instance.editorTypeSettingView.Show();
        }

        public void SetChannelType(CHANNELTYPE channeltype)
        {
            channelType = channeltype;
        }

        public CHANNELTYPE GetChannelType()
        {
            return channelType;
        }
    }
}