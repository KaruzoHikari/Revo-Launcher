using Animations;
using Data.ChannelTargets;
using Misc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
    public class SetupOverviewAppleShortcutSettingView : AbstractSetupOverviewSettingView
    {
        public SetupOverviewAppleShortcutSettingView() : base(SettingsController.SETTINGSMENU.CHANNEL_SETUP_APPLE_SHORTCUTS)
        {
        }

        public override void Show(Channel channel)
        {
            base.Show(channel);
            if (channel.GetTarget() is ChannelAppleShortcutTarget target)
            {
                OptionsSpawner.SummonTitle(_instance.optionsHolder, "settings.target.appleshortcutargs");
                Option id = Option.Create("settings.target.appleshortcutname", () => target.id, x => target.SetId(x));
                OptionsSpawner.SetupOption(id, _instance.optionsHolder);
                Option input = Option.Create("settings.target.appleshortcutinput", () => target.input, x => target.SetInput(x));
                OptionsSpawner.SetupOption(input, _instance.optionsHolder);
            }

            SpawnDefaultOptions();
        }
    }
}