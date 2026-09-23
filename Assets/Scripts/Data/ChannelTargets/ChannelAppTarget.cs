using Misc;

namespace Data.ChannelTargets
{
    public class ChannelAppTarget : ChannelTarget
    {
        public string data;
        public string activity;
        public string extra;

        public ChannelAppTarget() : base(CHANNELTARGETS.APP)
        {
        }

        public override void Execute()
        {
            // we launch an app
            if (IsValid())
            {
                // windows special treatment
                #if UNITY_STANDALONE_WIN
                System.Diagnostics.Process.Start(linkedChannel.info.packageName, data);
                #else
                AndroidLinker.LaunchApp(linkedChannel.info,activity,data,extra);
                #endif
            }
        }
        
        public void SetActivity(string args)
        {
            activity = args;
            linkedChannel?.Save();
        }

        public void SetExtra(string args)
        {
            extra = args;
            linkedChannel?.Save();
        }

        public void SetData(string args)
        {
            data = args;
            linkedChannel?.Save();
        }

        public override string GetTag()
        {
            return IsValid() ? linkedChannel.info.name : "Fake App";
        }

        public override bool IsValid()
        {
            return linkedChannel.info != null;
        }
    }
}