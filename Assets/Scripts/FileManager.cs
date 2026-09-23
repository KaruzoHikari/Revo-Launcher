using System;
using System.Collections;
using System.IO;
using System.Linq;
using SFB;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.Events;

public static class FileManager
{
    public static void CopyFile(string origin, string target, bool shouldOverride = true) {
        Debug.Log($"Copying {origin} to {target}");
        FileBrowserHelpers.CopyFile(origin, target);
    }
    
    public static void MoveFile(string origin, string target, bool shouldOverride = true) {
        Debug.Log($"Moving {origin} to {target}");
        FileBrowserHelpers.MoveFile(origin, target);
    }

    public static bool IsSamePath(string path1, string path2)
    {
        return Path.GetFullPath(path1).Equals(Path.GetFullPath(path2));
    }

    public static string GetFileName(string path)
    {
        return Application.isMobilePlatform && FileBrowserHelpers.FileExists(path) ? FileBrowserHelpers.GetFilename(path) : Path.GetFileName(path);
    }

    public static bool Exists(string path)
    {
        return FileBrowserHelpers.FileExists(path);
    }

    public static string GetDirectoryName(string path)
    {
        return Application.isMobilePlatform && FileBrowserHelpers.DirectoryExists(path) ? FileBrowserHelpers.GetDirectoryName(path) : Path.GetDirectoryName(path);
    }

    public static string GetExtension(string path)
    {
        return Path.GetExtension(GetFileName(path)).Replace(".","");
    }

    public static bool IsValidExtension(string path, FILETYPE type)
    {
        string myExtension = GetExtension(path).ToLowerInvariant();
        return type == FILETYPE.ANY || GetAllowedExtensions(type).Any(ext => ext.Equals(myExtension));
    }

    public static string[] GetAllowedExtensions(FILETYPE type)
    {
        switch (type)
        {
            case FILETYPE.ZIP: return new[] { "zip" };
            case FILETYPE.IMAGES_ONLY: return new[] {"png", "jpg", "jpeg" };
            case FILETYPE.IMAGES_WITH_GIF: return new[] { "png", "jpg", "jpeg", "gif" };
            case FILETYPE.IMAGES_WITH_GIF_AND_VIDEOS: return new[] { "png", "jpg", "jpeg", "gif", "mp4", "mov", "wmv", "webm" };
            case FILETYPE.VIDEO: return new[] { "mp4", "mov", "wmv", "webm" };
            case FILETYPE.AUDIO: return new[] { "mp3", "wav", "ogg" };
            case FILETYPE.FONT: return new[] { "otf", "ttf" };
            default: return new[] { "*" };
        }
    }

    private static ExtensionFilter[] GetPcFilters(FILETYPE type)
    {
        switch (type)
        {
            case FILETYPE.ZIP: return new[] { new ExtensionFilter("ZIP files", GetAllowedExtensions(type)) };
            case FILETYPE.IMAGES_ONLY: return new[] { new ExtensionFilter("Image Files", GetAllowedExtensions(type)) };
            case FILETYPE.IMAGES_WITH_GIF: return new[] { new ExtensionFilter("Image and GIF Files", GetAllowedExtensions(type)) };
            case FILETYPE.IMAGES_WITH_GIF_AND_VIDEOS: return new[] { new ExtensionFilter("Image, Videos and GIF Files", GetAllowedExtensions(type)) };
            case FILETYPE.VIDEO: return new[] {  new ExtensionFilter("Video Files", GetAllowedExtensions(type)) };
            case FILETYPE.AUDIO: return new[] {  new ExtensionFilter("Audio Files", GetAllowedExtensions(type)) };
            case FILETYPE.FONT: return new[] {  new ExtensionFilter("Font Files", GetAllowedExtensions(type)) };
            default: return new[] { new ExtensionFilter("Any file", GetAllowedExtensions(type)) };
        }
    }

