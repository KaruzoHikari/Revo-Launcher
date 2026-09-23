using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

// Taken from: https://discussions.unity.com/t/automate-installation-of-new-ios-macos-26-icons-created-with-icon-composer/1695306
// Thanks kirillchikalin!

public class PostprocessBuild
{
    // Place your icon to the project's root folder
    private static string iconName = "icon";
	private static string iconFile = $"{iconName}.icon";
    private static string iconPath = $"Assets/Textures/Others/{iconFile}";
    
    [PostProcessBuild(1)]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        if (target is not (BuildTarget.iOS or BuildTarget.StandaloneOSX))
        {
            return;
        }
        
        var projectPath = target == BuildTarget.iOS
            ? PBXProject.GetPBXProjectPath(path)
            : Path.Combine(path, Path.GetFileNameWithoutExtension(path) + ".xcodeproj/project.pbxproj");
        var project = new PBXProject();
        project.ReadFromString(File.ReadAllText(projectPath));
        var targetGuid = project.GetUnityMainTargetGuid();
        AddIcon(project, path);
        AddPListValues(target, path);
        File.WriteAllText(projectPath, project.WriteToString());
    }

    private static void AddIcon(PBXProject project, string path)
    {
        var dest = Path.Combine(path, iconFile);
        if (Directory.Exists(dest))
        {
            Directory.Delete(dest, recursive: true);
        }

        CopyDirectory(iconPath, dest, recursive: true);
        project.RemoveFile(project.FindFileGuidByRealPath("Unity-iPhone/Images.xcassets"));
        var fileGuid = project.AddFile(iconFile, iconFile);
        var mainTarget = project.GetUnityMainTargetGuid();
        project.AddFileToBuild(mainTarget, fileGuid);
        project.SetBuildProperty(mainTarget, "ASSETCATALOG_COMPILER_APPICON_NAME", iconName);
    }

    static void AddPListValues(BuildTarget target, string pathToXcode)
    {
        // Only apply these plist changes for macOS — remove or expand this if you need iOS changes too
        if (target != BuildTarget.StandaloneOSX)
        {
            return;
        }
        var plistObj = new PlistDocument();
        string pListDir = Path.Combine(pathToXcode, PlayerSettings.productName);
        var plistPath = Path.Combine(pListDir, "Info.plist");
        plistObj.ReadFromString(File.ReadAllText(plistPath));
        var plistRoot = plistObj.root;
        plistRoot.SetString("CFBundleIconFile", iconName);
        File.WriteAllText(plistPath, plistObj.WriteToString());
    }

    // https://learn.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
    private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
    {
        var dir = new DirectoryInfo(sourceDir);

        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");
        }
        DirectoryInfo[] dirs = dir.GetDirectories();
        Directory.CreateDirectory(destinationDir);
        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath);
        }
        if (recursive)
        {
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true);
            }
        }
    }

}
