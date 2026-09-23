using Animations;
using Data.ChannelTargets;
using Misc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
    public class SetupOverviewWebSettingView : AbstractSetupOverviewSettingView
    {
        public SetupOverviewWebSettingView() : base(SettingsController.SETTINGSMENU.CHANNEL_SETUP_WEB)
        {
        }

        public override void Show(Channel channel)
        {
            base.Show(channel);
            if (channel.GetTarget() is ChannelWebTarget target)
            {
                OptionsSpawner.SummonTitle(_instance.optionsHolder, "settings.target.webargs");
                
                // we setup the link
                Option id = Option.Create("settings.target.url", () => target.url, x => target.SetUrl(x));
                OptionsSpawner.SetupOption(id, _instance.optionsHolder);
            }

            SpawnDefaultOptions();
        }
    }
}