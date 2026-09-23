using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Animations;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

public class Theme : ContentContainer
{
    public bool rotatesHubCircle;
    public ThemeTexture wallpaper;
    
    // This is used for the default animations, so that they can get updated in the future
    public bool isDefaultTheme = false;
    public int innerVersion = 1;

    public Dictionary<ThemeColor, Color> colorMap = new Dictionary<ThemeColor, Color>();
    public List<ThemeTexture> replacementTextures = new List<ThemeTexture>();
    public List<ThemeAudio> replacementAudio = new List<ThemeAudio>();
    public List<ThemeTransform> replacementTransform = new List<ThemeTransform>();
    public List<ThemeFont> replacementFont = new List<ThemeFont>();
    public List<ThemeText> replacementText = new List<ThemeText>();

    public override void RunFirstSetup()
    {
        name = "New THEME";
        base.RunFirstSetup();
    }

    public override string GetCurrentFileName()
    {
        return "theme_" + GetValidFileName() + "_" + id + ".zip";
    }

    public void UpdateMaps()
    {
        try
        {
            // we add the missing colors to the color map, if any
            foreach (ThemeColor themeColors in Enum.GetValues(typeof(ThemeColor)))
            {
                if (!colorMap.ContainsKey(themeColors))
                {
                    Debug.Log($"Map didn't contain {themeColors}, adding!");
                    colorMap.Add(themeColors, ThemeController.GetDefaultColor(themeColors));
                }
            }
            
            // we add the transforms
            List<ThemeOffsets> trans = Enum.GetValues(typeof(ThemeOffsets)).Cast<ThemeOffsets>().ToList();
            foreach (ThemeTransform check in replacementTransform)
            {
                trans.Remove(check.category);
            }
            foreach (ThemeOffsets cat in trans)
            {
                replacementTransform.Add(new ThemeTransform(cat));
            }
            
            // we copy over legacy transforms to new ones
            UpdateLegacyTransforms();
        }
        catch (Exception e)
        {
            // some people having issues here? I'll skip it if any error shows up
        }
    }

