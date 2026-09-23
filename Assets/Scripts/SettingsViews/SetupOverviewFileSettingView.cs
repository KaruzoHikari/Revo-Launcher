using Animations;
using Data.ChannelTargets;
using Misc;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
    public class SetupOverviewFileSettingView : AbstractSetupOverviewSettingView
    {
        public SetupOverviewFileSettingView() : base(SettingsController.SETTINGSMENU.CHANNEL_SETUP_FILE)
        {
        }

        public override void Show(Channel channel)
        {
            base.Show(channel);
            if (channel.GetTarget() is ChannelFileTarget target)
            {
                OptionsSpawner.SummonTitle(_instance.optionsHolder, "settings.target.fileargs", true);
                
                // the path changer
                GameObject path = Spawn(_instance.pathSourcePrefab);
                path.transform.Find("NameAndChange").Find("Name").GetComponent<TextMeshProUGUI>().text = target.path;
                path.transform.Find("NameAndChange").Find("Button").GetComponent<Button>().onClick.AddListener(() => _instance.setupFileSettingView.Show(channel));
                
                // and the startup args
                Option args = Option.Create("settings.target.args", () => target.startupArgs, x => target.SetStartupArgs(x));
                OptionsSpawner.SetupOption(args, _instance.optionsHolder);
            }

            SpawnDefaultOptions();
        }
    }
}