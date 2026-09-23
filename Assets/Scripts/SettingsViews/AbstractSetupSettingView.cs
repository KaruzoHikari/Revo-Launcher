using System.Collections;
using System.IO;
using Animations;
using Misc;
using SFB;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsViews
{
    public abstract class AbstractSetupSettingView : SettingsView
    {
        protected Channel currentChannel;
        
        public AbstractSetupSettingView(SettingsController.SETTINGSMENU menu, ThemeColor color = ThemeColor.SettingsBackgroundEdit) : base(menu,
            "settings.title.channelcreation", color)
        {
        }

        public virtual void Show(Channel channel)
        {
            base.Show();
            currentChannel = channel;
            EnableMainGameObject(_instance.selectionMenu);
        }

        public override void ClickedBack()
        {
            BackToEdit();
        }

        protected void BackToEdit()
        {
            SettingsController._instance.EditChannel(currentChannel);
        }
        
        protected void LoadFilePicker(UnityAction<string> pathAction)
        {
            FileManager.RequestFile("Choose your game file!", FILETYPE.ANY, (path) =>
            {
                if (path == null)
                {
                    Debug.Log("Operation cancelled");
                }
                else
                {
                    pathAction.Invoke(path);
                }
            }, true);

            return;



            /*if (Application.isMobilePlatform)
            {
                string[] fileTypes = new string[] { " +/+ " };
                NativeFilePicker.PickFile((path) =>
                {
                    if (path == null)
                    {
                        Debug.Log("Operation cancelled");
                    }
                    else
                    {
                        pathAction.Invoke(path);
                    }
                }, fileTypes);
            }
            else
            {
                var extensions = new[]
                {
                    new ExtensionFilter("Any file", "*")
                };
                string[] paths = FileManager.ChooseFilesPc("Choose a file to open with this channel!", "", extensions, false);
                if (!paths.IsNullOrEmpty())
                {
                    pathAction.Invoke(paths[0]);
                }
            }*/
        }
    }
}