    private static string[] GetAndroidFilters(FILETYPE type)
    {
        switch (type)
        {
            #if UNITY_IOS
            case FILETYPE.ZIP: return new[] { "public.archive" }; 
            case FILETYPE.IMAGES_ONLY: return new[] { "public.image" };
            case FILETYPE.IMAGES_WITH_GIF: return new[] { "public.image" }; // we'll need to check manually if it was allowed or not
            case FILETYPE.IMAGES_WITH_GIF_AND_VIDEOS: return new[] { "public.image", "public.movie" }; // we'll need to check manually if it was allowed or not
            case FILETYPE.VIDEO: return new[] { "public.movie" };
            case FILETYPE.AUDIO: return new[] { "public.audio" };
            #else
            case FILETYPE.ZIP: return new[] {"application/zip"}; 
            case FILETYPE.IMAGES_ONLY: return new[] {"image/*"};
            case FILETYPE.IMAGES_WITH_GIF: return new[] {"image/*"}; // we'll need to check manually if it was allowed or not
            case FILETYPE.IMAGES_WITH_GIF_AND_VIDEOS: return new[] {"image/*", "video/*"}; // we'll need to check manually if it was allowed or not
            case FILETYPE.VIDEO: return new[] { "video/*" };
            case FILETYPE.AUDIO: return new[] { "audio/wav", "audio/x-wav", "audio/mpeg", "audio/mp3", "audio/ogg" };
            #endif
            case FILETYPE.FONT: return new[] { NativeFilePicker.ConvertExtensionToFileType("otf"), NativeFilePicker.ConvertExtensionToFileType("ttf") };
            default: return new string[] { }; // we allow all
        }
    }
    
    // don't forget to copy it to a readable place after requesting a file
    // because of Android SAF's file picker
    public static void RequestFile(string title, FILETYPE type, UnityAction<string> callback, bool forceAndroidStandalone = false)
    {
        AppController._instance.StartCoroutine(_ChooseFiles(title, type, callback, forceAndroidStandalone));
    }

    private static IEnumerator _ChooseFiles(string title, FILETYPE type, UnityAction<string> callback, bool useAndroidStandalone = false)
    {
        yield return new WaitForSeconds(0.1f);
        NativeFilePicker.FilePickedCallback innerCallback = path =>
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            
            // we check if it's a valid extension
            if (IsValidExtension(path, type))
            {
                callback?.Invoke(path);
            }
            else
            {
                PopupController.ShowPopup("popup.unsupportedextension", replacementArray: new []{ GetExtension(path).ToUpperInvariant() });
            }
        };
        
