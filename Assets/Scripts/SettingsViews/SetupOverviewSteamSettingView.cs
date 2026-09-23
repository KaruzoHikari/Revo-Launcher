using System.Collections.Generic;
using System.Linq;
using Animations;
using Data.ChannelTargets;
using Misc;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
    public class SetupOverviewSteamSettingView : AbstractSetupOverviewSettingView
    {
        public SetupOverviewSteamSettingView() : base(SettingsController.SETTINGSMENU.CHANNEL_SETUP_STEAM)
        {
        }

        public override void Show(Channel channel)
        {
            base.Show(channel);
            if (channel.GetTarget() is ChannelSteamTarget target)
            {
                // now we setup the game's path
                OptionsSpawner.SummonTitle(_instance.optionsHolder, "settings.target.gameargs", true);
                GameObject path = Spawn(_instance.pathSourcePrefab);
                path.transform.Find("NameAndChange").Find("Name").GetComponent<TextMeshProUGUI>().text = target.metadata != null ? target.metadata.title : "None";
                path.transform.Find("NameAndChange").Find("Button").GetComponent<Button>().onClick.AddListener(() => _instance.setupSteamView.Show(channel));

                if (target.metadata != null)
                {
                    GameMetadata metadata = target.metadata;
                    // finally we spawn box color + whether it should rotate
                    Option boxColor = Option.Create("settings.target.boxcolor", () => metadata.boxColor,
                        x => metadata.SetBoxColor(x));
                    OptionsSpawner.SetupOption(boxColor, _instance.optionsHolder);
                    Option rotateBox = Option.Create("settings.target.rotatebox", () => metadata.rotateBox,
                        x => metadata.rotateBox = x);
                    OptionsSpawner.SetupOption(rotateBox, _instance.optionsHolder);
                }
            }

            SpawnDefaultOptions();
        }

        public override void ClickedBack()
        {
            // we reset the metadata of the channel (so we don't download it 500 times)
            if (currentChannel != null && currentChannel.target is ChannelSteamTarget target)
            {
                target.metadata?.RefreshMetadata();
            }
            
            base.ClickedBack();
        }
    }
}