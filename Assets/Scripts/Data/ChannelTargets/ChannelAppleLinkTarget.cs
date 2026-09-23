using System.Web;
using Misc;
using UnityEngine;

namespace Data.ChannelTargets
{
    public class ChannelAppleLinkTarget : ChannelWebTarget
    {
        
        public ChannelAppleLinkTarget() : base(CHANNELTARGETS.APPLE_LINK)
        {
        }
        
        public override void Execute()
        {
            // we run the input URL as a protocol (and add the :// if the user hasn't input it)
            string fixedUrl = url.Contains("://") ? url : url + "://";
            Application.OpenURL(fixedUrl);
        }
    }
}