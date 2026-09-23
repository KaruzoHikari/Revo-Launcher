using System.Collections.Generic;
using Animations;
using Data.ChannelTargets;
using Misc;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
    public class SetupOverviewAppSettingView : AbstractSetupOverviewSettingView
    {
        public SetupOverviewAppSettingView() : base(SettingsController.SETTINGSMENU.CHANNEL_SETUP_APP)
        {
        }

        public override void Show(Channel channel)
        {
            base.Show(channel);
            OptionsSpawner.SummonTitle(_instance.optionsHolder, "settings.target.appargs", true);
            GameObject app = Spawn(_instance.appSourcePrefab);
            if (channel.info != null)
            {
                app.transform.Find("Image").GetComponent<RawImage>().texture = channel.info.icon;
                app.transform.Find("NameAndChange").Find("Name").GetComponent<TextMeshProUGUI>().text = channel.info.name;
                Transform button = app.transform.Find("NameAndChange").Find("Button");
                #if UNITY_IOS
                // no apps to be retrieved in iOS - we disable this button since it could still show up in default channels
                button.gameObject.SetActive(false);
                #else
                button.GetComponent<Button>().onClick.AddListener(() => _instance.setupAppSettingView.Show(channel));
                #endif
            }
            
            // we spawn the activity selector
            #if !UNITY_IOS            
            if (channel.target is ChannelAppTarget target && channel.info != null)
            {
                if (!Application.isMobilePlatform)
                {
                    // startup args
                    Option args = Option.Create("settings.target.args", () => target.data, x => target.SetData(x));
                    OptionsSpawner.SetupOption(args, _instance.optionsHolder);
                }
                else
                {
                    // we setup the activity selector. defaults to.. default
                    List<string> activities = new List<string>();
                    activities.Add("Default");
                    activities.AddRange(AndroidLinker.GetPackageActivities(channel.info.packageName));
                    Option<string> allowedActivities = Option.Create("settings.target.appactivity", () => target.activity ?? "Default", x =>
                    {
                        // we set the new activity
                        target.SetActivity(x.Equals("Default") ? null : x);
                        _instance.setupOverviewAppSettingView.Show(channel);
                    });
                    OptionsSpawner.SummonOption_DropdownString(allowedActivities, _instance.optionsHolder, activities);
            
                    // and if the activity is not the default, we spawn the extra
                    if (!string.IsNullOrEmpty(target.activity))
                    {
                        Option data = Option.Create("settings.target.appdata", () => target.data, x => target.SetData(x));
                        OptionsSpawner.SetupOption(data, _instance.optionsHolder);
                        Option extra = Option.Create("settings.target.appextra", () => target.extra, x => target.SetExtra(x));
                        OptionsSpawner.SetupOption(extra, _instance.optionsHolder);
                    }
                }
            }
            #endif
            
            SpawnDefaultOptions();
        }
    }
}