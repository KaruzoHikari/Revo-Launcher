using Animations;
using Misc;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
    public abstract class AbstractSetupOverviewSettingView : AbstractSetupSettingView
    {
        
        public AbstractSetupOverviewSettingView(SettingsController.SETTINGSMENU menu) : base(menu)
        {
        }

        public override void Show(Channel channel)
        {
            base.Show(channel);
            OptionsSpawner.SummonVoid(_instance.optionsHolder, 35f);
        }

        protected void SpawnDefaultOptions()
        {
            OptionsSpawner.SummonVoid(_instance.optionsHolder, 35f);
            OptionsSpawner.SummonTitle(_instance.optionsHolder, "settings.target.generalargs", true);
            Option id = Option.Create("settings.target.tag", () => currentChannel.GetTag(), x => currentChannel.SetTag(x));
            OptionsSpawner.SetupOption(id, _instance.optionsHolder);
            
            SpawnAnim(currentChannel, _instance.iconSourcePrefab, CHANNELTYPE.ICON);
            SpawnAnim(currentChannel, _instance.bannerSourcePrefab, CHANNELTYPE.BANNER);
        }
        
        private void SpawnAnim(Channel channel, GameObject prefab, CHANNELTYPE type) {
            GameObject banner = Spawn(prefab);
            ChannelAnimation anim = channel.GetAnimation(type);
            string name = anim?.name;
            if (string.IsNullOrEmpty(name))
            {
                name = channel.GetAnimationName(type);
                if (!string.IsNullOrEmpty(name))
                {
                    // we try to load its basic info
                    AnimationBasicInfo info = SaveManager.GetBasicInfo(true, name);
                    if (info != null)
                    {
                        name = info.animationName;
                    }
                }
            }
            
            // if we didn't find anything, we set it to None
            if (string.IsNullOrEmpty(name))
            {
                name = "None";
            }
            
            banner.transform.Find("NameAndChange").Find("Name").GetComponent<TextMeshProUGUI>().text = name;
            banner.transform.Find("NameAndChange").Find("Button").GetComponent<Button>().onClick.AddListener(() =>
            {
                _instance.setupAnimSettingView.Show(channel, type);
            });
        }

        protected GameObject Spawn(GameObject prefab)
        {
            return GameObject.Instantiate(prefab, _instance.optionsHolder);
        }

        public override void ClickedBack()
        {
            BackOrClose();
        }

        protected void BackOrClose()
        {
            // here we decide what we need to do
            if (currentChannel.isFullySetup)
            {
                // the channel was already created, this was an edit. let's close it
                _instance.ExitSettings(true, false);
            }
            else
            {
                // this was a new channel! let's go back to the types overview
                _instance.setupTypeSettingView.Show(currentChannel);
            }
        }
    }
}