    private void UpdateLegacyTransforms()
    {
        // we have stored them with the legacy names
        foreach (ThemeTransform newTransform in replacementTransform)
        {
            try
            {
                string oldName = ThemeController._instance.GetLegacyName(newTransform.category.ToString());
                if (!string.IsNullOrEmpty(oldName))
                {
                    ThemeOffsets oldCategory = (ThemeOffsets)Enum.Parse(typeof(ThemeOffsets), oldName);
                    ThemeTransform oldTransform = FindTransform(oldCategory);
                    if (!oldTransform.HasDefaultValues())
                    {
                        // found legacy transform with values, update and clear values for old transform
                        newTransform.RetrieveValues(oldTransform);
                        oldTransform.SetDefaultValues();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to retrieve values from legacy transform!\n" + e);
            }
        }
    }

    public ThemeText GetThemeText(SystemLanguage language)
    {
        foreach (ThemeText text in replacementText)
        {
            if (text.language.Equals(language))
            {
                return text;
            }
        }
        return null;
    }

    public Color GetColor(ThemeColor themeColor)
    {
        if (colorMap.ContainsKey(themeColor))
        {
            return colorMap[themeColor];
        }
        else
        {
            return ThemeController._instance.defaultTheme.Equals(this) ? Color.white : ThemeController.GetDefaultColor(themeColor);
        }
    }

    public ThemeTransform GetThemeTransform(ThemeOffsets category)
    {
        // we return a replacement, or null if no replacement
        // we try to find from the replacement names
        ThemeTransform transform = FindTransform(category);
        if (transform == null)
        {
            // we're gonna try to retrieve a legacy one.. might be troublesome so we trycatch
            try
            {
                string oldName = ThemeController._instance.GetLegacyName(category.ToString());
                if (!string.IsNullOrEmpty(oldName))
                {
                    ThemeOffsets oldCategory = (ThemeOffsets)Enum.Parse(typeof(ThemeOffsets), oldName);
                    transform = FindTransform(oldCategory);
                }
            }
            catch (Exception ignored)
            {
            }
        }
        return transform;
    }
    
    private ThemeTransform FindTransform(ThemeOffsets category)
    {
        foreach (ThemeTransform trans in replacementTransform)
        {
            if (trans.category.Equals(category))
            {
                return trans;
            }
        }
        return null;
    }
    
    public ThemeFont GetThemeFont(ThemeColor place)
    {
        foreach (ThemeFont font in replacementFont)
        {
            if (font.isSpecialFont && font.specialPlace.Equals(place))
            {
                return font;
            }
        }
        return null;
    }
    
    public ThemeFont GetThemeFont(string fontName)
    {
        foreach (ThemeFont font in replacementFont)
        {
            if (!font.isSpecialFont && font.originalFontName.Equals(fontName))
            {
                return font;
            }
        }
        return null;
    }

    public Sprite GetTexture(Texture texture)
    {
        return texture == null ? null : GetTexture(texture.name);
    }
    
    public Sprite GetTexture(string texName)
    {
        // we return a replacement, or null if no replacement
        // we try to find from the replacement names
        Sprite tex = FindTexture(texName) ?? FindTexture(ThemeController._instance.GetLegacyName(texName));
        return tex;
    }

    private Sprite FindTexture(string texName)
    {
        if (!string.IsNullOrEmpty(texName))
        {
            foreach (ThemeTexture tex in replacementTextures)
            {
                string name1 = tex.textureName;
                string name2 = texName;
                if (name1.Equals(name2))
                {
                    return tex.texture;
                }
            }
        }

        return null;
    }
    
    public AudioClip GetSound(AudioClip audioClip)
    {
        if (audioClip is null)
        {
            return null;
        }
        
        // we return the current one or the old one
        AudioClip sound = FindSound(audioClip.name) ?? FindSound(ThemeController._instance.GetLegacyName(audioClip.name));
        return sound;
    }

    private AudioClip FindSound(string soundName)
    {
        if (!string.IsNullOrEmpty(soundName))
        {
            // we return a replacement, or null if no replacement
            foreach (ThemeAudio audio in replacementAudio)
            {
                if (audio.audioClipName.Equals(soundName))
                {
                    return audio.audioClip;
                }
            }
        }

        return null;
    }
    
    [OnDeserialized]
    internal void OnDeserializedMethod(StreamingContext context)
    {
        SyncContext.RunOnUnityThread(() =>
        {
            // we link all the assets!
            replacementTextures.ForEach(asset => asset.LinkTheme(this));
            replacementAudio.ForEach(asset => asset.LinkTheme(this));
            replacementFont.ForEach(asset => asset.LinkTheme(this));
        });
    }
    
    public void SetupImages()
    {
        foreach(ThemeTexture texture in replacementTextures)
        {
            texture.SetupImage(deserializedPath);
        }
        wallpaper?.SetupImage(deserializedPath);
    }
    
    public void SetupAudio()
    {
        foreach(ThemeAudio audio in replacementAudio)
        {
            audio.LoadAudio(deserializedPath);
        }
    }
    
    public void SetupFonts()
    {
        foreach(ThemeFont font in replacementFont)
        {
            font.LoadFont(deserializedPath);
        }
    }
    
    public void SetupTexts()
    {
        foreach(ThemeText text in replacementText)
        {
            text.LoadText(deserializedPath);
        }
    }

    public void DeleteLegacyAssets()
    {
        // we have to check if we have content using both the NEW and OLD name. in that case we need to delete the old ones!
        // first textures
        List<ThemeTexture> deleteTex = new List<ThemeTexture>();
        foreach (ThemeTexture texture in replacementTextures)
        {
            string legacy = ThemeController._instance.GetLegacyName(texture.textureName);
            if (!string.IsNullOrEmpty(legacy))
            {
                ThemeTexture delete = GetThemeTexture(legacy);
                if (delete != null)
                {
                    Debug.Log($"Deleting legacy texture {legacy}");
                    deleteTex.Add(delete);
                }
            }
        }
        deleteTex.ForEach(tex => replacementTextures.Remove(tex));
        
        // now audio
        List<ThemeAudio> deleteAudio = new List<ThemeAudio>();
        foreach (ThemeAudio audio in replacementAudio)
        {
            string legacy = ThemeController._instance.GetLegacyName(audio.audioClipName);
            if (!string.IsNullOrEmpty(legacy))
            {
                ThemeAudio delete = GetThemeAudio(legacy);
                if (delete != null)
                {
                    Debug.Log($"Deleting legacy audio {legacy}");
                    deleteAudio.Add(delete);
                }
            }
        }
        deleteAudio.ForEach(tex => replacementAudio.Remove(tex));
    }
    
    public void SaveImages(string extractedPath)
    {
        foreach(ThemeTexture texture in replacementTextures)
        {
            texture.SaveImage(extractedPath);
        }
        wallpaper?.SaveImage(extractedPath);
    }
    
    public void SaveAudio(string extractedPath)
    {
        foreach(ThemeAudio audio in replacementAudio)
        {
            audio.SaveAudio(extractedPath);
        }
    }
    
    public void SaveFonts(string extractedPath)
    {
        foreach(ThemeFont font in replacementFont)
        {
            font.SaveFont(extractedPath);
        }
    }
    
    public void SaveTexts(string extractedPath)
    {
        foreach(ThemeText text in replacementText)
        {
            text.SaveText(extractedPath);
        }
    }
    
    public bool FinishedLoading()
    {
        foreach(ThemeTexture texture in replacementTextures)
        {
            if (!texture.finishedLoading)
            {
                return false;
            }
        }
        foreach(ThemeAudio audio in replacementAudio)
        {
            if (!audio.finishedLoading)
            {
                return false;
            }
        }
        foreach(ThemeFont font in replacementFont)
        {
            if (!font.finishedLoading)
            {
                return false;
            }
        }
        if (wallpaper != null && !wallpaper.finishedLoading)
        {
            return false;
        }
        return true;
    }

    public void ReplaceTexture(string texName, string path, bool isWallpaper = false, bool isGif = false, bool isVideo = false)
    {
        ThemeTexture themeTexture = new ThemeTexture();
        themeTexture.imageFileName = FileManager.GetFileName(path);
        themeTexture.textureName = texName;
        themeTexture.isGif = isGif;
        themeTexture.isVideo = isVideo;
        themeTexture.LinkTheme(this);
        
        // we copy it to a temp place
        MoveToLoadingFolder(path);
        themeTexture.SetupImage(GetLoadingFolder());

        if (isWallpaper)
        {
            wallpaper?.Unload();
            wallpaper = themeTexture;
        }
        else
        {
            DeleteTexture(texName);
            replacementTextures.Add(themeTexture);
        }
    }

    public void ReplaceAudio(string path, string originalName)
    {
        DeleteAudio(originalName);

        ThemeAudio themeAudio = new ThemeAudio();
        themeAudio.audioFileName = FileManager.GetFileName(path);
        themeAudio.audioClipName = originalName;
        themeAudio.LinkTheme(this);
        
        // we copy it to a temp place
        MoveToLoadingFolder(path);
        themeAudio.LoadAudio(GetLoadingFolder());
        replacementAudio.Add(themeAudio);
    }
    
    public void ReplaceText(ThemeText themeText)
    {
        // we just remove the current one if it exists, and use the new one
        ThemeText text = GetThemeText(themeText.language);
        if (text is not null)
        {
            text.Unload();
            replacementText.Remove(text);
        }
        replacementText.Add(themeText);
    }

    public void ReplaceFont(string path, TMP_FontAsset originalFont)
    {
        ReplaceFont(path, originalFont.name, false, originalFont, ThemeColor.HomeBackground);
    }

    public void ReplaceSpecialFont(string path, ThemeColor place)
    {
        ReplaceFont(path, place.ToString(), true, null, place);
    }

    private void ReplaceFont(string path, string originalName, bool isSpecial, TMP_FontAsset originalFont, ThemeColor place)
    {
        if (isSpecial)
        {
            DeleteFont(place);
        }
        else
        {
            DeleteFont(originalName);
        }

        ThemeFont themeFont = new ThemeFont();
        string fontName = FileManager.GetFileName(path);
        themeFont.fontFileName = fontName;
        themeFont.originalFontName = originalName;
        themeFont.newFontName = fontName;
        themeFont.LinkTheme(this);
        if(isSpecial)
        {
            themeFont.isSpecialFont = true;
            themeFont.specialPlace = place;
        }
        else
        {
            themeFont.originalLineHeight = originalFont.faceInfo.lineHeight;
        }
        
        // we copy it to a temp place
        MoveToLoadingFolder(path);
        themeFont.LoadFont(GetLoadingFolder());
        replacementFont.Add(themeFont);
    }

    public void DeleteTexture(string textureName)
    {
        ThemeTexture existingReplacement = GetThemeTexture(textureName);
        if (existingReplacement != null)
        {
            replacementTextures.Remove(existingReplacement);
            existingReplacement.Unload();
        }
    }

    public void DeleteWallpaper()
    {
        wallpaper?.Unload();
        wallpaper = null;
    }
    
    public void DeleteAudio(string name)
    {
        ThemeAudio existingReplacement = GetThemeAudio(name);
        if (existingReplacement != null)
        {
            replacementAudio.Remove(existingReplacement);
            existingReplacement.Unload();
        }
    }
    
    public void DeleteFont(string fontName)
    {
        ThemeFont existingReplacement = GetThemeFont(fontName);
        if (existingReplacement != null)
        {
            replacementFont.Remove(existingReplacement);
            existingReplacement.Unload();
        }
    }
    
    public void DeleteFont(ThemeColor place)
    {
        ThemeFont existingReplacement = GetThemeFont(place);
        if (existingReplacement != null)
        {
            replacementFont.Remove(existingReplacement);
            existingReplacement.Unload();
        }
    }

    public ThemeTexture GetThemeTexture(string texName)
    {
        foreach (ThemeTexture themeTexture in replacementTextures)
        {
            if (themeTexture.textureName.Equals(texName))
            {
                return themeTexture;
            }
        }
        return null;
    }
    
    public ThemeAudio GetThemeAudio(string audioName)
    {
        foreach (ThemeAudio audio in replacementAudio)
        {
            if (audio.audioClipName.Equals(audioName))
            {
                return audio;
            }
        }
        return null;
    }

    public void Unload()
    {
        replacementTextures.ForEach(texture => texture.Unload());
        replacementAudio.ForEach(audio => audio.Unload());
    }
    
    public bool Save(bool asCopy = false, bool toStreamingAssets = false)
    {
        Debug.Log("Saving theme!");
        try
        {
            string oldName = GetImportedFileName();
            bool shouldDeleteOld = !string.IsNullOrEmpty(requestedNewName) && !requestedNewName.Equals(name);

            AssignSaveValues();

            bool wasDefault = isDefaultTheme;
            if (asCopy)
            {
                isDefaultTheme = false;
            }

            // Then, we save the current one
            string zipPath = SaveManager.SaveTheme(this);
            if (toStreamingAssets)
            {
                FileManager.CopyFile(zipPath, Application.streamingAssetsPath + "/DefaultThemes/" + FileManager.GetFileName(zipPath), true);
            }
        
            importedFileName = GetCurrentFileName();
            isDefaultTheme = wasDefault;
        
            // If the name has been changed, we need to remove the previous files
            if(!asCopy && shouldDeleteOld) {
                SaveManager.DeleteTheme(oldName);
            }

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            PopupController.ShowPopup("popup.savingerror");
            return false;
        }
    }
    
    public bool IsOnlineTheme()
    {
        return onlineInfo != null;
    }

    public bool HasDifferentColors()
    {
        // we iterate all the colors and compare them to the default ones
        foreach(ThemeColor themeColor in colorMap.Keys)
        {
            Color myColor = GetColor(themeColor);
            Color defaultColor = ThemeController.GetDefaultColor(themeColor);
            if (!myColor.Equals(defaultColor))
            {
                return true;
            }
        }
        return false;
    }
    
    public bool HasDifferentTextures()
    {
        return replacementTextures.Count > 0 || wallpaper != null;
    }
    
    public bool HasDifferentAudio()
    {
        return replacementAudio.Count > 0;
    }

    public void UpdateGifs()
    {
        replacementTextures.ForEach(x => x.UpdateGif());
        wallpaper?.UpdateGif();
    }
}

public enum ThemeColor
{
    // KAZ: Don't forget adding it to a category later in order to use it!
    MainAccent, HomeArrows, HomeBackground, HomeBar, HomeCircleButtonsBackground, HomeCircleButtonsIcons, BannerBar, BannerButtons,
    SettingsBackgroundLight, SettingsButtons, EditorPreviewButtonText, HubBackground, HubButtons, HubAccent, ClockText,
    DateText, ChannelBorders, ChannelBackground, ChannelHighlight, ChannelTagButton, ChannelTagText, LiftChannel,
    BannerButtonsText, ArrowSignButtons, ArrowSignIcon, HomeClip, HomeClipBorders, EditorBackground, EditorButtons, EditorText,
    EditorButtonsText, EditorPreviewButton, SettingsButtonsText, SettingsButtonsIcons, SettingsBars, SettingsBackgroundDark,
    SettingsTitleTag, SettingsTitleText, SettingsHelpButton, SettingsHelpIcon, ScrollbarMainColor, ScrollbarBackColor,
    EditorSeparator, SettingsLocalAnimation, SettingsOnlineAnimation, MainCursorColor, MainCursorNumber, MainCursorBackground,
    PopupBars, PopupBackground, PopupText, PopupButtons, PopupButtonsText, BooleanTrueBackground, BooleanTrueIcon,
    BooleanFalseBackground, BooleanFalseIcon, SettingsSliderKnob, SettingsSliderBackground, SettingsButtonBoxes,
    SettingsBarIcon, SettingsDiscordIcon, SettingsStarIcon, HubSeparators, HubButtonsText, HubSearchButtonIcon, HubText,
    HubButtonBoxes, SettingsBackgroundThemes, HubStarIcon, HubDownloadsIcon, HubHeartIcon, HubBombIcon, HubReviewsSeparator,
    HubAnimationButtons, HubAnimationButtonsText, HubAnimationButtonsMidBorder, HubPagesIcon, HubLoadingIcon,
    EditorButtonBoxes, SettingsButtonBoxesText, HubButtonBoxesText, EditorButtonBoxesText, HubTrashIcon,
    HubAnimationListInstalled, HubUnheartIcon, HubUnstarIcon, HubAnimationListUninstalled, HubAnimationListText,
    HubReviewsMaxStars, HubReviewsMinStars, HubReviewsText, SettingsWrenchIcon, SettingsHomeIcon, HubUploadUpdateIcons,
    HubAnimationInfoBackground, HubAnimationInfoText, HomeGridShadow, HubAnimationButtonsIcons, MainCursorOutline,
    WarningScreenTitleIcon, WarningScreenTitleText, WarningScreenDescriptionText, WarningScreenProgressFilled, WarningScreenLoadingText,
    WarningScreenPressText, WarningScreenAlsoAtText, WarningScreenProgressUnfilled, WarningScreenBackground,
    HubVerifiedBadge, SdCardCover, SdCardTag, SdCardCoverDisabled, SettingsBackgroundEdit, SettingsRestartIcon,
    SettingsGestureIcon
}

public enum ThemeOffsets
{
    Date, Clock, WiiClip, WiiButton, MailClip, MailButton, WiiShadow, MailShadow, SdCard,
    RevoClip, RevoButton, RevoShadow /* these replace the 3 above that should NOT be used anymore! kept here for legacy reasons. */
}

public enum ThemePositions
{
    TopLeft, TopMid, TopRight, MidLeft, Neutral, MidRight, BottomLeft, BottomMid, BottomRight
}