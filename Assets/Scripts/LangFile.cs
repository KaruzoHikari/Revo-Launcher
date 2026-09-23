using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Lang Channel", menuName = "Lang Files/Lang File", order = 1)]
public class LangFile : ScriptableObject // was Odin's SerializedScriptableObject
{
    public SystemLanguage language;
    public string fileName;
}
