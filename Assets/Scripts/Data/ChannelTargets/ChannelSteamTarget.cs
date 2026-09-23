using System.IO;
using Misc;
using UnityEngine.Device;

namespace Data.ChannelTargets
{
    public class ChannelSteamTarget : ChannelTarget, HasGameMetadata
    {
        public GameMetadata metadata;
        
        public ChannelSteamTarget() : base(CHANNELTARGETS.STEAM)
        {
        }
        
        public override void LinkChannel(Channel channel)
        {
            base.LinkChannel(channel);
            metadata?.LinkChannel(channel);
        }

        public override void Execute()
        {
            if (IsValid())
            {
                System.Diagnostics.Process.Start(PREFS.SteamLocation.GetString(), $"steam://rungameid/{metadata.gameId}");
            }
        }

        public override string GetTag()
        {
            return IsValid() ? metadata.GetTitle() : "Steam";
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