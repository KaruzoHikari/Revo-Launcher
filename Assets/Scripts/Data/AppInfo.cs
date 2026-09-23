using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class AppInfo
{
    public string name;
    public string packageName;
    [JsonIgnore] public Texture2D icon;
    public bool isWindows = false;
    public byte[] iconAsBytes
    {
        get => icon.DuplicateTexture().EncodeToPNG();
        set
        {
            SyncContext.RunOnUnityThread(() =>
            {
                icon = Application.isMobilePlatform
                    ? new Texture2D(4, 4, TextureFormat.ETC2_RGBA8Crunched, false) // Empty texture
                    : new Texture2D(4, 4); // Empty texture
                icon.LoadImage(value,true);
            });
        }
    }

    public AppInfo(string name, string packageName, Texture2D icon)
    {
        this.name = name;
        this.packageName = packageName;
        this.icon = icon;
    }

    public void Unload()
    {
        GameObject.Destroy(icon);
    }
}
