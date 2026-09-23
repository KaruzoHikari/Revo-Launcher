using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class PreprocessBuild : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;
    
    public void OnPreprocessBuild(BuildReport report)
    {
#if UNITY_ANDROID || UNITY_IOS // we only increase the build number when building for Android or iOS
        
        // we load the build info
        BuildInfo buildInfo = null;
        try
        {
            buildInfo = (BuildInfo)AssetDatabase.LoadAssetAtPath("Assets/DefaultSettings/BuildInfo.asset", typeof(BuildInfo));
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        
        if (buildInfo != null)
        {
            int info = buildInfo.RaiseBundle();
            Debug.Log($"The version is now: {buildInfo.buildBundle}");
            PlayerSettings.Android.bundleVersionCode = info;
            PlayerSettings.iOS.buildNumber = info.ToString();
        }
        else
        {
            Debug.LogError("Couldn't find asset!");
        }
#endif
    }
}
