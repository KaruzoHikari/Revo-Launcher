using System;
using System.Collections.Generic;
using UnityEngine;

public class Console
{
    public CONSOLETYPE type;
    public string name;
    public string urlName;
    public string preferredUrlExtension;
    public BOXTYPE boxSize;
    public bool isTdbCover;
    public bool canRetrieveLanguage;
    public Vector3 modelSize;
    public Color boxColor;
    public List<string> knownFormats;
    public string htmlName;
    public bool hasOnlineCover;

    public Console(CONSOLETYPE type, string name, string urlName, string preferredUrlExtension,
        BOXTYPE size, Vector3 modelSize, Color boxColor, bool canRetrieveLanguage, bool isTdbCover, string[] knownFormats,
        string htmlName, bool hasOnlineCover = true)
    {
        this.type = type;
        this.name = name;
        this.urlName = urlName;
        this.preferredUrlExtension = preferredUrlExtension;
        this.boxSize = size;
        this.canRetrieveLanguage = canRetrieveLanguage;
        this.isTdbCover = isTdbCover;
        this.modelSize = modelSize;
        this.boxColor = boxColor;
        this.knownFormats = new List<string>(knownFormats);
        this.htmlName = htmlName;
        this.hasOnlineCover = hasOnlineCover;
    }

    public string[] GetUrlExtensions()
    {
        // we return a list where ours is first, just so we don't waste time

        return preferredUrlExtension.Equals("jpg") ? new[] { "jpg", "png" } : new[] { "png", "jpg" };
    }

    public string GetDbName()
    {
        return "console_" + (int)type + "_db";
    }

    public bool HasOnlineCover()
    {
        return hasOnlineCover;
    }

    public bool SupportsFullTextures()
    {
        return boxSize == BOXTYPE.DVD || boxSize == BOXTYPE.CARTRIDGE;
    }
}

public class RetroConsole : Console
{
    public RetroConsole(CONSOLETYPE type, string urlName, string[] knownFormats, bool hasOnlineCover = true) : this(type, urlName, knownFormats, BOXTYPE.GENERIC, new Vector3(1f,1f,1f), Color.white, hasOnlineCover)
    {
    }

    public RetroConsole(CONSOLETYPE type, string urlName, string[] knownFormats, BOXTYPE boxType, Vector3 size, Color color, bool hasOnlineCover = true) : base(type, urlName, urlName,
        "png", boxType, size, color, false, false, knownFormats, null, hasOnlineCover)
    {
        // now we fix the name
        int index = urlName.IndexOf(" - ", StringComparison.Ordinal);
        if (index >= 0)
        {
            name = urlName.Substring(index + 3);
        } 
    }
}