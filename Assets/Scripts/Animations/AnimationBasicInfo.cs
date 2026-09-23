using System;

namespace Animations
{
    // Also used for themes since they share the same basic info
    public class AnimationBasicInfo
    {
        public string animationName;
        public string animationFileName;
        public DateTime animationLastEdited;
        public bool isOnline;
        public int innerVersion;
        public bool isEmulator;
    }
}