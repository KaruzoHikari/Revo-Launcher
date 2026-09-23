using Animations;
using Misc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
    public class SetupAppSettingView : AbstractSetupSettingView
    {
        public SetupAppSettingView() : base(SettingsController.SETTINGSMENU.CHANNEL_SETUP_APP, ThemeColor.SettingsBackgroundLight)
        {
        }

        public override void Show(Channel channel)
        {
            base.Show(channel);
            _instance.SetupAppSelection_Editor(OnSelectedApp);
        }

        private void OnSelectedApp(AppInfo appInfo)
        {
            currentChannel.SetAppInfo(appInfo);
            
            // now we setup the icon and banner, if they haven't been setup yet
            if (string.IsNullOrEmpty(currentChannel.iconName))
            {
                PopupController.ShowPopup("popup.chooseicon", () =>
                {
                    _instance.setupAnimSettingView.Show(currentChannel, CHANNELTYPE.ICON);
                });   
            }
            else
            {
                currentChannel.Save();
                BackToEdit();
            }
        }
    }
}