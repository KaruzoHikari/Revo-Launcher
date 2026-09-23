using System;
using System.Web;
using Misc;
using UnityEngine;

namespace Data.ChannelTargets
{
    public class ChannelAppleShortcutTarget : ChannelTarget
    {
        public string id;
        public string input;
        
        public ChannelAppleShortcutTarget() : base(CHANNELTARGETS.APPLE_SHORTCUTS)
        {
        }
        
        public void SetId(string id)
        {
            this.id = id;
            linkedChannel?.Save();
        }
        
        public void SetInput(string input)
        {
            this.input = input;
            linkedChannel?.Save();
        }
        
        public override void Execute()
        {
            // we open the iOS shortcut with the proper params
            string fixedId = string.IsNullOrEmpty(id) ? "" : Uri.EscapeDataString(id);
            string fixedInput = string.IsNullOrEmpty(input) ? "" : Uri.EscapeDataString(input);
            string url = $"shortcuts://run-shortcut?name={fixedId}&input=text&text={fixedInput}";
            Application.OpenURL(url);
        }

        public override string GetTag()
        {
            return id;
        }

        public override bool IsValid()
        {
            return !string.IsNullOrEmpty(id);
        }
    }
}