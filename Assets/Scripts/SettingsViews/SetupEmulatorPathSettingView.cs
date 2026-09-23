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
    public class SetupEmulatorPathSettingView : AbstractSetupSettingView
    {
        public SetupEmulatorPathSettingView() : base(SettingsController.SETTINGSMENU.CHANNEL_SETUP_EMULATOR)
        {
        }

        public override void Show(Channel channel)
        {
            base.Show(channel);

            if (currentChannel.GetTarget() is ChannelEmulatorTarget target && target.metadata.consoleType != CONSOLETYPE.UNKNOWN)
            {
                // we try to assign the app
                TryAssigningApp(target.metadata);
            }
            else
            {
                // they have to choose the platform first
                PopupController.ShowPopup("popup.cantdetectplatform", () =>
                {
                    BackToEdit();
                });
            }
        }

        private void TryAssigningApp(GameMetadata metadata)
        {
            // We're gonna try finding an app suitable for this game installed in the device.
            // If we find it, we just finish the setup. Otherwise, we send the user to select it
            
            // First we check if we already have a preferred emulator for this
            UnityStringPreference pref = PREFS.GetEmulatorPreference(metadata);
            string chosen = pref.GetString();
            if (!string.IsNullOrEmpty(chosen))
            {
                // Now we check if we can still use this
                if (Application.isMobilePlatform)
                {
                    // It's a package
                    AppInfo info = AndroidLinker._instance.GetAppInfo(chosen);
                    if (info != null)
                    {
                        currentChannel.SetAppInfo(info);
                        InitChannelAndExit();
                        return;
                    }
                }
                else
                {
                    // It's a filepath
                    if (File.Exists(chosen))
                    {
                        ((ChannelEmulatorTarget)currentChannel.target).SetEmulatorPath(chosen);
                        InitChannelAndExit();
                        return;
                    }
                }
            }
            
            // No luck. Now we try finding it automatically from the apps installed
            AppInfo app = MetadataController._instance.FindSuitableApp(metadata);
            if (app != null)
            {
                currentChannel.SetAppInfo(app);
                pref.SetString(app.packageName);
                InitChannelAndExit();
            }
            else
            {
                // No suitable app found. They're gonna have to choose it themselves
                string popup = Application.isMobilePlatform ? "popup.missingemulatorapp" : "popup.missingemulatorprogram";
                PopupController.ShowPopup(popup, () => { SelectApp(pref); });
            }
        }

        public void OverrideApp(Channel channel)
        {
            if (channel.GetTarget() is ChannelEmulatorTarget target)
            {
                this.currentChannel = channel;
                SelectApp(PREFS.GetEmulatorPreference(target.GetGameMetadata()));
            }
        }

        public void SelectApp(UnityStringPreference pref)
        {
            if (Application.isMobilePlatform)
            {
                _instance.SetupAppSelection_Editor(app =>
                {
                    currentChannel.SetAppInfo(app);
                    pref.SetString(app.packageName);
                    InitChannelAndExit();
                });
            }
            else
            {
                FileManager.RequestFile("Choose the emulator executable to launch this game!", FILETYPE.ANY, path =>
                {
                    if(!string.IsNullOrEmpty(path) && currentChannel.GetTarget() is ChannelEmulatorTarget target)
                    {
                        target.SetEmulatorPath(path);
                        pref.SetString(path);
                        InitChannelAndExit();
                    }
                });
            }
        }

        private void InitChannelAndExit(bool isNewApp = false)
        {
            if (!currentChannel.isFullySetup && currentChannel.GetTarget() is ChannelEmulatorTarget target)
            {
                bool found = target.FindDefaultParams();
                currentChannel.isFullySetup = true;
                ChannelController._instance.InitializeNewChannel(currentChannel);

                // we check if it's retroarch or an unknown app, in that case, we might still be missing the cores
                if (target.IsSupportedByDefault() || found)
                {
                    _instance.ExitSettings(true, false);   
                }
                else
                {
                    // bad luck, gotta setup some stuff
                    BackToEdit();
                }

                
                // and we refresh the metadata just in case!
                target.metadata?.RefreshMetadata();
            }
            else
            {
                // we just go back to edit
                BackToEdit();
            }
        }
    }
}