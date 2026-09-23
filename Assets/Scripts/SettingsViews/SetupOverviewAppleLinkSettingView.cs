using Animations;
using Data.ChannelTargets;
using Misc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
    public class SetupOverviewAppleLinkSettingView : AbstractSetupOverviewSettingView
    {
        public SetupOverviewAppleLinkSettingView() : base(SettingsController.SETTINGSMENU.CHANNEL_SETUP_APPLE_LINK)
        {
        }

        public override void Show(Channel channel)
        {
            base.Show(channel);
            if (channel.GetTarget() is ChannelAppleLinkTarget target)
            {
                // force show explanation for Apple Link if it's the first time
                if (PREFS.FirstTimeAppleLink.GetBool())
                {
                    PopupController.ShowPopup("popup.applelinkexplanation");
                    PREFS.FirstTimeAppleLink.SetBool(false);
                }
                
                OptionsSpawner.SummonTitle(_instance.optionsHolder, "settings.target.appargs");
                Option id = Option.Create("settings.target.applelinkurl", "popup.applelinkexplanation", () => target.url, x => target.SetUrl(x));
                OptionsSpawner.SetupOption(id, _instance.optionsHolder);
            }

            SpawnDefaultOptions();
        }
    }
}