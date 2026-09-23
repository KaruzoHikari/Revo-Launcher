using System.Collections;
using System.IO;
using Misc;
using UnityEngine;

namespace Data.ChannelTargets
{
    public class ChannelEmulatorTarget : ChannelTarget, HasGameMetadata
    {
        public GameMetadata metadata;
        
        // todo we could retrieve these args from other channel targets using the same ones, so they don't need to always repeat them
        public string overrideAndroidActivity; // todo mayybe there's a way to list an app's activities and retrieve one containing Emulation one?
        public string overrideAndroidExtra = "Uri"; // we default to Uri because it's what most emulators seem to take
        public string emulatorPath;
        public string startupArgs;
        public string retroarchCore;

        public ChannelEmulatorTarget() : base(CHANNELTARGETS.EMULATOR)
        {
        }

        public bool IsRetroArch()
        {
            return linkedChannel.info != null && linkedChannel.info.packageName.Contains("com.retroarch");
        }

        public bool IsSupportedByDefault()
        {
            // we also exclude retroarch
            bool found = false;
            foreach (Emulator emulator in EMULATORS.GetAllEmulators())
            {
                if (emulator.androidPackage != null && linkedChannel.info != null && emulator.androidPackage.Equals(linkedChannel.info.packageName))
                {
                    if (!IsRetroArch())
                    {
                        found = true;
                        break;
                    }
                }
                else if (!Application.isMobilePlatform && emulator.consoles.Contains(metadata.consoleType))
                {
                    found = true;
                }
            }

            return found;
        }

        public bool FindDefaultParams()
        {
            // we find our settings from other activities settings
            if (IsRetroArch())
            {
                foreach (Channel channel in ChannelController._instance.loadedChannels)
                {
                    if (channel.GetTarget() is ChannelEmulatorTarget target)
                    {
                        if (IsRetroArch())
                        {
                            // we need to find the core
                            if (target.GetGameMetadata().consoleType == metadata.consoleType && !string.IsNullOrEmpty(target.retroarchCore))
                            {
                                // found the first core for this console!
                                this.retroarchCore = target.retroarchCore;
                                return true;
                            }
                        }
                        else
                        {
                            // we find activities for apps like ours
                            if (channel.info != null && linkedChannel.info != null && channel.info.packageName.Equals(linkedChannel.info.packageName))
                            {
                                if (target.overrideAndroidActivity != null)
                                {
                                    this.overrideAndroidActivity = target.overrideAndroidActivity;
                                    this.overrideAndroidExtra = target.overrideAndroidExtra;
                                    return true;
                                }
                            }
                            // PC emulator
                            else if(!string.IsNullOrEmpty(target.emulatorPath) && !string.IsNullOrEmpty(this.emulatorPath))
                            {
                                this.startupArgs = target.startupArgs;
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public void SetEmulatorPath(string path)
        {
            emulatorPath = path;
            SetStartupArgs(MetadataController._instance.GetEmulatorPCArgs(emulatorPath, metadata));
        }

        public void SetStartupArgs(string args)
        {
            startupArgs = args;
            linkedChannel?.Save();
        }

        public void SetActivity(string args)
        {
            overrideAndroidActivity = args;
            linkedChannel?.Save();
        }

        public void SetExtra(string args)
        {
            overrideAndroidExtra = args;
            linkedChannel?.Save();
        }

        public void SetRetroCore(string core)
        {
            retroarchCore = core;
            linkedChannel?.Save();
        }

        public override void LinkChannel(Channel channel)
        {
            base.LinkChannel(channel);
            metadata?.LinkChannel(channel);
        }

        public override void Execute()
        {
            if (Application.isMobilePlatform)
            {
                // we need to launch the app with an intent
                string activity = MetadataController._instance.GetEmulatorAndroidActivity(linkedChannel.info.packageName) ?? overrideAndroidActivity;
                string extra = MetadataController._instance.GetEmulatorAndroidExtra(linkedChannel.info.packageName) ?? overrideAndroidExtra;
                bool sendAsPath = MetadataController._instance.GetEmulatorAndroidCast(linkedChannel.info.packageName);
                if (IsRetroArch())
                {
                    // let's do it differently
                    AndroidLinker.LaunchRetroArch(linkedChannel.info.packageName, metadata.filePath, retroarchCore);
                }
                else
                {
                    AndroidLinker.LaunchEmulator(linkedChannel.info.packageName, metadata.filePath, activity, extra, sendAsPath);
                }
            }
            else
            {
                // depending on the emulator, the commandline args are different.
                string args = metadata.GetFixedArgs(startupArgs);
                System.Diagnostics.Process.Start(emulatorPath, args);
            }
        }

        public override string GetTag()
        {
            return IsValid() ? metadata.GetTitle() : "Emulator";
        }

        public override bool IsValid()
        {
            return metadata != null;
        }

        public GameMetadata GetGameMetadata()
        {
            return metadata;
        }
    }
}