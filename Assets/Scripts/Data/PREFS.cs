using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Data;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

public static class PREFS
{
    // Booleans
    public static UnityBoolPreference FullScreen = new UnityBoolPreference("FullScreen", false);
    public static UnityBoolPreference HasTriedHolding = new UnityBoolPreference("HasTriedHolding", false);
    public static UnityBoolPreference FirstTimeOpening = new UnityBoolPreference("FirstTimeOpening", true);
    public static UnityBoolPreference FirstTimeCurrentVersion = new UnityBoolPreference("FirstTimeOpening_" + Application.version, true);
    public static UnityBoolPreference OpenDirectly = new UnityBoolPreference("OpenDirectly", false);
    public static UnityBoolPreference HasOpenedEditor = new UnityBoolPreference("HasOpenedEditor", false);
    public static UnityBoolPreference ShowCursor = new UnityBoolPreference("ShowCursor", false);
    public static UnityBoolPreference ShowNavigationBar = new UnityBoolPreference("ShowNavigationBar", false);
    public static UnityBoolPreference SetNewRevoChannels = new UnityBoolPreference("SetNewRevoChannels", false);
    public static UnityBoolPreference ShowLoadingBar = new UnityBoolPreference("ShowLoadingBar", true);
    public static UnityBoolPreference HasOpenedRatings = new UnityBoolPreference("HasOpenedRatings", false);
    public static UnityBoolPreference Is12Format = new UnityBoolPreference("Is12Format", false);
    public static UnityBoolPreference AllowHorizontal = new UnityBoolPreference("AllowHorizontal", false);
    public static UnityBoolPreference IsAmericanDate = new UnityBoolPreference("IsAmericanFormat", false);
    public static UnityBoolPreference CrashedDuringLoad = new UnityBoolPreference("CrashedDuringLoad", false);
    //public static UnityBoolPreference LowMemoryMode = new UnityBoolPreference("LowMemoryMode", true);
    public static UnityBoolPreference LoadBannersAtBoot = new UnityBoolPreference("LoadBannersAtBoot", false);
    public static UnityBoolPreference DecimalAsString = new UnityBoolPreference("DecimalAsString", false);
    public static UnityBoolPreference StandaloneBrowser = new UnityBoolPreference("StandaloneBrowser", false);
    public static UnityBoolPreference ShowDebugConsole = new UnityBoolPreference("ShowDebugConsole", false);
    public static UnityBoolPreference UseThemeWallpaper = new UnityBoolPreference("UseThemeWallpaper", false);
    public static UnityBoolPreference ShowSdCard = new UnityBoolPreference("ShowSdCard", true);
    public static UnityBoolPreference SdCardColor = new UnityBoolPreference("SdCardColor", true);
    public static UnityBoolPreference RotateLoadingCircle = new UnityBoolPreference("RotateLoadingCircle", true);
    public static UnityBoolPreference LoadChannelsAutomatically = new UnityBoolPreference("LoadChannelsAutomatically", false);
    public static UnityBoolPreference IsLauncherVersion = new UnityBoolPreference("IsLauncherVersion", false);
    public static UnityBoolPreference FirstTimeAppleLink = new UnityBoolPreference("FirstTimeAppleLink", true);
    
    // Boolean gestures
    public static UnityBoolPreference EnableHoldChannel = new UnityBoolPreference("EnableHoldChannel", true);
    public static UnityBoolPreference AllowSwipe = new UnityBoolPreference("AllowSwipe", true);
    public static UnityBoolPreference AllowDoubleClick = new UnityBoolPreference("AllowDoubleClick", true);
    public static UnityBoolPreference HoldSdCard = new UnityBoolPreference("HoldSdCard", true);
    public static UnityBoolPreference AllowSwipeDown = new UnityBoolPreference("AllowSwipeDown", true);
    public static UnityBoolPreference AllowDrag = new UnityBoolPreference("AllowDrag", true);
    public static UnityBoolPreference HoldEmptyChannel = new UnityBoolPreference("HoldEmpty", true);
    
    // String
    public static UnityStringPreference Language = new UnityStringPreference("Language", "Default");
    public static UnityStringPreference MessagingApp = new UnityStringPreference("messaging_app");
    public static UnityStringPreference UserId = new UnityStringPreference("UserId");
    public static UnityStringPreference UserAuth = new UnityStringPreference("UserAuth");
    public static UnityStringPreference UserName = new UnityStringPreference("UserName", "Not logged in");
    public static UnityStringPreference CurrentTheme = new UnityStringPreference("CurrentTheme", ThemeController.defaultThemeFileName);
    public static UnityStringPreference SteamLocation = new UnityStringPreference("SteamLocation");
    public static UnityStringPreference AltHubUrl = new UnityStringPreference("AltHubUrl");
    public static UnityStringPreference GridSize = new UnityStringPreference("GridSize", GRIDSIZE.BIG.ToString());
    
