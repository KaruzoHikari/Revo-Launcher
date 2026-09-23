using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "Build Info", menuName = "DefaultThemes/BuildInfoScript", order = 1)]
public class BuildInfo : ScriptableObject
{
    public int buildBundle;

    public int RaiseBundle()
    {
        ++buildBundle;
        #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        #endif
        return buildBundle;
    }
}
