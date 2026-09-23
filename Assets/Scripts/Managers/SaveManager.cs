using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Animations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.UnityConverters.Configuration;
using Newtonsoft.Json.UnityConverters.Math;
using SimpleFileBrowser;
using TMPro;
using TriInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;
using CompressionLevel = System.IO.Compression.CompressionLevel;
using Object = System.Object;

public class SaveManager : MonoBehaviour
{
    // save folders
    public static string SAVE_FOLDER;
    public static string SAVE_FOLDER_PREFERENCES;
    public static string SAVE_FOLDER_WALLPAPER;
    public static string SAVE_FOLDER_LANG;
    public static string SAVE_FOLDER_CHANNELS;
    public static string SAVE_FOLDER_ANIMATIONS;
    public static string SAVE_FOLDER_ANIMATIONS_ICONS;
    public static string SAVE_FOLDER_ANIMATIONS_BANNERS;
    public static string SAVE_FOLDER_THEMES;
    public static string SAVE_FOLDER_GAMECOVERS;
    public static string SAVE_FOLDER_GAMEDB;
    
    // temp folders
    public static string TEMP_GENERAL;
    public static string TEMP_REVIEW;
    public static string TEMP_ANIMATIONINFO;
    public static string TEMP_ONLINEINFO;
    public static string TEMP_ASSETSLOAD;
    public static string TEMP_SERIALIZE;
    public static string TEMP_DESERIALIZE;
    public static string TEMP_THUMBNAIL;
    public static string TEMP_ANIMATIONS;
    public static string TEMP_DATA;
    public static string TEMP_TEXTURES;
    public static string TEMP_GAMES;
    
    public static string TEMPZIP_GENERAL;
    public static string TEMPZIP_EXTRACT;
    public static string TEMPZIP_ANIMATIONS;
    public static string TEMPZIP_THEMES;
    public static string TEMPZIP_CHANNELS;
    public static string TEMPZIP_GAMECOVERS;
    public static string TEMPZIP_PREFERENCES;

    public static SaveManager _instance;
    public bool simulateFirstLoad;
    [HideInInspector] public bool isLoadingApps = false;
    public List<DefaultChannelScript> defaultChannels = new List<DefaultChannelScript>();
    public List<DefaultThemeScript> defaultThemes = new List<DefaultThemeScript>();

    public static string boxArtIcon = "icon_a4ec5432-418c-4122-aadd-d2023cd2730d.zip";
    public static string boxArtBanner = "banner_a4ec5432-418c-4122-aadd-d2023cd2730d.zip";
    private void AssignPaths()
    {
        // Regular
        SAVE_FOLDER = Application.persistentDataPath;
        SAVE_FOLDER_PREFERENCES = Application.persistentDataPath + "/preferences.json";
        SAVE_FOLDER_WALLPAPER = Application.persistentDataPath + "/wallpaper"; // extension will vary
        SAVE_FOLDER_CHANNELS = Application.persistentDataPath + "/Channels/";
        SAVE_FOLDER_ANIMATIONS = Application.persistentDataPath + "/Animations/";
        SAVE_FOLDER_ANIMATIONS_ICONS = Application.persistentDataPath + "/Animations/Icons/";
        SAVE_FOLDER_ANIMATIONS_BANNERS = Application.persistentDataPath + "/Animations/Banners/";
        SAVE_FOLDER_LANG = Application.persistentDataPath + "/Languages/";
        SAVE_FOLDER_THEMES = Application.persistentDataPath + "/Themes/";
        SAVE_FOLDER_GAMECOVERS = Application.persistentDataPath + "/GameCovers/";
        SAVE_FOLDER_GAMEDB = Application.persistentDataPath + "/GameDB/";
        
        // Temp
        TEMP_GENERAL = Application.persistentDataPath + "/Temp/";
        TEMP_ONLINEINFO = Application.persistentDataPath + "/Temp/OnlineInfo/";
        TEMP_ANIMATIONINFO = Application.persistentDataPath + "/Temp/AnimationInfo/";
        TEMP_ASSETSLOAD = Application.persistentDataPath + "/Temp/AssetsLoad/";
        TEMP_SERIALIZE = Application.persistentDataPath + "/Temp/Serialize/";
        TEMP_DESERIALIZE = Application.persistentDataPath + "/Temp/Deserialize/";
        TEMP_THUMBNAIL = Application.persistentDataPath + "/Temp/Thumbnail/";
        TEMP_ANIMATIONS = Application.persistentDataPath + "/Temp/Animations/";
        TEMP_REVIEW = Application.persistentDataPath + "/Temp/Review/";
        TEMP_DATA = Application.persistentDataPath + "/Temp/Data/";
        TEMP_TEXTURES = Application.persistentDataPath + "/Temp/Textures/";
        TEMP_GAMES = Application.persistentDataPath + "/Temp/Games/";
        
        // Import data
        TEMPZIP_GENERAL = Application.persistentDataPath + "/TempZip/";
        TEMPZIP_EXTRACT = Application.persistentDataPath + "/TempZip/Extract/";
        TEMPZIP_ANIMATIONS = Application.persistentDataPath + "/TempZip/Extract/Animations/";
        TEMPZIP_CHANNELS = Application.persistentDataPath + "/TempZip/Extract/Channels/";
        TEMPZIP_THEMES = Application.persistentDataPath + "/TempZip/Extract/Themes/";
        TEMPZIP_GAMECOVERS = Application.persistentDataPath + "/TempZip/Extract/GameCovers/";
        TEMPZIP_PREFERENCES = Application.persistentDataPath + "/TempZip/Extract/preferences.json";
    }