    // Integers
    public static UnityIntPreference NumberOfPages = new UnityIntPreference("NumberOfPages", 4);
    public static UnityIntPreference LastOpenBundle = new UnityIntPreference("LastOpenBundle", 0);
    public static UnityIntPreference TargetFramerate = new UnityIntPreference("TargetFramerate", 120);
    public static UnityIntPreference NumberAppsX = new UnityIntPreference("NumberAppsX", 2);
    public static UnityIntPreference NumberAppsXHorizontal = new UnityIntPreference("NumberAppsXHorizontal", -1);
    public static UnityIntPreference NumberAppsY = new UnityIntPreference("NumberAppsY", -1);
    public static UnityIntPreference NumberAppsYHorizontal = new UnityIntPreference("NumberAppsYHorizontal", 2);
    public static UnityIntPreference DownloadTimeout = new UnityIntPreference("DownloadTimeout", 50);

    // Floats
    public static UnityFloatPreference MusicVolume = new UnityFloatPreference("MusicVolume", 1f);
    public static UnityFloatPreference SfxVolume = new UnityFloatPreference("SfxVolume", 1f);
    public static UnityFloatPreference Scale = new UnityFloatPreference("Scale", 1f);
    public static UnityFloatPreference FadeoutSpeed = new UnityFloatPreference("FadeoutSpeed", 2f);
    public static UnityFloatPreference NunchuckSensitivity = new UnityFloatPreference("NunchuckSensitivity", 0.03f);
    public static UnityFloatPreference DPadSensitivity = new UnityFloatPreference("DPadSensitivity", 2.5f);

    private static List<UnityStringPreference> DynamicPreferences = new List<UnityStringPreference>();
    private static List<UnityPreference> AllPreferences = new List<UnityPreference>();

    private static JObject preferencesObject;

    public static List<UnityPreference> GetAllLegacyPreferences()
    {
        // we generate all the regular ones
        List<UnityPreference> prefs = GetAllPreferences();
        
        // now we have to find the dynamic ones (consoles, formats...)
        foreach (CONSOLETYPE type in Enum.GetValues(typeof(CONSOLETYPE)))
        {
            string name = "PreferredEmulator_" + type;
            if (PlayerPrefs.HasKey(name))
            {
                UnityStringPreference pref = new UnityStringPreference("PreferredEmulator_" + type, PlayerPrefs.GetString(name));
                prefs.Add(pref);
                DynamicPreferences.Add(pref);
            }
        }

        return prefs;
    }
    
    public static List<UnityPreference> GetAllPreferences()
    {
        if (AllPreferences.IsNullOrEmpty())
        {
            foreach (FieldInfo info in typeof(PREFS).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                try
                {
                    AllPreferences.Add((UnityPreference)info.GetValue(null));
                }
                catch (Exception e)
                {
                    // ignored
                }
            }
        }

        List<UnityPreference> all = new List<UnityPreference>();
        all.AddRange(AllPreferences);
        all.AddRange(DynamicPreferences);
        return all;
    }

    public static UnityPreference GetPreference(string name, bool generateIfNotFound = false)
    {
        foreach (UnityPreference preference in GetAllPreferences())
        {
            if (preference.GetName().Equals(name))
            {
                return preference;
            }
        }

        if (generateIfNotFound)
        {
            UnityStringPreference preference = new UnityStringPreference(name);
            DynamicPreferences.Add(preference);
            return preference;
        }
        
        return null;
    }

    public static UnityStringPreference GetEmulatorPreference(GameMetadata metadata)
    {
        if (metadata == null)
        {
            return null;
        }
        
        // we try to find by console or file first first
        string name;
        if (metadata.console != null && metadata.consoleType != CONSOLETYPE.UNKNOWN)
        {
            name = "PreferredEmulator_" + metadata.consoleType;
        }
        else
        {
            string extension = FileManager.GetExtension(StaticUtils.GetGameFileNameWithExtension(metadata.filePath)).ToUpperInvariant();
            name = "PreferredEmulator_" + extension;
        }
        
        // we retrieve and generate it
        UnityPreference preference = GetPreference(name);
        if (preference == null)
        {
            preference = new UnityStringPreference(name, "");
            DynamicPreferences.Add((UnityStringPreference)preference);
            PreferencesSerializer.Save();
        }
        return (UnityStringPreference)preference;
    }

