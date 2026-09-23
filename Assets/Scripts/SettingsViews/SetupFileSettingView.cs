using System.IO;
using Animations;
using Data.ChannelTargets;
using Misc;
using SFB;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
    public class SetupFileSettingView : AbstractSetupSettingView
    {
        public SetupFileSettingView() : base(SettingsController.SETTINGSMENU.CHANNEL_SETUP_FILE)
        {
        }

        public override void Show(Channel channel)
        {
            base.Show(channel);
            
            // we load a file picker to choose the file
            LoadFilePicker(path =>
            {
                if (currentChannel.GetTarget() is ChannelFileTarget target)
                {
                    target.path = path;
                    
                    // now we setup the icon and banner, if they haven't been setup yet
                    if (string.IsNullOrEmpty(currentChannel.iconName))
                    {
                        PopupController.ShowPopup("popup.selectedpath", () =>
                        {
                            _instance.setupAnimSettingView.Show(currentChannel, CHANNELTYPE.ICON);
                        }, true, new[] {FileManager.GetFileName(path)});   
                    }
                    else
                    {
                        currentChannel.Save();
                        BackToEdit();
                    }
                }
            });
        }
    }
}