    private void Awake()
    {
        _instance = this;
        AssignPaths();
        
        CreateFolder(SAVE_FOLDER_CHANNELS);
        CreateFolder(SAVE_FOLDER_ANIMATIONS_ICONS);
        CreateFolder(SAVE_FOLDER_ANIMATIONS_BANNERS);
        CreateFolder(SAVE_FOLDER_LANG);
        CreateFolder(SAVE_FOLDER_THEMES);
        CreateFolder(SAVE_FOLDER_GAMECOVERS);
        CreateFolder(SAVE_FOLDER_GAMEDB);
        RecreateFolder(TEMP_GENERAL);
        RecreateFolder(TEMP_ONLINEINFO);
        RecreateFolder(TEMP_ANIMATIONINFO);
        RecreateFolder(TEMP_DESERIALIZE);
        RecreateFolder(TEMP_SERIALIZE);
        RecreateFolder(TEMP_THUMBNAIL);
        RecreateFolder(TEMP_ANIMATIONS);
        RecreateFolder(TEMP_DATA);
        RecreateFolder(TEMP_TEXTURES);
        RecreateFolder(TEMP_GAMES);
        DeleteFolder(TEMPZIP_GENERAL);
        CreateNoMedia();

        // Now we load the preferences
        LoadPreferences();
    }

    private void Start()
    {
        UnpackEmuAnims();
        
        // Now we check if it's the first time the channel opens.
        // In that case, we save the default channels and animations in the save folder
        bool isFirstLoad = AppController._instance.IsFirstGeneralLoad();
        bool fixedRevo = PREFS.SetNewRevoChannels.GetBool();
        if (isFirstLoad || !fixedRevo) {
            PREFS.SetNewRevoChannels.SetBool(true);
            foreach (DefaultChannelScript defaultChannel in defaultChannels)
            {
                SaveDefaultChannel(defaultChannel, true, isFirstLoad);
                // we don't unpack the channels if it's not the first load, only the anims
            }
        }
        
        // And finally we check the themes in case they need to get updated
        CheckThemeUpdates();
    }

    private void LoadPreferences()
    {
        // we port over the legacy anims if needed, otherwise we just load them
        string pref = "LegacyPreferences";
        string path = PreferencesSerializer.GetPreferencesPath();
        bool isOld = !File.Exists(path) && PlayerPrefs.GetInt(pref, 1) == 1;
        if (isOld)
        {
            Debug.Log("Porting over legacy preferences to new system!");
            foreach (UnityPreference preference in PREFS.GetAllLegacyPreferences())
            {
                preference.LegacyToNew();
            }
            PreferencesSerializer.Save();

            // and we change the URL to base64
            string auth = PREFS.UserAuth.GetString();
            if (!string.IsNullOrEmpty(auth))
            {
                PREFS.UserAuth.SetString(auth.ToBase64());
            }
            
            // and we enable low memory mode
            Debug.Log("Preferences have been ported!");
        }
        else
        {
            // we load preferences
            PreferencesSerializer.Load();
            Debug.Log("Loaded preferences.json!");
        }
        
        // we reset the key so it doesn't try to port it again
        PlayerPrefs.SetInt(pref, 0);
        PlayerPrefs.Save();
    }

    private void UnpackEmuAnims()
    {
        try
        {
            // we ask to unpack the emu anims if they don't exist
            foreach (DefaultChannelScript defaultChannel in defaultChannels)
            {
                if (defaultChannel.iconFileName != null && defaultChannel.iconFileName.Contains("a4ec5432-418c-4122-aadd-d2023cd2730d")
                    || defaultChannel.bannerFileName != null && defaultChannel.bannerFileName.Contains("a4ec5432-418c-4122-aadd-d2023cd2730d"))
                {
                    SaveDefaultChannel(defaultChannel,AppController._instance.IsFirstLoadCurrentBundle());
                }
            }
        }
        catch (Exception e)
        {
            // doubt it'll raise, but with +360k downloads can't risk it lol
        }
    }

    private void Update()
    {
        if (isLoadingApps)
        {
            if (Input.GetKey(KeyCode.Menu) || Input.GetKey(KeyCode.Escape))
            {
                // Here we close the app and redirect to the launcher settings, in case they're stuck loading the apps.
                AndroidLinker._instance.OpenPhoneLauncherSettings();
            }
        }
    }

    private void CheckThemeUpdates(bool force = false)
    {
        // first we port the old themes
        PortTheme("theme_light(default)_12345.zip", "theme_riilight(default)_12345.zip");
        PortTheme("theme_matrix_1687889913136.zip", "theme_riimatrix_1687889913136.zip");
        PortTheme("theme_dark_1687812871419.zip", "theme_riidark_1687812871419.zip");
        
        foreach (DefaultThemeScript script in defaultThemes)
        {
            string destinationPath = SAVE_FOLDER_THEMES + script.themeFileName + ".zip";
            bool shouldCopy = force || !File.Exists(destinationPath) || AppController._instance.IsFirstLoadCurrentBundle(false);
            
            // removed because it wouldn't work well on new phones-
            /*if (!shouldCopy)
            {
                // we also check for possible updates
                string originalPath = Application.streamingAssetsPath + "/DefaultThemes/" + script.themeFileName + ".zip";
                AnimationBasicInfo zipTheme = GetBasicInfo(false, originalPath, true);
                AnimationBasicInfo savedTheme = GetBasicInfo(false, destinationPath, true);
                shouldCopy = zipTheme.innerVersion > savedTheme.innerVersion;
            }*/

            if (shouldCopy)
            {
                SaveDefaultTheme(script);
            }
        }
    }

