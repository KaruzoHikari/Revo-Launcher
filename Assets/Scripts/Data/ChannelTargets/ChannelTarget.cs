using Misc;
using Newtonsoft.Json;

namespace Data
{
    public abstract class ChannelTarget
    {
        public CHANNELTARGETS type;
        [JsonIgnore] protected Channel linkedChannel;

        protected ChannelTarget(CHANNELTARGETS type)
        {
            this.type = type;
        }
        
        public CHANNELTARGETS GetTargetType()
        {
            return type;
        }

        public virtual void LinkChannel(Channel channel)
        {
            linkedChannel = channel;
        }

        public abstract void Execute();

        public abstract string GetTag();
        
        public abstract bool IsValid();
    }
}