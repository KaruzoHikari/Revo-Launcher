using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThemeCategories
{
    public static ThemeCategories General = new ThemeCategories("themes.category.general", new ThemeColor[]
    {
        ThemeColor.MainCursorColor, ThemeColor.MainCursorNumber, ThemeColor.MainCursorBackground, ThemeColor.MainCursorOutline,
        ThemeColor.ScrollbarMainColor, ThemeColor.ScrollbarBackColor, ThemeColor.BooleanTrueIcon, ThemeColor.BooleanTrueBackground,
        ThemeColor.BooleanFalseIcon, ThemeColor.BooleanFalseBackground, ThemeColor.PopupBars, ThemeColor.PopupBackground,
        ThemeColor.PopupText, ThemeColor.PopupButtons, ThemeColor.PopupButtonsText
    });
    
    public static ThemeCategories Home = new ThemeCategories("themes.category.home", new ThemeColor[]
    {
        ThemeColor.MainAccent, ThemeColor.HomeBackground, ThemeColor.HomeArrows, ThemeColor.ArrowSignIcon, ThemeColor.ArrowSignButtons,
        ThemeColor.HomeBar, ThemeColor.HomeClip, ThemeColor.HomeClipBorders, ThemeColor.HomeCircleButtonsIcons, ThemeColor.HomeCircleButtonsBackground,
        ThemeColor.HomeGridShadow, ThemeColor.DateText, ThemeColor.ClockText, ThemeColor.BannerBar, ThemeColor.ChannelBackground,
        ThemeColor.ChannelBorders, ThemeColor.ChannelHighlight, ThemeColor.LiftChannel, ThemeColor.ChannelTagButton, ThemeColor.ChannelTagText,
        ThemeColor.BannerButtons, ThemeColor.BannerButtonsText, ThemeColor.SdCardCover, ThemeColor.SdCardTag, ThemeColor.SdCardCoverDisabled
    });
    
    public static ThemeCategories Warning = new ThemeCategories("themes.category.warning", new ThemeColor[]
    {
        ThemeColor.WarningScreenBackground, ThemeColor.WarningScreenTitleIcon, ThemeColor.WarningScreenTitleText, ThemeColor.WarningScreenDescriptionText,
        ThemeColor.WarningScreenAlsoAtText, ThemeColor.WarningScreenProgressUnfilled, ThemeColor.WarningScreenProgressFilled,
        ThemeColor.WarningScreenLoadingText, ThemeColor.WarningScreenPressText
    });
    
    public static ThemeCategories Settings = new ThemeCategories("themes.category.settings", new ThemeColor[]
    {
        ThemeColor.SettingsBackgroundDark, ThemeColor.SettingsBackgroundLight, ThemeColor.SettingsBackgroundThemes, ThemeColor.SettingsButtons,
        ThemeColor.SettingsButtonsIcons, ThemeColor.SettingsButtonsText, ThemeColor.SettingsBars, ThemeColor.SettingsBarIcon,
        ThemeColor.SettingsTitleTag, ThemeColor.SettingsTitleText, ThemeColor.SettingsHelpButton,
        ThemeColor.SettingsHelpIcon, ThemeColor.SettingsDiscordIcon, ThemeColor.SettingsWrenchIcon, ThemeColor.SettingsStarIcon,
        ThemeColor.SettingsHomeIcon, ThemeColor.SettingsRestartIcon, ThemeColor.SettingsGestureIcon, ThemeColor.SettingsButtonBoxes,
        ThemeColor.SettingsButtonBoxesText, ThemeColor.SettingsSliderKnob, ThemeColor.SettingsSliderBackground,
        ThemeColor.SettingsLocalAnimation, ThemeColor.SettingsOnlineAnimation, ThemeColor.SettingsBackgroundEdit
    });
    
    public static ThemeCategories Editor = new ThemeCategories("themes.category.editor", new ThemeColor[]
    {
        ThemeColor.EditorBackground, ThemeColor.EditorText, ThemeColor.EditorSeparator, ThemeColor.EditorButtons,
        ThemeColor.EditorButtonsText, ThemeColor.EditorPreviewButton, ThemeColor.EditorPreviewButtonText, ThemeColor.EditorButtonBoxes,
        ThemeColor.EditorButtonBoxesText
    });
    
    public static ThemeCategories Hub = new ThemeCategories("themes.category.hub", new ThemeColor[]
    {
        ThemeColor.HubAccent, ThemeColor.HubBackground, ThemeColor.HubText, ThemeColor.HubLoadingIcon, ThemeColor.HubSeparators,
        ThemeColor.HubButtons,  ThemeColor.HubButtonsText, ThemeColor.HubSearchButtonIcon, ThemeColor.HubUploadUpdateIcons, ThemeColor.HubButtonBoxes,
        ThemeColor.HubButtonBoxesText, ThemeColor.HubAnimationListText, ThemeColor.HubAnimationListInstalled, ThemeColor.HubAnimationListUninstalled,
        ThemeColor.HubVerifiedBadge, ThemeColor.HubPagesIcon, ThemeColor.HubDownloadsIcon, ThemeColor.HubHeartIcon, ThemeColor.HubUnheartIcon,
        ThemeColor.HubBombIcon, ThemeColor.HubStarIcon, ThemeColor.HubUnstarIcon, ThemeColor.HubTrashIcon,
        ThemeColor.HubAnimationInfoBackground, ThemeColor.HubAnimationInfoText,
        ThemeColor.HubAnimationButtons, ThemeColor.HubAnimationButtonsMidBorder, ThemeColor.HubAnimationButtonsText, ThemeColor.HubAnimationButtonsIcons,
        ThemeColor.HubReviewsMaxStars, ThemeColor.HubReviewsMinStars, ThemeColor.HubReviewsSeparator, ThemeColor.HubReviewsText
    });
    
    private static ThemeCategories[] categories = new ThemeCategories[] { General, Home, Warning, Settings, Editor, Hub };
    private string titleId;
    private ThemeColor[] colors;
    
    public ThemeCategories(string titleId, ThemeColor[] colors)
    {
        this.titleId = titleId;
        this.colors = colors;
    }

    public ThemeColor[] GetColors()
    {
        return colors;
    }

    public string GetTitleId()
    {
        return titleId;
    }

    public static ThemeCategories[] GetCategories()
    {
        return categories;
    }

    public static ThemeCategories GetCategory(ThemeColor color)
    {
        foreach (ThemeCategories cat in categories)
        {
            foreach (ThemeColor checkColor in cat.colors)
            {
                if (checkColor.Equals(color))
                {
                    return cat;
                }
            }
        }

        return null;
    }

    public static Dictionary<ThemeCategories, List<ThemeColor>> GetEmptyCategories()
    {
        Dictionary<ThemeCategories, List<ThemeColor>> dict = new Dictionary<ThemeCategories, List<ThemeColor>>();
        foreach (ThemeCategories cat in GetCategories())
        {
            dict[cat] = new List<ThemeColor>();
        }
        return dict;
    }

}