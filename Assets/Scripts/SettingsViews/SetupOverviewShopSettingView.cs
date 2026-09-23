using Animations;
using Misc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
    public class SetupOverviewShopSettingView : AbstractSetupOverviewSettingView
    {
        public SetupOverviewShopSettingView() : base(SettingsController.SETTINGSMENU.CHANNEL_SETUP_SHOP)
        {
        }

        public override void Show(Channel channel)
        {
            base.Show(channel);
            SpawnDefaultOptions();
            //_instance.setupAnimSettingView.Show(currentChannel, CHANNELTYPE.ICON);
        }
    }
}