    public static void ResetAllPreferences()
    {
        foreach(UnityPreference preference in GetAllPreferences())
        {
            preference.Reset();
        }
    }
}

public enum UNITYPREFTYPE
{
    INT, FLOAT, STRING, BOOL
}

public abstract class UnityPreference
{
    public string name;
    public UNITYPREFTYPE type;
    
    public string GetName()
    {
        return name;
    }
    public UNITYPREFTYPE GetType()
    {
        return type;
    }

    public abstract void Reset();
    public abstract void LegacyToNew();
    public abstract bool IsDefault();
    public abstract void PortValue(UnityPreference target);
}

public class UnityStringPreference : UnityPreference
{
    private string defaultString;
    public string currentValue;

    public UnityStringPreference(string name, string defaultString = null)
    {
        this.name = name;
        this.type = UNITYPREFTYPE.STRING;
        this.defaultString = defaultString;
        this.currentValue = defaultString;
    }

    public string GetDefaultString()
    {
        return defaultString;
    }
    public string GetString()
    {
        return currentValue;
    }
    public void SetString(string value, bool save=true)
    {
        bool same = currentValue != null && currentValue.Equals(value);
        currentValue = value;
        if (save && !same)
        {
            PreferencesSerializer.Save();
        }
    }

    public override void Reset()
    {
        SetString(GetDefaultString());
    }

    public override void LegacyToNew()
    {
        SetString(PlayerPrefs.GetString(name, defaultString), false);
    }

    public override bool IsDefault()
    {
        // a string is defaulted if its value corresponds to the default string; or if it's null, if the default string is -also- null
        return GetString()?.Equals(GetDefaultString()) ?? GetDefaultString() == null;
    }

    public override void PortValue(UnityPreference target)
    {
        if (target is UnityStringPreference pref)
        {
            pref.SetString(GetString(), false);
        }
    }
}

public class UnityIntPreference : UnityPreference
{
    protected int defaultInt;
    public int currentValue;
    public UnityIntPreference(string name, int defaultInt)
    {
        this.name = name;
        this.type = UNITYPREFTYPE.INT;
        this.defaultInt = defaultInt;
        this.currentValue = defaultInt;
    }
    
    public int GetDefaultInt()
    {
        return defaultInt;
    }
    public int GetInt()
    {
        return currentValue;
    }
    public void SetInt(int number, bool save=true)
    {
        bool same = currentValue == number;
        currentValue = number;
        if (save && !same)
        {
            PreferencesSerializer.Save();
        }
    }

    public override void Reset()
    {
        SetInt(GetDefaultInt());
    }

    public override void LegacyToNew()
    {
        SetInt(PlayerPrefs.GetInt(name, defaultInt), false);
    }

    public override bool IsDefault()
    {
        return GetInt() == GetDefaultInt();
    }

    public override void PortValue(UnityPreference target)
    {
        if (target is UnityIntPreference pref)
        {
            pref.SetInt(GetInt(), false);
        }
    }
}

public class UnityFloatPreference : UnityPreference
{
    private float defaultFloat;
    public float currentValue;
    
    public UnityFloatPreference(string name, float defaultFloat)
    {
        this.name = name;
        this.type = UNITYPREFTYPE.FLOAT;
        this.defaultFloat = defaultFloat;
        this.currentValue = defaultFloat;
    }

    public float GetDefaultFloat()
    {
        return defaultFloat;
    }
    public float GetFloat()
    {
        return currentValue;
    }
    public void SetFloat(float number, bool save=true)
    {
        bool same = currentValue == number;
        currentValue = number;
        if (save && !same)
        {
            PreferencesSerializer.Save();
        }
    }

    public override void Reset()
    {
        SetFloat(GetDefaultFloat());
    }

    public override void LegacyToNew()
    {
        SetFloat(PlayerPrefs.GetFloat(name, defaultFloat), false);
    }

    public override bool IsDefault()
    {
        return Math.Abs(GetFloat() - GetDefaultFloat()) < 0.01;
    }

    public override void PortValue(UnityPreference target)
    {
        if (target is UnityFloatPreference pref)
        {
            pref.SetFloat(GetFloat(), false);
        }
    }
}

public class UnityBoolPreference : UnityIntPreference
{
    public UnityBoolPreference(string name, bool defaultBool) : base(name, defaultBool ? 1 : 0) { }
    public bool GetDefaultBool()
    {
        return defaultInt > 0;
    }
    public bool GetBool()
    {
        return GetInt() > 0;
    }
    public void SetBool(bool value)
    {
        SetInt(value ? 1 : 0);
    }
}