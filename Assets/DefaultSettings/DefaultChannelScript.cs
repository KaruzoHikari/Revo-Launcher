using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Default Channel", menuName = "DefaultSettings/DefaultChannelScript", order = 1)]
public class DefaultChannelScript : ScriptableObject
{
    public string channelFileName;
    public string iconFileName;
    public string bannerFileName;
}
