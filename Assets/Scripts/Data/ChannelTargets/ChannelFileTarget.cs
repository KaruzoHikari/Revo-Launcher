using System.IO;
using Misc;
using UnityEngine.Device;

namespace Data.ChannelTargets
{
    public class ChannelFileTarget : ChannelTarget
    {
        public string path;
        public string startupArgs;
        
        public ChannelFileTarget() : base(CHANNELTARGETS.FILE)
        {
        }
        
        public void SetStartupArgs(string args)
        {
            startupArgs = args;
            linkedChannel?.Save();
        }

        public override void Execute()
        {
            // this is prob not gonna work on mobile. not sure how to fix rn honestly
            System.Diagnostics.Process.Start(path, startupArgs);
        }

        public override string GetTag()
        {
            return Path.GetFileNameWithoutExtension(path);
        }

        public override bool IsValid()
        {
            return !string.IsNullOrEmpty(path);
        }
    }
}