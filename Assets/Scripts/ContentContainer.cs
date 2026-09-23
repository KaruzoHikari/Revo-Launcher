using System;
using System.IO;
using Animations;
using Newtonsoft.Json;
using UnityEngine;

public abstract class ContentContainer
{
    [JsonIgnore] public string deserializedPath;
    [JsonIgnore] public string importedFileName;
    [JsonIgnore] public OnlineInfo onlineInfo;
    
    public string name;
    public long id = -1;
    public DateTime lastEdited;
    public string forkId;

    public string lastVersion;
    public int lastBundle;
    
    [JsonIgnore] public string requestedNewName;
    [JsonIgnore] public string LatestName
    {
        get {
            if (!string.IsNullOrEmpty(requestedNewName))
            {
                return requestedNewName;
            }
            return name;
        }
    }
    
    public void CreateID()
    {
        id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public virtual void RunFirstSetup()
    {
        // load first values
        CreateID();
        lastEdited = DateTime.Now;
        
        // we create the deserialized folder
        CreateDeserializedFolder();
    }

    protected void AssignSaveValues()
    {
        // names and dates
        lastEdited = DateTime.Now;
        name = LatestName;
        requestedNewName = null;
        
        // versions
        lastVersion = Application.version;
        lastBundle = AppController._instance.GetCurrentBundle();
    }

    protected void CreateDeserializedFolder()
    {
        if (string.IsNullOrEmpty(deserializedPath))
        {
            // we don't have a deserialized path! we need to create it
            deserializedPath = SaveManager.TEMP_DESERIALIZE + Path.GetFileNameWithoutExtension(GetImportedFileName());
            SaveManager.RecreateFolder(deserializedPath);
        }
    }

    public string GetImportedFileName()
    {
        return string.IsNullOrEmpty(importedFileName) ? GetCurrentFileName() : importedFileName;
    }
    
    public abstract string GetCurrentFileName();

    // kaz 2024-04-01: so many of these that are pretty much the same between themes and animations.. will need a refactor at some point
    protected string GetLoadingFolder()
    {
        string extractedPath = SaveManager.TEMP_ASSETSLOAD + GetValidFileName();
        if (!Directory.Exists(extractedPath))
        {
            Directory.CreateDirectory(extractedPath);
        }

        return extractedPath;
    }
    
    public string GetValidFileName()
    {
        return StaticUtils.SanitizeString(name).ToLowerInvariant().Replace(" ", "");
    }

    public void DeleteLoadingFolder()
    {
        string extractedPath = SaveManager.TEMP_ASSETSLOAD + GetValidFileName();
        SaveManager.DeleteFolder(extractedPath);
    }
    
    public string MoveToLoadingFolder(string asset)
    {
        if (string.IsNullOrEmpty(asset))
        {
            return null;
        }

        string extractedPath = GetLoadingFolder();
        string finalPath = extractedPath + "/" + FileManager.GetFileName(asset);
        FileManager.CopyFile(asset, finalPath);
        return finalPath;
    }

    public string MoveToDeserializedFolder(string asset)
    {
        if (string.IsNullOrEmpty(asset))
        {
            return null;
        }
        
        string finalPath = deserializedPath + "/" + FileManager.GetFileName(asset);
        if (FileManager.IsSamePath(asset, finalPath))
        {
            // already in deserialized path
            return null;
        }
        
        FileManager.CopyFile(asset, finalPath);
        return finalPath;
    }
}