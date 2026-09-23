using Animations;
using Data.ChannelTargets;
using Misc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
    public class SetupAnimSettingView : AbstractSetupSettingView
    {
        private CHANNELTYPE currentType;
        
        public SetupAnimSettingView() : base(SettingsController.SETTINGSMENU.CHANNEL_SETUP_ANIM, ThemeColor.SettingsBackgroundLight)
        {
        }

        public void Show(Channel channel, CHANNELTYPE type)
        {
            base.Show(channel);
            currentType = type;
            SetTitle(type == CHANNELTYPE.ICON
                ? "settings.title.selecticon"
                : "settings.title.selectbanner");

            _instance.SetupChannelSelection_Editor(type, true, OnSelectedChannel, channel.GetTarget() is HasGameMetadata);
        }

        private void OnSelectedChannel(ChannelAnimation animation)
        {
            if (animation.type == CHANNELTYPE.ICON)
            {
                // we assign the icon animation.
                currentChannel.iconAnimation?.linkedChannels.Remove(currentChannel);
                currentChannel.SetIconAnimation(animation);
                
                // if the banner doesn't exist, we go for it automatically
                if (string.IsNullOrEmpty(currentChannel.bannerName))
                {
                    PopupController.ShowPopup("popup.choosebanner", () =>
                    {
                        Show(currentChannel, CHANNELTYPE.BANNER);
                    });
                }
            }
            else
            {
                // we assign the banner
                currentChannel.bannerAnimation?.linkedChannels.Remove(currentChannel);
                currentChannel.SetBannerAnimation(animation);
                
                // if we're on low memory mode, we can unload the banner now
                if (AppController._instance.IsLowMemoryMode())
                {
                    animation.Unload();
                }

                // if the icon doesn't exist, we go for it automatically
                if (string.IsNullOrEmpty(currentChannel.iconName))
                {
                    PopupController.ShowPopup("popup.chooseicon", () =>
                    {
                        Show(currentChannel, CHANNELTYPE.ICON);
                    });
                }
            }

            if (!string.IsNullOrEmpty(currentChannel.iconName) && !string.IsNullOrEmpty(currentChannel.bannerName))
            {
                // the anims are both setup! we either go back to the edit menu, or leave
                bool isTargetFinished = currentChannel.target.IsValid();
                if (currentChannel.isFullySetup || !isTargetFinished)
                {
                    // back to the edit menu!
                    currentChannel.Save();
                    SettingsController._instance.EditChannel(currentChannel);
                }
                else
                {
                    // we finish the channel!
                    currentChannel.isFullySetup = true;
                    ChannelController._instance.InitializeNewChannel(currentChannel);
                    _instance.ExitSettings(true, false);
                }
            }
        }

        public override void ClickedBack()
        {
            // we just go back to the edit page, they can finish the setup from there if they want (or cancel it altogether)
            if (currentChannel.target is ChannelShopTarget)
            {
                // this target doesn't need more setup so there's no 
            }
            SettingsController._instance.EditChannel(currentChannel);
        }
    }
}