        if (Application.isMobilePlatform)
        {
            ChooseFilesAndroid(title,GetAndroidFilters(type),type,innerCallback,useAndroidStandalone);
        }
        else
        {
            string[] paths = ChooseFilesPc(title, "", GetPcFilters(type), false);
            if (paths.Length > 0)
            {
                innerCallback.Invoke(paths[0]);
            }
        }
    }

    private static bool ShouldUseStandaloneBrowser(bool shouldUse)
    {
        #if UNITY_IOS
        return false; // the standalone file browser can't access files outside our own in iOS
        #endif
        return PREFS.StandaloneBrowser.GetBool() || shouldUse;
    }

    private static void ChooseFilesAndroid(string title, string[] allowedFileTypes, FILETYPE type, NativeFilePicker.FilePickedCallback callback, bool shouldUseStandalone = false)
    {
        if (!ShouldUseStandaloneBrowser(shouldUseStandalone))
        {
            bool isGallery = type is FILETYPE.IMAGES_ONLY or FILETYPE.IMAGES_WITH_GIF;
            if (isGallery)
            {
                NativeGallery.GetImageFromGallery(path => callback?.Invoke(path), title);
            }
            else
            {
                NativeFilePicker.PickFile(callback, allowedFileTypes);
            }
        }
        else
        {
            AppController._instance.StartCoroutine(_ChooseFilesStandaloneAndroid(callback,title));
        }
    }

    private static IEnumerator _ChooseFilesStandaloneAndroid(NativeFilePicker.FilePickedCallback callback, string title)
    {
        // let's try picking stuff with standalone
        FileBrowser.SingleClickMode = false;
        FadeController._instance.FadeIn(0.1f);
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, null, null, title);

        // Dialog is closed now
        FadeController._instance.FadeOut(0.1f);

        if (FileBrowser.Success)
        {
            string originalPath = FileBrowser.Result[0];
            Debug.Log($"The path read is: {originalPath}");
            callback.Invoke(originalPath);
        }
        else
        {
            Debug.Log("Couldn't read the path!");
        }
    }

    private static string[] ChooseFilesPc(string title, string directory, ExtensionFilter[] extensions, bool multiselect)
    {
        Cursor.visible = true;
        string[] paths = StandaloneFileBrowser.OpenFilePanel(title, directory, extensions, multiselect);
        Cursor.visible = false;
        return paths;
    }
    
    public static void ExportFile(string title, FILETYPE type, string sourcePath, string fileName, UnityAction<bool> callback, bool forceAndroidStandalone = false)
    {
        AppController._instance.StartCoroutine(_ExportFiles(title, type, sourcePath, fileName, callback, forceAndroidStandalone));
    }

    private static IEnumerator _ExportFiles(string title, FILETYPE type, string sourcePath, string fileName, UnityAction<bool> callback, bool useAndroidStandalone = false)
    {
        yield return new WaitForSeconds(0.1f);
        
        // time to export ourselves
        UnityAction<string> innerCallback = path =>
        {
            Debug.Log("Exported to: " + path);
            bool finished = !string.IsNullOrEmpty(path);
            #if !UNITY_IOS // not needed in iOS, the file is properly exported
            if (finished)
            {
                // we need to finish the export ourselves!
                if (!Application.isMobilePlatform)
                {
                    path = GetDirectoryName(path) + Path.DirectorySeparatorChar + fileName;
                }
                CopyFile(sourcePath,path);
            }
            #endif
            callback.Invoke(finished);
        };
        
        if (Application.isMobilePlatform)
        {
            ExportFilesAndroid(title, sourcePath, fileName, innerCallback, useAndroidStandalone);
        }
        else
        {
            string exportedPath = ExportFilesPc(title, "", fileName, GetPcFilters(type));
            if (!string.IsNullOrEmpty(exportedPath))
            {
                innerCallback.Invoke(exportedPath);
            }
        }
    }

    private static void ExportFilesAndroid(string title, string path, string filename, UnityAction<string> callback, bool shouldUseStandalone = false)
    {
        if (!ShouldUseStandaloneBrowser(shouldUseStandalone))
        {
            NativeFilePicker.ExportFile(path, exported =>
            {
                Debug.Log("inner exported to: " + path);
                callback.Invoke(exported ? "Exported" : null);
            });
        }
        else
        {
            AppController._instance.StartCoroutine(_ExportFilesStandaloneAndroid(title, filename, callback));
        }
    }
    
    private static IEnumerator _ExportFilesStandaloneAndroid(string title, string filename, UnityAction<string> callback)
    {
        // let's try picking stuff with standalone
        FileBrowser.SingleClickMode = false;
        FadeController._instance.FadeIn(0.1f);
        yield return FileBrowser.WaitForSaveDialog(FileBrowser.PickMode.Files, false, null, filename, title);

        // Dialog is closed now
        FadeController._instance.FadeOut(0.1f);

        if (FileBrowser.Success)
        {
            string originalPath = FileBrowser.Result[0];
            Debug.Log($"The path read is: {originalPath}");
            callback.Invoke(originalPath);
        }
        else
        {
            Debug.Log("Couldn't read the path!");
        }
    }
    
    private static string ExportFilesPc(string title, string directory, string fileName, ExtensionFilter[] extensions)
    {
        Cursor.visible = true;
        string path = StandaloneFileBrowser.SaveFilePanel(title, directory, GetFileName(fileName), extensions);
        Cursor.visible = false;
        return path;
    }

    public static void CopyDirectory(string original, string target)
    {
        DirectoryInfo directory = new DirectoryInfo(original);

        // First we create the target dir
        string newTarget = target + Path.DirectorySeparatorChar + directory.Name + Path.DirectorySeparatorChar;
        if (!Directory.Exists(newTarget))
        {
            Directory.CreateDirectory(newTarget);
        }
        
        // Then we copy all the subfolders and files
        foreach (string dir in Directory.GetDirectories(directory.FullName, "*", SearchOption.AllDirectories)) 
        {
            string dirToCreate = dir.Replace(directory.FullName, newTarget); 
            Directory.CreateDirectory(dirToCreate); 
        }
        
        foreach (string newPath in Directory.GetFiles(directory.FullName, "*.*", SearchOption.AllDirectories)) 
        { 
            CopyFile(newPath, newPath.Replace(directory.FullName, newTarget), true); 
        }
    }
    
    public static void EmptyDirectory(string path)
    {
        DirectoryInfo di = new DirectoryInfo(path);
        foreach (FileInfo file in di.GetFiles())
        {
            try
            {
                file.Delete(); 
            }
            catch (Exception e)
            {
                // ignored
            }
        }
        foreach (DirectoryInfo dir in di.GetDirectories())
        {
            try
            {
                dir.Delete(true);
            }
            catch (Exception e)
            {
                // ignored
            }
        }
    }
}