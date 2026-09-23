using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Data;
using Misc.SFO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public partial class GameMetadata
{
    public string filePath;
    public string gameId;
    [JsonIgnore] public Console console;
    public CONSOLETYPE consoleType = CONSOLETYPE.UNKNOWN;
    public string title;
    public string description;
    public string language;
    public Color boxColor;
    public bool rotateBox;
    public COVERSOURCE coverSource = COVERSOURCE.UNKNOWN;

    [JsonIgnore] public Texture2D frontCover;
    [JsonIgnore] public Texture2D backCover;
    [JsonIgnore] public Texture2D fullCover;

    [JsonIgnore] public Channel linkedChannel;

    public GameMetadata(string filePath)
    {
        this.filePath = filePath;
    }

    public void SetConsole(Console newConsole)
    {
        this.console = newConsole;
        this.consoleType = newConsole.type;
        this.boxColor = newConsole.boxColor;
        this.rotateBox = newConsole.isTdbCover;

        if (newConsole.type == CONSOLETYPE.STEAM)
        {
            this.coverSource = COVERSOURCE.STEAM;
        }
        else
        {
            this.coverSource = newConsole.isTdbCover ? COVERSOURCE.GAMETDB : COVERSOURCE.LIBRETRO;
        }
    }

    [OnDeserialized]
    internal void OnDeserializedMethod(StreamingContext context)
    {
        // we link the channel to the target so they can access info from here if needed
        this.console = CONSOLES.GetConsole(consoleType);
        if (coverSource == COVERSOURCE.UNKNOWN)
        {
            coverSource = IsConsoleTdbCover() ? COVERSOURCE.GAMETDB : COVERSOURCE.LIBRETRO;
        }
    }

    public string GetFixedArgs(string args)
    {
        return args.Replace("{file}", "\"" + filePath + "\"");
    }

    public bool ShouldRotateBox()
    {
        // for now it's user defined, but we should probably default to "no" in generic cases
        return rotateBox;
    }

    public void LinkChannel(Channel channel)
    {
        this.linkedChannel = channel;
    }
    
    public void SetBoxColor(Color boxColor)
    {
        this.boxColor = boxColor;
        linkedChannel?.Save();
    }

    public void SetGameId(string id)
    {
        this.gameId = id;
        linkedChannel?.Save();
    }
    
    public void SetCoverSource(COVERSOURCE source)
    {
        this.coverSource = source;
        InvalidateCovers();
        linkedChannel?.Save();
    }

    public async Task RefreshMetadata()
    {
        Debug.Log($"Refreshing metadata for {gameId}!");
        if (coverSource == COVERSOURCE.LIBRETRO)
        {
            // a better game ID defined by the user could ironically help with finding a proper title for this game
            // so we run it again
            gameId = MetadataController._instance.GetIdFromRetro(gameId, this);
        }

        if (coverSource == COVERSOURCE.GAMETDB)
        {
            // they might have updated their IDs database! so we check again
            FindTitle();
            FindBoxColor();
        }
        
        Debug.Log("Downloading covers!");
        await DownloadAllCovers();

        Debug.Log("Covers downloaded! Saving...");
        // and then we save the channel again
        linkedChannel?.Save();
    }

    public void FindTitle()
    {
        // we temporarily set it back to the default
        title = StaticUtils.GetGameFileName(filePath);
        
        // and now we load it
        title = MetadataController._instance.GetGameTitle(this);
    }

    public async void FindBoxColor()
    {
        boxColor = await MetadataController._instance.DownloadBoxColor(this);
    }

    public string GetTitle()
    {
        return /*console != null && !console.isTdbCover ? gameId :*/ title;
    }

    public bool IsConsoleTdbCover()
    {
        return console != null && console.isTdbCover;
    }
    
    public bool SupportsFullTextures()
    {
        return console != null && console.SupportsFullTextures();
    }

    public string GetCoverPath(string suffix)
    {
        if (coverSource == COVERSOURCE.CUSTOM)
        {
            return "custom_" + linkedChannel.id + "_" + suffix;
        }
        else
        {
            return consoleType.ToString().ToLowerInvariant() + "_" + StaticUtils.GetOnlyAlphanumeric(gameId) + "_" + suffix;
        }
    }

    public string[] GetUrlExtensions()
    {
        return console != null ? console.GetUrlExtensions() : new string[]{"jpg"};
    }

    private void InvalidateCovers()
    {
        frontCover = null;
        backCover = null;
        fullCover = null;
    }

    public async Task DownloadAllCovers()
    {
        // this method discards all the current textures and tries to download new ones, as long as it's not custom
        if (coverSource == COVERSOURCE.CUSTOM)
        {
            return;
        }
        
        InvalidateCovers();
        
        await MetadataController._instance.DownloadCover(this);
    }

    public void GetFrontCover(UnityAction<Texture2D> action)
    {
        if (frontCover is not null)
        {
            action.Invoke(frontCover);
        }
        else
        {
            MetadataController.LoadCoverFromCache(this, tex =>
            {
                this.frontCover = tex;
                action.Invoke(tex);
            }, "front");
        }
    }
    
    public void GetBackCover(UnityAction<Texture2D> action)
    {
        if (backCover is not null)
        {
            action.Invoke(backCover);
        }
        else
        {
            // If it's not a TDB console, the back cover is the same as the front one?
            if (coverSource == COVERSOURCE.LIBRETRO)
            {
                backCover = frontCover;
                action.Invoke(backCover);
                return;
            }
            
            // Otherwise we try to download it
            MetadataController.LoadCoverFromCache(this, tex =>
            {
                this.backCover = tex;
                action.Invoke(tex);
            }, "back");
        }
    }
    
    public void GetFullCover(UnityAction<Texture2D> action)
    {
        if (fullCover is not null)
        {
            action.Invoke(fullCover);
        }
        else
        {
            MetadataController.LoadCoverFromCache(this, tex =>
            {
                this.fullCover = tex;
                action.Invoke(tex);
            }, "full");
        }
    }

    public void GetCover(string suffix, UnityAction<Texture2D> action)
    {
        switch (suffix)
        {
            case "full": GetFullCover(action); break;
            case "front": GetFrontCover(action); break;
            case "back": GetBackCover(action); break;
        }
    }
    
    public void SetCover(string suffix, Texture2D tex)
    {
        switch (suffix)
        {
            case "full": fullCover = tex; break;
            case "front": frontCover = tex; break;
            case "back": backCover = tex; break;
        }
    }
}