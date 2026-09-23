using Animations;
using Data.ChannelTargets;
using Misc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
    public class SetupOverviewWebhookSettingView : AbstractSetupOverviewSettingView
    {
        public SetupOverviewWebhookSettingView() : base(SettingsController.SETTINGSMENU.CHANNEL_SETUP_WEBHOOK)
        {
        }

        public override void Show(Channel channel)
        {
            base.Show(channel);
            if (channel.GetTarget() is ChannelWebhookTarget target)
            {
                OptionsSpawner.SummonTitle(_instance.optionsHolder, "settings.target.webhookargs");
                
                // we setup the link
                Option url = Option.Create("settings.target.url", () => target.url, x => target.SetUrl(x));
                OptionsSpawner.SetupOption(url, _instance.optionsHolder);
                Option body = Option.Create("settings.target.webhookbody", () => target.body, x => target.SetBody(x));
                OptionsSpawner.SetupOption(body, _instance.optionsHolder);
                Option type = Option.Create("settings.target.webhooktype", () => target.requestType, x => target.SetType(x));
                OptionsSpawner.SetupOption(type, _instance.optionsHolder);
                Option timeout = Option.Create("settings.target.webhooktimeout", () => target.timeout, x => target.SetTimeout(x));
                OptionsSpawner.SetupOption(timeout, _instance.optionsHolder);
                Option popup = Option.Create("settings.target.webhookpopup", () => target.showPopup, x => target.SetPopup(x));
                OptionsSpawner.SetupOption(popup, _instance.optionsHolder);
                Option arg = Option.Create("settings.target.webhookreply", () => target.jsonArg, x => target.SetJsonArg(x));
                OptionsSpawner.SetupOption(arg, _instance.optionsHolder);
            }

            SpawnDefaultOptions();
        }
    }
}