    private void PortTheme(string oldName, string newName)
    {
        string oldPath = SAVE_FOLDER_THEMES + oldName;
        if (File.Exists(oldPath))
        {
            // we delete the old theme
            File.Delete(oldPath);
        }

        if (oldName.Equals(PREFS.CurrentTheme.GetString()))
        {
            // we change it to the new one
            PREFS.CurrentTheme.SetString(newName);
        }
    }
    
    public static AnimationBasicInfo GetBasicInfo(bool isAnim, string fileName, bool isFullPath = false)
    {
        // We try to find it in the loaded files
        if (isAnim)
        {
            ChannelAnimation loadedAnim = ChannelController._instance.GetLoadedChannelAnimation(fileName);
            if (loadedAnim != null)
            {
                return loadedAnim.GetBasicInfo();
            }
        }

        // Otherwise, we find the zip, and extract exclusively to read the basic animation info and return it
        string initialPath = fileName.StartsWith("icon_")
            ? SaveManager.SAVE_FOLDER_ANIMATIONS_ICONS
            : fileName.StartsWith("banner_") ? SaveManager.SAVE_FOLDER_ANIMATIONS_BANNERS : SaveManager.SAVE_FOLDER_THEMES;
        string zipPath = isFullPath ? fileName : initialPath + fileName;

        if (File.Exists(zipPath))
        {
            string extractPath = SaveManager.TEMP_ANIMATIONINFO + Path.GetFileNameWithoutExtension(zipPath);
            SaveManager.RecreateFolder(extractPath);
            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                string target = !isAnim ? "theme.json" : "animation.json";
                foreach (ZipArchiveEntry entry in archive.Entries.Where(e => e.Name.Equals(target)))
                {
                    SaveManager.UnzipEntry(entry,extractPath);
                    string filePath = Path.Combine(extractPath, entry.FullName);
                    JObject jObject = JObject.Parse(File.ReadAllText(filePath));
                    AnimationBasicInfo info = new AnimationBasicInfo
                    {
                        animationName = jObject["name"].ToString(),
                        animationFileName = fileName,
                        animationLastEdited = DateTime.Parse(jObject["lastEdited"].ToString()),
                        isOnline = archive.Entries.Any(e => e.Name.Equals("onlineinfo.json")),
                        isEmulator = IsBoxArt(fileName)
                    };
                    if (!isAnim)
                    {
                        int innerVersion = 1;
                        if (jObject.ContainsKey("innerVersion"))
                        {
                            innerVersion = (int) jObject["innerVersion"];
                        }
                        info.innerVersion = innerVersion;
                    }
                    
                    File.Delete(filePath);
                    return info;
                }
            }
        }
        return null;
    }

    private static void SaveDefaultChannel(DefaultChannelScript script, bool deleteOriginal = false, bool includeChannel = true)
    {
        if (!string.IsNullOrEmpty(script.iconFileName))
        {
            string originalPath = Application.streamingAssetsPath + "/DefaultAnimations/" + script.iconFileName + ".zip";
            string destinationPath = SAVE_FOLDER_ANIMATIONS_ICONS + script.iconFileName + ".zip";
            SendStreamingCopyRequest(originalPath,destinationPath,deleteOriginal);
        }
        if (!string.IsNullOrEmpty(script.bannerFileName))
        {
            string originalPath = Application.streamingAssetsPath + "/DefaultAnimations/" + script.bannerFileName + ".zip";
            string destinationPath = SAVE_FOLDER_ANIMATIONS_BANNERS + script.bannerFileName + ".zip";
            SendStreamingCopyRequest(originalPath,destinationPath,deleteOriginal);
        }
        if (!string.IsNullOrEmpty(script.channelFileName) && includeChannel)
        {
            string originalPath = Application.streamingAssetsPath + "/DefaultChannels/" + script.channelFileName + ".json";
            string destinationPath = SAVE_FOLDER_CHANNELS + script.channelFileName + ".json";
            SendStreamingCopyRequest(originalPath,destinationPath,deleteOriginal);
        }
    }

    public static void SaveDefaultTheme(DefaultThemeScript script)
    {
        if (!string.IsNullOrEmpty(script.themeFileName))
        {
            Debug.Log($"Saving default theme: {script.themeFileName}");
            string originalPath = Application.streamingAssetsPath + "/DefaultThemes/" + script.themeFileName + ".zip";
            string destinationPath = SAVE_FOLDER_THEMES + script.themeFileName + ".zip";
            SendStreamingCopyRequest(originalPath,destinationPath,true);
        }
    }

    public static void SendStreamingCopyRequest(string originalPath, string destinationPath, bool deleteOriginal = false)
    {
        try
        {
            // we create the target directory if it doesn't exist
            string dir = Path.GetDirectoryName(destinationPath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (File.Exists(destinationPath))
            {
                if (deleteOriginal)
                {
                    File.Delete(destinationPath);
                }
                else
                {
                    return;
                }
            }
            
            #if UNITY_ANDROID
            if (Application.isMobilePlatform)
            {
                Debug.Log($"Saving channel from {originalPath} to {destinationPath}!");
                UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(originalPath);
                www.SendWebRequest();
                while (!www.downloadHandler.isDone)
                {
                }

                File.WriteAllBytes(destinationPath, www.downloadHandler.data);
                return;
            }
            #endif
            
            FileManager.CopyFile(originalPath, destinationPath, true);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public static void RecreateFolder(string path)
    {
        DeleteFolder(path);
        CreateFolder(path);
    }

    public static void CreateFolder(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
    
    public static void DeleteFolder(string path)
    {
        if (Directory.Exists(path))
        {
            try
            {
                Directory.Delete(path,true);
            }
            catch (Exception _)
            {
                // ignored
            }
        }
    }
    
    private static void CreateNoMedia()
    {
        string path = SAVE_FOLDER + "/.nomedia";
        if (Directory.Exists(SAVE_FOLDER) && !File.Exists(path))
        {
            File.WriteAllText(path, "");
        }
    }

    public static string GetChannelImportedPath(ChannelAnimation channelAnimation)
    {
        // this only works for imported anims!
        return GetChannelSaveFolder(channelAnimation.type) + channelAnimation.GetImportedFileName();
    }

    public static string GetChannelZipPath(ChannelAnimation channelAnimation)
    {
        return GetChannelSaveFolder(channelAnimation.type) + channelAnimation.GetCurrentFileName();
    }
    
    public static string GetThemeImportedPath(Theme theme)
    {
        return SAVE_FOLDER_THEMES + theme.GetImportedFileName();
    }
    
    public static string GetThemeZipPath(Theme theme)
    {
        return SAVE_FOLDER_THEMES + theme.GetCurrentFileName();
    }

    public static void SaveChannelAnimation(ChannelAnimation channelAnimation)
    {
        Debug.Log("Saving " + channelAnimation.name + "!");
        string zipPath = GetChannelZipPath(channelAnimation);

        JsonSerializerSettings settings = JsonConvert.DefaultSettings.Invoke();
        settings.TypeNameHandling = TypeNameHandling.Auto;

        // NEW:
        string extractedPath = TEMP_SERIALIZE + channelAnimation.GetValidFileName();
        RecreateFolder(extractedPath);

        File.WriteAllText(extractedPath + "/animation.json",
            JsonConvert.SerializeObject(channelAnimation, settings));
        channelAnimation.SaveImages(extractedPath);
        channelAnimation.SaveAudio(extractedPath);

        if (File.Exists(zipPath))
        {
            File.Delete(zipPath);
        }
        ZipFile.CreateFromDirectory(extractedPath, zipPath, CompressionLevel.Fastest, false);

        DeleteFolder(extractedPath);
    }

    public static string SaveTheme(Theme theme)
    {
        // i hate that it's so similar to the method above, but rn I don't want to refactor stuff
        Debug.Log("Saving theme " + theme.name + "!");
        string zipPath = GetThemeZipPath(theme);

        JsonSerializerSettings settings = JsonConvert.DefaultSettings.Invoke();
        settings.TypeNameHandling = TypeNameHandling.Auto;

        string extractedPath = TEMP_SERIALIZE + theme.GetValidFileName();
        RecreateFolder(extractedPath);

        File.WriteAllText(extractedPath + "/theme.json",
            JsonConvert.SerializeObject(theme, settings));
        theme.DeleteLegacyAssets();
        theme.SaveImages(extractedPath);
        theme.SaveAudio(extractedPath);
        theme.SaveFonts(extractedPath);
        theme.SaveTexts(extractedPath);

        if (File.Exists(zipPath))
        {
            File.Delete(zipPath);
        }
        ZipFile.CreateFromDirectory(extractedPath, zipPath, CompressionLevel.Fastest, false);

        theme.DeleteLoadingFolder();
        DeleteFolder(extractedPath);
        return zipPath;
    }

    public static void DeleteChannelAnimationFile(ChannelAnimation animation)
    {
        DeleteChannelAnimationFile(animation.GetImportedFileName());
    }
    
    public static void DeleteChannelAnimationFile(string fileAnimation)
    {
        try
        {
            if (fileAnimation.StartsWith("temp_"))
            {
                File.Delete(TEMP_ANIMATIONS + fileAnimation);
            }
            else if (fileAnimation.StartsWith("icon_"))
            {
                File.Delete(SAVE_FOLDER_ANIMATIONS_ICONS + fileAnimation);
            }
            else
            {
                File.Delete(SAVE_FOLDER_ANIMATIONS_BANNERS + fileAnimation);
            }
        }
        catch (Exception ignored)
        {
            // ignored
        }
    }
    
    public static void DeleteTheme(Theme theme)
    {
        DeleteTheme(theme.GetImportedFileName());
    }
    
    public static void DeleteTheme(string theme)
    {
        try
        {
            File.Delete(SAVE_FOLDER_THEMES + theme);
        }
        catch (Exception ignored)
        {
            // ignored
        }
    }

    public static bool IsThemeInDisk(string fileName)
    {
        return File.Exists(SAVE_FOLDER_THEMES + fileName);
    }

    public static void ImportChannelAnimation(string path)
    {
        string fileName = FileBrowserHelpers.GetFilename(path);
        string initialPath = fileName.StartsWith("icon_")
            ? SAVE_FOLDER_ANIMATIONS_ICONS
            : SAVE_FOLDER_ANIMATIONS_BANNERS;
        if (!File.Exists(initialPath + fileName))
        {
            FileManager.CopyFile(path,initialPath + fileName);
        }
    }
    
    public static void ImportTheme(string path)
    {
        string fileName = FileManager.GetFileName(path);
        string exportPath = SAVE_FOLDER_THEMES + fileName;
        if (!File.Exists(exportPath))
        {
            FileManager.CopyFile(path,exportPath);
        }
    }

    public static string ConvertChannelAnimation(string name)
    {
        // This method moves channels between the icon and banner folders
        bool isIcon = name.StartsWith("icon_");
        string initialPath = isIcon
            ? SAVE_FOLDER_ANIMATIONS_ICONS
            : SAVE_FOLDER_ANIMATIONS_BANNERS;
        string zipPath = initialPath + name;

        if (!File.Exists(zipPath))
        {
            // Then it's nowhere to be found - return null
            return null;
        }
        
        string newPath = isIcon ? SAVE_FOLDER_ANIMATIONS_BANNERS : SAVE_FOLDER_ANIMATIONS_ICONS;
        string newName = isIcon ? "banner_" + name.Substring(5) : "icon_" + name.Substring(7);
        string finalZip = newPath + newName;

        if (File.Exists(finalZip))
        {
            File.Delete(finalZip);
        }
        FileManager.CopyFile(zipPath, finalZip, true);
        return newName;
    }
    
    public static Theme LoadTheme(string fileName, bool isFullPath = false)
    {
        try
        {
            string zipPath = isFullPath ? fileName : SAVE_FOLDER_THEMES + fileName;
            if (!File.Exists(zipPath))
            {
                // Then it's nowhere to be found - return null
                return null;
            }
            
            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto }; 
            if (fileName.EndsWith(".zip"))
            {
                // NEW:
                string extractedPath = TEMP_DESERIALIZE + Path.GetFileNameWithoutExtension(zipPath);
                RecreateFolder(extractedPath);

                UnzipFile(zipPath, extractedPath);
            
                Theme theme = JsonConvert.DeserializeObject<Theme>(File.ReadAllText(extractedPath + "/theme.json"), settings);
                theme.deserializedPath = extractedPath;
                theme.SetupImages();
                theme.SetupAudio();
                theme.SetupFonts();
                theme.SetupTexts();
                theme.importedFileName = FileManager.GetFileName(zipPath);
                if (File.Exists(extractedPath + "/onlineinfo.json"))
                {
                    OnlineInfo onlineInfo = JsonConvert.DeserializeObject<OnlineInfo>(File.ReadAllText(extractedPath + "/onlineinfo.json"), settings);
                    theme.onlineInfo = onlineInfo;
                }
                
                // Unfortunately we can't delete it because of sync tasks that run within the constructors
                // Directory.Delete(extractedPath,true);
                return theme;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
        return null;
    }

    public static ChannelAnimation LoadChannelAnimation(string fileName, bool isTemp = false)
    {
        try
        {
            string zipPath;
            if (!isTemp)
            {
                string initialPath = fileName.StartsWith("icon_")
                    ? SAVE_FOLDER_ANIMATIONS_ICONS
                    : SAVE_FOLDER_ANIMATIONS_BANNERS;
                zipPath = initialPath + fileName;
            }
            else
            {
                // We try to find it as temp file
                string initialPath = TEMP_ANIMATIONS;
                zipPath = initialPath + "temp_" + fileName;
            }
            
            if (!File.Exists(zipPath))
            {
                // Then it's nowhere to be found - return null
                return null;
            }
            
            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

            if (fileName.EndsWith(".json"))
            {
                Debug.Log("The old animations aren't supported anymore!");
            }
            else if (fileName.EndsWith(".zip"))
            {
                // NEW:
                string extractedPath = TEMP_DESERIALIZE + Path.GetFileNameWithoutExtension(zipPath);
                RecreateFolder(extractedPath);

                UnzipFile(zipPath, extractedPath);
            
                ChannelAnimation animation = JsonConvert.DeserializeObject<ChannelAnimation>(File.ReadAllText(extractedPath + "/animation.json"), settings);
                animation.deserializedPath = extractedPath;
                animation.SetupImages();
                animation.LoadAudio();
                animation.importedFileName = FileManager.GetFileName(zipPath);
                animation.isTemp = isTemp;
                if (File.Exists(extractedPath + "/onlineinfo.json"))
                {
                    OnlineInfo onlineInfo = JsonConvert.DeserializeObject<OnlineInfo>(File.ReadAllText(extractedPath + "/onlineinfo.json"), settings);
                    animation.onlineInfo = onlineInfo;
                }
                
                // Unfortunately we can't delete it because of sync tasks that run within the constructors
                // Directory.Delete(extractedPath,true);
                return animation;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
        return null;
    }

    public static void UnzipFile(string zipPath, string extractedPath)
    {
        try
        {
            ZipFile.ExtractToDirectory(zipPath, extractedPath);
        }
        catch (Exception exception)
        {
            // Apparently there's a few Android devices that don't fully support ZipFile.ExtractToDirectory,
            // so we do the extraction manually
            Debug.Log("IO Exception thrown! Using fallback method");
            FallbackUnzipAnimation(zipPath,extractedPath);
        }
    }

    public static void UnzipEntry(ZipArchiveEntry entry, string directory)
    {
        // First we get the file destination path
        string destinationFileName = Path.GetFullPath(Path.Combine(directory, entry.FullName));
        
        // Then we delete it if it already exists
        if (File.Exists(destinationFileName))
        {
            File.Delete(destinationFileName);
        }

        // Now we create the directory where the item should be, in case it's not already created
        string finalDirectory = Path.GetDirectoryName(destinationFileName);
        if (finalDirectory != null && !Directory.Exists(finalDirectory))
        {
            Directory.CreateDirectory(finalDirectory);
        }
        
        // And finally, we write the file
        using (Stream destination = File.Open(destinationFileName, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            using (var entryStream = entry.Open())
            {
                entryStream.CopyTo(destination);
            }
        }
    }

    private static void FallbackUnzipAnimation(string zipPath, string extractedPath)
    {
        using (ZipArchive archive = ZipFile.OpenRead(zipPath))
        {
            foreach (var entry in archive.Entries)
            {
                string fileExtractedPath = extractedPath + Path.DirectorySeparatorChar + entry.FullName;
                if (fileExtractedPath.Length > 255)
                {
                    // in some platforms like Windows, if the path is longer than 255 it crashes.
                    // so we skip the file: the animation or theme creator should shorten the filename
                    Debug.Log($"Skipping file as the extract path is longer than 255 chars: {entry.FullName}");
                    continue;
                }
                UnzipEntry(entry,extractedPath);
            }
        }
    }

    public static OnlineInfo GenerateOnlineInfo(string zipPath)
    {
        if (File.Exists(zipPath))
        {
            string extractPath = SaveManager.TEMP_ONLINEINFO + Path.GetFileNameWithoutExtension(zipPath);
            RecreateFolder(extractPath);
            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries.Where(e => e.Name.Equals("onlineinfo.json")))
                {
                    string filePath = Path.Combine(extractPath, entry.FullName);
                    UnzipEntry(entry,extractPath);
                    OnlineInfo onlineInfo = JsonConvert.DeserializeObject<OnlineInfo>(File.ReadAllText(filePath));
                    File.Delete(filePath);
                    return onlineInfo;
                }
            }
        }

        return null;
    }

    public static bool HasOnlineChannel(string id, CHANNELTYPE channeltype)
    {
        return File.Exists(GetChannelSaveFolder(channeltype) + $"{channeltype.ToString().ToLowerInvariant()}_{id}.zip");
    }
    
    public static bool HasOnlineTheme(string id)
    {
        return File.Exists(SAVE_FOLDER_THEMES + $"theme_{id}.zip");
    }

    public static string GetChannelSaveFolder(CHANNELTYPE channeltype)
    {
        return channeltype == CHANNELTYPE.ICON ? SAVE_FOLDER_ANIMATIONS_ICONS : SAVE_FOLDER_ANIMATIONS_BANNERS;
    }

    public static bool HasTempChannel(string id, CHANNELTYPE channeltype)
    {
        return File.Exists(TEMP_ANIMATIONS + $"temp_{channeltype.ToString().ToLowerInvariant()}_{id}.zip");
    }

    public static void LoadAllChannelAnimations(CHANNELTYPE channeltype)
    {
        // NEW ANIMATIONS:
        string[] newPaths = channeltype == CHANNELTYPE.ICON
            ? Directory.GetFiles(SAVE_FOLDER_ANIMATIONS_ICONS, "*.zip")
            : Directory.GetFiles(SAVE_FOLDER_ANIMATIONS_BANNERS, "*.zip");
        
        LoadAllChannelAnimations(newPaths);
    }

    private static void LoadAllChannelAnimations(string[] paths)
    {
        foreach (string path in paths)
        {
            ChannelController._instance.GetOrLoadChannelAnimation(FileManager.GetFileName(path));
        }
    }

    public static void SaveChannel(Channel channel)
    {
        string path = SAVE_FOLDER_CHANNELS + channel.GetFileName();
        JsonSerializerSettings settings = JsonConvert.DefaultSettings.Invoke();
        settings.TypeNameHandling = TypeNameHandling.Auto;
        string content = JsonConvert.SerializeObject(channel, settings);

        if (File.Exists(path))
        {
            File.Delete(path);
        }
        File.WriteAllText(path, content);
    }

    public static void DeleteChannel(Channel channel)
    {
        DeleteOldChannel(channel);
        try
        {
            File.Delete(SAVE_FOLDER_CHANNELS + channel.GetFileName());
        }
        catch (Exception ignored)
        {
            // ignored
        }
    }
    
    public static void DeleteOldChannel(Channel channel)
    {
        // we have to delete channels that use the old naming scheme
        string oldName = channel.GetOldFilename();
        if (!string.IsNullOrEmpty(oldName))
        {
            string path = SAVE_FOLDER_CHANNELS + oldName;
            if (File.Exists(path))
            {
                Debug.Log("Deleting old naming scheme channel!");
                File.Delete(path);
            }
        }
    }

    private static void CacheChannelInDictionary(Dictionary<string, List<Channel>> dictionary, string animName, Channel channel, bool shouldAdd)
    {
        if (string.IsNullOrEmpty(animName) || !shouldAdd)
        {
            return;
        }
        
        if (!dictionary.ContainsKey(animName))
        {
            dictionary[animName] = new List<Channel>();
        }
        dictionary[animName].Add(channel);
    }

    private static bool IsDuplicatePosition(List<Channel> channels, Channel channel)
    {
        foreach (Channel check in channels)
        {
            if (check.gridNumber == channel.gridNumber && check.position == channel.position)
            {
                return true;
            }
        }
        return false;
    }

    public static async void LoadAllChannels()
    {
        Debug.Log("Started loading channels!");
        bool lowMemoryMode = PREFS.CrashedDuringLoad.GetBool() || !PREFS.LoadBannersAtBoot.GetBool();
        bool ultraLowMemoryMode = AppController._instance.IsUltraLowMemoryMode();
        AppController._instance.SetLowMemoryMode(lowMemoryMode);
        Debug.Log($"Is using low memory mode? {lowMemoryMode}!");
        _instance.isLoadingApps = true;
        PREFS.CrashedDuringLoad.SetBool(true);
        
        Slider progressBar = WarningController._instance.progressBar;
        TextMeshProUGUI channelText = WarningController._instance.channelNumber;
        WarningController._instance.taskDescription.text = TextController.GetTranslation("general.loadinganims");
        DateTime startTime = System.DateTime.Now;
        
        await Task.Run(() =>
        {
            List<Channel> channelList = new List<Channel>();
            Dictionary<string, Channel> deleteList = new Dictionary<string, Channel>();
            Debug.Log($"Searching for channels at {SAVE_FOLDER_CHANNELS}...");
            string[] files = Directory.GetFiles(SAVE_FOLDER_CHANNELS, "*.json");
            Debug.Log($"Found {files.Length} channels!");
            
            // we load all the channels and cache the animation names
            Dictionary<string, List<Channel>> animationsToLoad = new Dictionary<string, List<Channel>>();
            
            for (int i = 0; i < files.Length; i++)
            {
                int value = i;
                string path = files[value];
                Debug.Log($"Loading channel {path}...");
                Channel channel = null;
                try
                {
                    // NEW:
                    JsonSerializerSettings settings = new JsonSerializerSettings
                        { TypeNameHandling = TypeNameHandling.Auto };
                    channel = JsonConvert.DeserializeObject<Channel>(File.ReadAllText(path), settings);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error when deserializing channel!\n{e}");
                }

                // we cache the animation names
                if (channel != null)
                {
                    // we check if it's a dupe, in that case we delete it
                    if (IsDuplicatePosition(channelList, channel))
                    {
                        deleteList.Add(path, channel);
                    }
                    else
                    {
                        // we cache it with its animations
                        CacheChannelInDictionary(animationsToLoad,channel.iconName,channel, !ultraLowMemoryMode);
                        CacheChannelInDictionary(animationsToLoad,channel.bannerName,channel, !lowMemoryMode && !ultraLowMemoryMode);
                        channelList.Add(channel);
                    }
                    // todo don't load animations that are outside of the chosen grid size?
                }
                else
                {
                    Debug.LogError("Unable to load channel!");
                }
            }
            
            // we delete all the duplicate ones
            foreach (string path in deleteList.Keys)
            {
                Channel channel = deleteList[path];
                Debug.Log($"Deleting duplicate {FileManager.GetFileName(path)} at pos = {channel.position}");
                File.Delete(path);
            }

            // we're gonna split the load in N threads
            int size = animationsToLoad.Keys.Count;
            int threads = 4;
            int columns = 1;
            Dictionary<int, List<string>> animationStrings = new Dictionary<int, List<string>>();
            int temp = 0;
            foreach (string animationName in animationsToLoad.Keys)
            {
                if (!animationStrings.ContainsKey(temp))
                {
                    animationStrings[temp] = new List<string>();
                }
                animationStrings[temp].Add(animationName);
                temp = (temp + 1) % threads;
                if (temp == 0)
                {
                    columns += 1;
                }
            }
            
            // now that the channels finished loading, we load all their animations in the threads
            byte[,] mask = new byte[threads,columns];
            for (int i = 0; i < threads; i++)
            {
                int thread = i;
                Task.Run(() =>
                {
                    for (int j = 0; j < animationStrings[thread].Count; j++)
                    {
                        string animationName = animationStrings[thread][j];
                        ChannelAnimation animation = ChannelController._instance.GetOrLoadChannelAnimation(animationName);
                        
                        // we assign the channel to the corresponding animations
                        foreach (Channel channel in animationsToLoad[animationName])
                        {
                            if (animationName.Equals(channel.iconName))
                            {
                                channel.SetIconAnimation(animation);
                            }
                            else
                            {
                                channel.SetBannerAnimation(animation);
                            }
                        }
                        
                        // Here we hack a way to stop the count from going
                        if (animation != null)
                        {
                            while (!animation.FinishedLoading())
                            {
                            }
                            
                            Debug.Log("Animation " + animationName + " finished loading!");
                        }
                        else
                        {
                            Debug.Log("Animation was null!");
                        }

                        // and we mark it as finished!
                        mask[thread,j] = 1;
                    }
                });
            }

            // we wait until all the animations have finished loading, and update the progress bar
            bool finished = false;
            int count = 0;
            float waitedTime = 0;
            while (!finished)
            {
                int currentCount = 0;
                foreach (var b in mask)
                {
                    currentCount += b;
                }

                if (count == currentCount)
                {
                    // deadlock timer. just in case.. i saw something weird last time
                    // we let users restart the app if it has been stuck loading anims for more than 10 seconds
                    waitedTime += 0.1f;
                    if (waitedTime >= 10)
                    {
                        waitedTime = 0f;
                        SyncContext.RunOnUnityThread(() =>
                        {
                            PopupController.ShowPopup("popup.deadlock", () =>
                            {
                                AndroidLinker._instance.RestartApp();
                            }, PopupController.ClosePopup);
                        });
                    }
                }
                else
                {
                    waitedTime = 0;
                }
                
                count = currentCount;
                
                finished = count == animationsToLoad.Count;
                SyncContext.RunOnUnityThread(() =>
                {
                    progressBar.value = count / (float) size;
                    channelText.text = count + "/" + size;
                });
                Thread.Sleep(100);
            }
            
            // and now we finish the boot
            SyncContext.RunOnUnityThread(() =>
            {
                Debug.Log("Finished loading channels!");
                TimeSpan delta = DateTime.Now - startTime;
                Debug.Log($"Loading duration: {delta}");
                
                Resources.UnloadUnusedAssets();
                PREFS.CrashedDuringLoad.SetBool(false);
                
                // If channelNumber == 1, it means no animation was loaded.
                // Can be caused by loading the app too early in the Android Launcher,
                // where it seems to fail to read the files
                if (count == 1)
                {
                    PopupController.ShowPopup("popup.nochannels",
                        () =>
                        {
                            AndroidLinker._instance.RestartApp();
                        }, () =>
                        {
                            PopupController.ClosePopup();
                            FinishChannelsLoad(channelList);
                        });
                }
                else
                {
                    FinishChannelsLoad(channelList);
                }
            });
        });
    }

    private static void FinishChannelsLoad(List<Channel> channelList)
    {
        foreach(Channel channel in channelList)
        {
            ChannelController._instance.SetupChannel(channel);
        }
        _instance.isLoadingApps = false;
        WarningController._instance.Disappear();
    }

    public string ExportData()
    {
        // We setup the zip target
        string zipPath = TEMP_GENERAL + Path.DirectorySeparatorChar + "RevoData.zip";
        if (File.Exists(zipPath))
        {
            File.Delete(zipPath);
        }
        RecreateFolder(TEMP_DATA);

        // We copy the channels and animations to a new folder
        string channelPath = Application.persistentDataPath + "/Channels/";
        string animPath = Application.persistentDataPath + "/Animations/";
        string themesPath = Application.persistentDataPath + "/Themes/";
        string coversPath = Application.persistentDataPath + "/GameCovers/";
        FileManager.CopyDirectory(channelPath, TEMP_DATA);
        FileManager.CopyDirectory(animPath, TEMP_DATA);
        FileManager.CopyDirectory(themesPath, TEMP_DATA);
        FileManager.CopyDirectory(coversPath, TEMP_DATA);
        
        // Then we export our preferences
        PreferencesSerializer.Save(TEMP_DATA);
        
        // Finally we create the zip
        ZipFile.CreateFromDirectory(TEMP_DATA, zipPath, CompressionLevel.Fastest, false);
        return zipPath;
    }

    public bool ImportData(string zipPath)
    {
        // if the file doesn't exist we leave
        if (!FileManager.Exists(zipPath))
        {
            PopupController.ShowPopup("popup.invalidimport");
            Debug.Log("Invalid import zip path!");
            return false;
        }
        
        // we're gonna copy the zip to a readable place and check its stats (thanks android SAF)
        RecreateFolder(TEMPZIP_GENERAL);
        string newZip = TEMPZIP_GENERAL + "TempZip.zip";
        bool isValid = true;
        try
        {
            // first we check the size
            FileManager.CopyFile(zipPath, newZip);
            if (new FileInfo(newZip).Length < 1024)
            {
                isValid = false;
                Debug.Log("Null zip! 0kb");
            }
            // now we try to unzip it (sometimes it errors when unzipping)
            RecreateFolder(TEMPZIP_EXTRACT);
            UnzipFile(newZip, TEMPZIP_EXTRACT);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            isValid = false;
        }

        // if it's not valid we don't continue the export
        if (!isValid)
        {
            PopupController.ShowPopup("popup.invalidimport");
            DeleteFolder(TEMPZIP_GENERAL);
            return false;
        }

        // If it's valid we move our directories
        ImportDataDirectory(TEMPZIP_ANIMATIONS, SAVE_FOLDER_ANIMATIONS);
        ImportDataDirectory(TEMPZIP_CHANNELS, SAVE_FOLDER_CHANNELS);
        ImportDataDirectory(TEMPZIP_THEMES, SAVE_FOLDER_THEMES);
        ImportDataDirectory(TEMPZIP_GAMECOVERS, SAVE_FOLDER_GAMECOVERS);
        if (File.Exists(SAVE_FOLDER_PREFERENCES))
        {
            File.Delete(SAVE_FOLDER_PREFERENCES);
        }
        File.Move(TEMPZIP_PREFERENCES, SAVE_FOLDER_PREFERENCES);
        
        // We empty our temp zip
        DeleteFolder(TEMPZIP_GENERAL);
        
        // Finally we load the player preferences
        LoadPreferences();
        
        // We're gonna re-import the default themes because they might get messed up from previous versions
        CheckThemeUpdates(true);
        
        // And we reset the fullscreen and scale
        PREFS.FullScreen.SetBool(false);
        PREFS.Scale.SetFloat(1);
        return true;
    }

    private void ImportDataDirectory(string import, string export)
    {
        if (!Directory.Exists(import))
        {
            return;
        }
        
        // we clear the export path
        DeleteFolder(export);
        
        // and we move our data
        Directory.Move(import, export);
    }

    private void DeleteData()
    {
        // we delete all folders that get overriden by the new zip, like preferences
        FileManager.EmptyDirectory(TEMP_GENERAL);
        FileManager.EmptyDirectory(SAVE_FOLDER_CHANNELS);
        FileManager.EmptyDirectory(SAVE_FOLDER_ANIMATIONS_ICONS);
        FileManager.EmptyDirectory(SAVE_FOLDER_ANIMATIONS_BANNERS);
        FileManager.EmptyDirectory(SAVE_FOLDER_THEMES);
        FileManager.EmptyDirectory(SAVE_FOLDER_GAMECOVERS);
        File.Delete(PreferencesSerializer.GetPreferencesPath());
    }

    public static bool IsBoxArt(string path)
    {
        return path.Equals(boxArtBanner) || path.Equals(boxArtIcon);
    }

    [Button("Save all Channel Animations")]
    public void SaveAllChannelAnimations()
    {
        LoadAllChannelAnimations(CHANNELTYPE.ICON);
        LoadAllChannelAnimations(CHANNELTYPE.BANNER);
        foreach (List<ChannelAnimation> animList in ChannelController._instance.loadedAnimationsList.Values)
        {
            foreach (ChannelAnimation animation in animList)
            {
                animation.Save();
            }
        }
    }

    [Button("Save all Channels")]
    public void SaveAllChannels()
    {
        foreach (Channel channel in ChannelController._instance.loadedChannels)
        {
            channel.Save();
        }
    }
}
