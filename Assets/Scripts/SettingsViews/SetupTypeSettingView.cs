using Animations;
using Data;
using Data.ChannelTargets;
using Misc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
    public class SetupTypeSettingView : AbstractSetupSettingView
    {
        public SetupTypeSettingView() : base(SettingsController.SETTINGSMENU.CHANNEL_SETUP_TYPE, ThemeColor.SettingsBackgroundLight)
        {
        }

        public override void Show(Channel channel)
        {
            base.Show(channel);
            
            // We're gonna spawn all the different channel types available in Revo
            SetContentPivot(1f);
            if (!Application.isMobilePlatform)
            {
                SpawnButton(_instance.setupChannelSteamPrefab, new ChannelSteamTarget(), () => _instance.setupSteamView.Show(channel));
            }
            
            #if UNITY_IOS || UNITY_EDITOR
            SpawnButton(_instance.setupChannelAppleLinkPrefab, new ChannelAppleLinkTarget(), () => _instance.setupOverviewAppleLinkSettingView.Show(channel));
            SpawnButton(_instance.setupChannelAppleShortcutsPrefab, new ChannelAppleShortcutTarget(), () => _instance.setupOverviewAppleShortcutSettingView.Show(channel));
            #endif
            #if !UNITY_IOS
            SpawnButton(_instance.setupChannelAppPrefab, new ChannelAppTarget(), () => _instance.setupAppSettingView.Show(channel));
            SpawnButton(_instance.setupChannelEmulatorPrefab, new ChannelEmulatorTarget(), () => _instance.setupEmulatorSettingView.Show(channel));
            #endif
            
            SpawnButton(_instance.setupChannelWebPrefab, new ChannelWebTarget(), () => _instance.setupOverviewWebSettingView.Show(channel));
            if (!Application.isMobilePlatform)
            {
                /*// mock app in case they want to test it
                SpawnButton(_instance.setupChannelAppMockPrefab, new ChannelAppTarget(), () => _instance.setupAppSettingView.Show(channel));*/

                // no real way to "open" a file in Android, it only gives issues
                SpawnButton(_instance.setupChannelFilePrefab, new ChannelFileTarget(), () => _instance.setupFileSettingView.Show(channel));
            }
            SpawnButton(_instance.setupChannelShopPrefab, new ChannelShopTarget(), () => _instance.setupOverviewShopSettingView.Show(channel));
            SpawnButton(_instance.setupChannelWebhookPrefab, new ChannelWebhookTarget(), () => _instance.setupOverviewWebhookSettingView.Show(channel));
        }

        private void SpawnButton(GameObject obj, ChannelTarget target, UnityAction action)
        {
            GameObject.Instantiate(obj, _instance.optionsHolder).GetComponent<Button>().onClick.AddListener(() =>
            {
                // we reset the channel just in case
                currentChannel.target = null;
                currentChannel.info = null;
                currentChannel.SetIconAnimation(null);
                currentChannel.SetBannerAnimation(null);

                // and we retarget it
                currentChannel.target = target;
                target.LinkChannel(currentChannel);
                
                action.Invoke();
            });
        }

        public override void ClickedBack()
        {
            _instance.ExitSettings(firstGrid: false);
        }
    }
}