using Misc;
using UnityEngine;

namespace Data.ChannelTargets
{
    public class ChannelWebTarget : ChannelTarget
    {
        public string url;
        
        public ChannelWebTarget() : base(CHANNELTARGETS.WEB)
        {
        }
        
        protected ChannelWebTarget(CHANNELTARGETS target) : base(target)
        {
        }
        
        public void SetUrl(string url)
        {
            this.url = url;
            linkedChannel?.Save();
        }
        
        public override void Execute()
        {
            // we open the URL (might not work on mobile)
            string fixedUrl = !url.ToLowerInvariant().Contains("://") ? "https://" + url : url;
            Application.OpenURL(fixedUrl);
        }

        public override string GetTag()
        {
            return url;
        }

        public override bool IsValid()
        {
            return !string.IsNullOrEmpty(url);
        }
    }
}