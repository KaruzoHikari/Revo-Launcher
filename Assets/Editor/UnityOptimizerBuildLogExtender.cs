using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Build;
using System;
using System.Globalization;

/// <summary>
/// UnityOptimizerBuildLogExtender.cs
/// Version : 1.0.0.1
/// Author : Eviral
/// This class is provided by Unity Optimizer (Eviral Software) to collect extra build data
/// This editor class won't be added to your builds and can be safely deleted if you want
/// </summary>
public class UnityOptimizerBuildLogExtender : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }

    int IOrderedCallback.callbackOrder => DoNothing();

    public int DoNothing()
    {
        return 0;
    }

    public void OnPreprocessBuild(BuildReport report)
    {

    }

    public void OnPostprocessBuild(BuildReport report)
    {
        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        
        Debug.Log("UnityOptimizerBuildLogExtender.OnPostprocessBuild report name : " + report.name);

        DateTime dt = DateTime.Now;

        string s = dt.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);


        Debug.Log("UnityOptimizerBuildLogExtender.OnPostprocessBuild build date : " + s);

        Debug.Log("UnityOptimizerBuildLogExtender.OnPostprocessBuild platform " + report.summary.platform);

        Debug.Log("UnityOptimizerBuildLogExtender.OnPostprocessBuild build path : " + report.summary.outputPath);

        Debug.Log("UnityOptimizerBuildLogExtender.OnPostprocessBuild files length : " + report.GetFiles().Length);

        BuildOptions options = report.summary.options;

        bool Lz4 = false;
        bool Lz4HC = false;

        if ((options & BuildOptions.CompressWithLz4) == BuildOptions.CompressWithLz4)
        {
            Lz4 = true;
            Debug.Log("UnityOptimizerBuildLogExtender.OnPostprocessBuild compression : CompressWithLz4");
        }

        if ((options & BuildOptions.CompressWithLz4HC) == BuildOptions.CompressWithLz4HC)
        {
            Lz4HC = true;
            Debug.Log("UnityOptimizerBuildLogExtender.OnPostprocessBuild compression : CompressWithLz4HC");
        }

        if (!Lz4 && !Lz4HC)
        {
            Debug.Log("UnityOptimizerBuildLogExtender.OnPostprocessBuild compression : None");
        }

        if ((options & BuildOptions.InstallInBuildFolder) == BuildOptions.InstallInBuildFolder)
        {
            Debug.Log("UnityOptimizerBuildLogExtender.OnPostprocessBuild InstallInBuildFolder : yes");
        }
    }
}