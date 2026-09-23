using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Animations;
using Data;
using Data.ChannelTargets;
using DG.Tweening;
using Misc.SFO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleFileBrowser;
using TriInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

[DeclareFoldoutGroup("Box materials")]
[DeclareFoldoutGroup("Game case objects")]
public class MetadataController : MonoBehaviour
{
    public static MetadataController _instance;
    public Texture2D emptyTexture;
    public RenderTexture cameraTexture;
    public Texture2D unknownCover;
    public Texture2D unknownFront;
    public Texture2D steamIcon;

    [GroupNext("Box materials")]
    public Material artMaterial;
    public Material frontMaterial;
    public Material backMaterial;
    public Material boxMaterial;
    public Material boxNoTextureMaterial;

    [GroupNext("Game case objects")]
    public GameObject boxView;
    public GameObject boxContainer;
    public GameObject bigGameCase;
    public GameObject bigGameCaseFront;
    public GameObject bigGameCaseBack;
    public GameObject smallGameCase;
    public GameObject smallGameCaseFront;
    public GameObject smallGameCaseBack;
    public GameObject genericGameCase;
    public GameObject genericGameCaseFront;
    public GameObject genericGameCaseBack;
    [UnGroupNext]

    public List<string> debugFilePaths;
    private Tween rotationTween;

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        DownloadAllTitles(false);
        RestartBoxRotation();
        boxView.SetActive(false);
    }

    public async Task DownloadAllTitles(bool replaceExisting)
    {
        // we're gonna pre-download all the game titles so we have them ready for later
        List<Task> downloadTasks = new List<Task>();
        foreach (Console console in CONSOLES.GetAllConsoles())
        {
            string file = console.GetDbName();
            if (console.HasOnlineCover() && (replaceExisting || !File.Exists(SaveManager.SAVE_FOLDER_GAMEDB + file)))
            {
                downloadTasks.Add(DownloadTitles(console));
            }
        }

        if (replaceExisting)
        {
            await Task.WhenAll(downloadTasks);
        }
    }

    private async Task DownloadTitles(Console console)
    {
        Debug.Log($"Starting downloading names for console {(int)console.type}!");
        if (console.isTdbCover)
        {
            string url = $"https://www.gametdb.com/{console.urlName}tdb.txt";
            UnityWebRequest response = await WebRequestController.SendDownloadText(url);
            if (!response.isNetworkError && !response.isHttpError)
            {
                byte[] results = response.downloadHandler.data;
                string finalPath = SaveManager.SAVE_FOLDER_GAMEDB + console.GetDbName();
                if (File.Exists(finalPath))
                {
                    File.Delete(finalPath);
                }

                await File.WriteAllBytesAsync(finalPath, results);
            }
        }
        else
        {
            // Ok so we have to cheat a little, not as easy as GameTDB
            // Big thanks to everyone who contributed to LibRetro for creating such a neat public database!
            string url = $"https://thumbnails.libretro.com/{console.urlName}/Named_Boxarts/";
            UnityWebRequest response = await WebRequestController.SendDownloadText(url, true);
            if (!response.isNetworkError && !response.isHttpError)
            {
                string results = response.downloadHandler.text;
                List<string> file = new List<string>();

                // We have to read the results and save the link names in the actual db file
                // We'll do it in another task as to not block the main one
                await Task.Run(() =>
                {
                    StringBuilder builder = new StringBuilder();
                    bool isSaving = false;
                    foreach (char ch in results)
                    {
                        if (ch == '"')
                        {
                            if (builder.Length != 0)
                            {
                                // Found end of a link
                                if (builder.Length > 4)
                                {
                                    string key = HttpUtility.UrlDecode(builder.ToString()[0..(builder.Length - 4)]);
                                    //int pos = key.IndexOf('(');
                                    //string value = pos <= 0 ? key : key.Substring(0, pos-1);
                                    file.Add(key);
                                }

                                builder.Clear();
                                isSaving = false;
                            }
                            else
                            {
                                // we just skip it
                                continue;
                            }
                        }

                        if (isSaving)
                        {
                            // Gotta save this char, it's part of the link
                            builder.Append(ch);
                        }

                        if (ch == '=')
                        {
                            // Found the beginning of a link! We'll start saving it
                            isSaving = true;
                        }
                    }

                    // and we save it
                    string finalPath = SaveManager.SAVE_FOLDER_GAMEDB + console.GetDbName();
                    if (File.Exists(finalPath))
                    {
                        File.Delete(finalPath);
                    }

                    File.WriteAllLines(finalPath, file);
                });
            }
        }

        Debug.Log($"Finished downloading names from console {(int)console.type}!");
    }

    public async Task<Color> DownloadBoxColor(GameMetadata metadata)
    {
        if (metadata.consoleType == CONSOLETYPE.UNKNOWN || !metadata.console.isTdbCover)
        {
            Debug.Log($"Can't find box color for {metadata.gameId} since it's not from GameTDB!");
            return metadata.boxColor;
        }

        Debug.Log($"Finding box color for {metadata.gameId}!");
        // We're gonna cheat once again, since the only element with "bgcolor" in the page is the box color!
        string url = $"https://www.gametdb.com/{metadata.console.htmlName}/{metadata.gameId}";
        UnityWebRequest response = await WebRequestController.SendDownloadText(url, true);
        if (!response.isNetworkError && !response.isHttpError)
        {
            string results = response.downloadHandler.text;

            int index = results.IndexOf("bgcolor", StringComparison.Ordinal);
            if (index > 0)
            {
                Color newColor;
                string color = results.Substring(index + 9, 7);
                if (color.Contains("#\"") || color.Contains("#'"))
                {
                    // It's transparent, so they didn't specify a color
                    return new Color(1f, 1f, 0.97f, 0.1f);
                }

                bool success = ColorUtility.TryParseHtmlString(color, out newColor);
                if (success)
                {
                    Debug.Log($"Found cover color for {metadata.gameId}: {color}!");
                    return newColor;
                }
                else
                {
                    Debug.Log($"Failed to parse cover color for {metadata.gameId}: {color}!");
                    return metadata.boxColor;
                }
            }
            else
            {
                Debug.Log($"The game {metadata.gameId} doesn't have a cover color uploaded!");
                return metadata.boxColor;
            }
        }

        return metadata.boxColor;
    }

    public async Task UpdateTitles()
    {
        PopupController.ShowPopup("popup.updatingtitles", null);
        await DownloadAllTitles(true);

        // now we update all the metadatas we have
        foreach (Channel channel in ChannelController._instance.loadedChannels)
        {
            if (channel.GetTarget() is ChannelEmulatorTarget target && target.metadata != null)
            {
                string title = target.metadata.title;
                target.metadata.FindTitle();
                string newTitle = title;
                if (newTitle != null && !newTitle.Equals(title))
                {
                    channel.Save();
                }
            }
        }

        // and we let the user know we're done
        PopupController.ShowPopup("popup.updatedtitles");
    }

    public void RestartBoxRotation()
    {
        rotationTween?.Kill();
        boxContainer.transform.localEulerAngles = new Vector3(0, 0, 0);
    }

    public string GetGameTitle(GameMetadata metadata)
    {
        string file = metadata.console.GetDbName();
        return FindString(SaveManager.SAVE_FOLDER_GAMEDB + file, metadata.gameId);
    }

    private string FindString(string path, string key)
    {
        // hope this isn't too expensive. oh boy.
        string[] lines = File.ReadAllLines(path);
        foreach (string line in lines)
        {
            int index = line.IndexOf('=');
            string checkKey = line.Substring(0, index - 1);
            if (index >= 0 && checkKey.Equals(key))
            {
                return line.Substring(index + 2);
            }
        }

        return null;
    }

    private AppInfo TryGetAppInfo(string[] packages)
    {
        foreach (string check in packages)
        {
            AppInfo appInfo = AndroidLinker._instance.GetAppInfo(check);
            if (appInfo != null)
            {
                return appInfo;
            }
        }

        return null;
    }

    public AppInfo FindSuitableApp(GameMetadata metadata)
    {
        if (!Application.isMobilePlatform || metadata is null)
        {
            // they should select it themselves
            return null;
        }

        // we go through the supported ANDROID consoles, only those that allow a direct access to the game
        foreach (Emulator emulator in EMULATORS.GetAllEmulators())
        {
            if (emulator.consoles.Contains(metadata.consoleType))
            {
                AppInfo appInfo = AndroidLinker._instance.GetAppInfo(emulator.androidPackage);
                if (appInfo != null)
                {
                    return appInfo;
                }
            }
        }

        // not found, or unsupported platform. they should choose it on their own
        return null;
    }

    public string GetEmulatorAndroidActivity(string package)
    {
        // we return the in-app activity that launches games
        // this way we can try to launch the game directly into the emulator instead of through the main activity

        foreach (Emulator emulator in EMULATORS.GetAllEmulators())
        {
            if (emulator.androidPackage != null && emulator.androidPackage.Equals(package))
            {
                return emulator.androidActivity;
            }
        }

        // not one of ours. let's return the default one
        return null;
    }

    public string GetEmulatorAndroidExtra(string package)
    {
        // some apps want the URI send as an extra instead of content uri
        foreach (Emulator emulator in EMULATORS.GetAllEmulators())
        {
            if (emulator.androidPackage != null && emulator.androidPackage.Equals(package))
            {
                return emulator.androidExtra;
            }
        }

        // not one of ours. let's return the default one
        return null;
    }

    public bool GetEmulatorAndroidCast(string package)
    {
        // some apps want the URI send as an extra instead of content uri
        foreach (Emulator emulator in EMULATORS.GetAllEmulators())
        {
            if (emulator.androidPackage != null && emulator.androidPackage.Equals(package))
            {
                return emulator.sendAsPath;
            }
        }

        // not one of ours. let's return the default one
        return true;
    }

    public string GetEmulatorPCArgs(string emulatorPath, GameMetadata metadata)
    {
        // We'll try to figure it out from the console
        // if we ever need it, we'll also use the emulator path
        foreach (Emulator emulator in EMULATORS.GetAllEmulators())
        {
            if (emulator.consoles.Contains(metadata.consoleType))
            {
                return emulator.pcArgs;
            }
        }

        // if we don't recognize or support the emulator, we just provide the game file path to the emulator
        // and hope it runs it anyway lmao
        return "{file}";
    }

    public GameMetadata GenerateSteamMetadata(SteamGame game, Channel channel)
    {
        // much more simple than a regular emulator one
        GameMetadata metadata = new GameMetadata(game.gameId)
        {
            title = game.gameTitle,
            gameId = game.gameId
        };
        metadata.SetConsole(CONSOLES.STEAM);
        metadata.LinkChannel(channel);
        metadata.RefreshMetadata(); // this downloads the covers in async
        Debug.Log($"ID from Steam game is: {metadata.gameId}");
        return metadata;
    }

    public GameMetadata GenerateMetadata(string originalPath, Channel channel)
    {
        GameMetadata metadata = new GameMetadata(originalPath);
        // at least for now, we set this as the title
        metadata.title = StaticUtils.GetGameFileName(originalPath);

        metadata.LinkChannel(channel);
        string id = RetrieveId(metadata);
        metadata.language = GetLanguage(metadata);
        metadata.SetGameId(id ?? StaticUtils.GetGameFileName(metadata.filePath)); // we set the game's name as default if no ID was found, just so it can download titles
        metadata.RefreshMetadata(); // this downloads the covers in async
        Debug.Log($"ID from {originalPath} is: {metadata.gameId}");
        if (metadata.console == null)
        {
            Debug.Log("Couldn't determinate the console this file belongs to!");
        }

        return metadata;
    }

    private string GetLanguage(GameMetadata metadata)
    {
        string gameId = metadata.gameId;
        if (string.IsNullOrEmpty(gameId) || metadata.console == null || !metadata.console.canRetrieveLanguage)
        {
            return "EN";
        }

        string country;
        switch (gameId[3])
        {
            case 'J':
            {
                country = "JA";
                break;
            }
            case 'K':
            {
                country = "KO";
                break;
            }
            case 'E':
            {
                country = "US";
                break;
            }
            default:
            {
                country = "EN";
                break;
            }
        }

        return country;
    }

    private string RetrieveId(GameMetadata metadata)
    {
        try
        {
            // We try to obtain it from the binary files
            string gameId = FindFormatFromExtension(metadata);
            if (!string.IsNullOrEmpty(gameId))
            {
                return gameId;
            }

            // If not found, we access the file to try finding the format in the binaries
            gameId = FindFormatFromFile(metadata);
            if (!string.IsNullOrEmpty(gameId))
            {
                return gameId;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

        // Finally we check whether they include it manually in the file name
        return FindIdFromName(StaticUtils.GetGameFileName(metadata.filePath));
    }

    private string FindIdFromName(string fileName)
    {
        int initial = fileName.IndexOf("[");
        int final = fileName.LastIndexOf("]");
        if (initial != -1 && final != -1)
        {
            int length = final - initial;
            return fileName.Substring(initial + 1, length - 1);
        }

        return null;
    }

    private string FindFormatFromExtension(GameMetadata metadata)
    {
        return GetIdByFormat(metadata, Path.GetExtension(metadata.filePath));
    }

    private string FindFormatFromFile(GameMetadata metadata)
    {
        return GetIdByFormat(metadata, GetStringFromBinary(metadata.filePath, 0, 3));
    }

    private string GetIdByFormat(GameMetadata metadata, string format)
    {
        try
        {
            // First we assign the console based on the known console file types we know about
            string ext = format.ToLowerInvariant().Replace(".", "");
            foreach (Console console in CONSOLES.GetAllConsoles())
            {
                if (console.knownFormats.Contains(ext))
                {
                    metadata.SetConsole(console);
                    break;
                }
            }

            // If not found, we check for special cases
            // (For example, games with .iso are too generic to recognize just from the extension
            if (metadata.console == null && StaticUtils.GetGameFileNameWithExtension(metadata.filePath)
                    .ToLowerInvariant().Equals("eboot.bin"))
            {
                // It's a PS3 game!
                metadata.SetConsole(CONSOLES.PS3);

                // We can -also- obtain the game ID from the SFO file right above it!
                return GetIdFromPs3(metadata.filePath);
            }

            // If we get here we weren't able to locate the console
            if (metadata.console == null)
            {
                Debug.Log($"Couldn't find a console for {metadata.filePath}!");
                metadata.console = CONSOLES.UNKNOWN;
                return null;
            }

            // And now we try to find the ID from those file formats that allow it
            string path = metadata.filePath;
            switch (StaticUtils.GetExtension(path))
            {
                case "rvz":
                {
                    return GetIdFromGamecubeRvz(path);
                }
                case "wbfs":
                case "wbf":
                {
                    return GetIdFromWiiWbfs(path);
                }
                case "nds":
                {
                    return GetIdFromNds(path);
                }
                // No GBA anymore, we consider it a Retro console. Although we -can- get the ID if we wanted.
                /*case CONSOLETYPE.GBA:
                {
                    return GetIdFromGba(path);
                }*/
                case "rpx":
                {
                    return GetIdFromWiiuRpx(path);
                }
                case "3ds":
                {
                    return GetIdFrom3ds3ds(path);
                }
                case "cia":
                {
                    return GetIdFrom3dsCia(path);
                }
                case "nsp":
                case "xci":
                {
                    return GetIdFromSwitch(path);
                }
                default:
                {
                    return metadata.console.isTdbCover
                        ? FindIdFromDatabase(path, metadata.console) // we try to do a reverse search from the file name, just like in the switch
                        : GetIdFromRetro(StaticUtils.GetGameFileName(path), metadata);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

        return null;

        // Some unsupported auto-ID-retrieving formats:
        //  - Switch NSP
        //  - Switch XCI
        //  - GB
        //  - GBC
    }

    private string GetIdFromWiiuRpx(string path)
    {
        // ID -usually- available in the parent directory
        return FindIdFromName(Directory.GetParent(path).Parent.Name);
    }

    private string GetIdFromGamecubeRvz(string path)
    {
        // ID available in the .rvz file
        return GetStringFromBinary(path, 88, 6);
    }

    private string GetIdFromWiiWbfs(string path)
    {
        // ID available in the .wbfs file
        return GetStringFromBinary(path, 512, 6);
    }

    private string GetIdFromNds(string path)
    {
        // ID available in the .nds file
        return GetStringFromBinary(path, 12, 4);
    }

    private string GetIdFromGba(string path)
    {
        // ID available in the .gba file
        return GetStringFromBinary(path, 172, 4);
    }

    private string GetIdFrom3ds3ds(string path)
    {
        // ID available in the .3ds file
        return GetStringFromBinary(path, 4438, 4);
    }

    private string GetIdFrom3dsCia(string path)
    {
        // ID available in the .cia file
        return GetStringFromBinary(path, 14998, 4);
    }

    private string GetStringFromBinary(string path, int offset, int count)
    {
        if (Application.isMobilePlatform)
        {
            // We read from Android paths
            string id = AndroidLinker.GetPlugin().CallStatic<string>("readStringFromBytes",
                AndroidLinker.currentActivity, path, offset, count);
            Debug.Log("Read bytes from file: " + id);
            return id;
        }
        else
        {
            // We read from the regular path
            byte[] test = new byte[count];
            FileStream stream = new FileStream(path, FileMode.Open);
            using (BinaryReader reader = new BinaryReader(stream))
            {
                reader.BaseStream.Seek(offset, SeekOrigin.Begin);
                reader.Read(test, 0, count);
            }

            return System.Text.Encoding.UTF8.GetString(test);
        }
    }

    private string GetIdFromPs3(string path)
    {
        byte[] bytes;
        if (Application.isMobilePlatform)
        {
            // No way to read from super path. Oh well.
            return null;

            bytes = AndroidLinker.GetPlugin().CallStatic<byte[]>("readBytes", AndroidLinker.currentActivity, path);
            Debug.Log("Read bytes from file, length = " + bytes.Length);
        }
        else
        {
            // We read from the regular path
            bytes = File.ReadAllBytes(Directory.GetParent(path).Parent.ToString() + Path.DirectorySeparatorChar +
                                      "PARAM.SFO");
        }

        return (string)Sfo.ReadSfo(bytes)["TITLE_ID"];
    }

    private string GetIdFromSwitch(string path)
    {
        // tried to find ways to retrieve game ID easily from .nsp and .xci, no luck
        // so i'll try with the closest match of the game's filename. reverse check time
        return FindIdFromDatabase(path, CONSOLES.SWITCH);
    }

    public string GetIdFromRetro(string path, GameMetadata metadata)
    {
        if (metadata.consoleType == CONSOLETYPE.UNKNOWN)
        {
            return null;
        }

        string id = FindIdFromDatabase(path, metadata.console, true);
        if (!string.IsNullOrEmpty(id))
        {
            // we also update the title and fix the ID!
            if (metadata.console.isTdbCover)
            {
                // we update title + ID
                int pos = id.IndexOf('=');
                metadata.title = pos <= 1 ? id : id.Substring(pos+2);
            }
            else
            {
                // we clean the name
                int pos = id.IndexOf('(');
                metadata.title = pos <= 1 ? id : id.Substring(0, pos - 1);   
            }
        }

        return id ?? StaticUtils.GetGameFileName(metadata.filePath);
    }

    private string FindIdFromDatabase(string path, Console console, bool compareThroughKey = false)
    {
        if (console == null)
        {
            return null;
        }

        // we're gonna do a reverse search from a database to get the closest matching game from the file title
        string id = "";
        int distance = 0;

        // we need to fix the string since it's a content in Android, and it looks pretty bad
        string str = StaticUtils.GetOnlyAlphanumeric(path).Trim();
        Debug.Log($"Trying to find game closest to: {str}");

        // let's check if the titles were downloaded
        string text = SaveManager.SAVE_FOLDER_GAMEDB + console.GetDbName();
        if (!File.Exists(text))
        {
            return null;
        }

        // time to iterate the file names
        string[] lines = File.ReadAllLines(text);
        foreach (string line in lines)
        {
            string checkValue = "";
            int index = 1;
            if (compareThroughKey)
            {
                checkValue = line;
            }
            else
            {
                index = line.IndexOf('=');
                checkValue = line.Substring(index + 2);
            }

            /*int index = line.IndexOf('=');
            string checkValue = compareThroughKey ? line.Substring(0, index - 1) : line.Substring(index + 2);*/

            //int checkDistance = StaticUtils.GetLevenshteinDistance(checkValue.ToLowerInvariant().Trim(), str);

            string check = StaticUtils.GetOnlyAlphanumeric(checkValue.Trim());
            int checkDistance = StaticUtils.GetLongestCommonSubstring(check, str);
            if (checkDistance > distance || (checkDistance == distance && !string.IsNullOrEmpty(id) && check.Length < id.Length))
            {
                // we choose either one with more length, or one with same length but shorter
                distance = checkDistance;
                id = compareThroughKey ? checkValue : line.Substring(0, index - 1);
            }
        }

        Debug.Log($"Found title {id} with a distance of {distance}");
        return id;
    }

    private void ResetCovers()
    {
        boxMaterial.color = Color.white;
        boxNoTextureMaterial.color = Color.white;
        boxView.SetActive(false);
        smallGameCaseFront.gameObject.SetActive(false);
        smallGameCaseBack.gameObject.SetActive(false);
        bigGameCaseFront.gameObject.SetActive(false);
        bigGameCaseBack.gameObject.SetActive(false);
        genericGameCaseFront.gameObject.SetActive(false);
        genericGameCaseBack.gameObject.SetActive(false);

        artMaterial.mainTexture = unknownCover;
        frontMaterial.mainTexture = emptyTexture;
        backMaterial.mainTexture = emptyTexture;
    }

    public static void LoadCoverFromCache(GameMetadata metadata, UnityAction<Texture2D> action, string suffix)
    {
        string path = SaveManager.SAVE_FOLDER_GAMECOVERS + metadata.GetCoverPath(suffix);
        if (!File.Exists(path))
        {
            // Then it's nowhere to be found - return null
            action.Invoke(null);
            return;
        }

        // If we found it, we return it
        AppController._instance.StartCoroutine(LoadImage(path, action));
    }

    private static IEnumerator LoadImage(string path, UnityAction<Texture2D> action)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture("file://" + path, true))
        {
            yield return uwr.SendWebRequest();
            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log(uwr.error);
                action.Invoke(null);
            }
            else
            {
                action.Invoke(((DownloadHandlerTexture)uwr.downloadHandler).texture);
            }
        }
    }

    private GameObject GetBoxObject(bool isFront, GameMetadata metadata)
    {
        switch (metadata.console.boxSize)
        {
            case BOXTYPE.DVD: return isFront ? bigGameCaseFront : bigGameCaseBack;
            case BOXTYPE.CARTRIDGE: return isFront ? smallGameCaseFront : smallGameCaseBack;
            default: return isFront ? genericGameCaseFront : genericGameCaseBack;
        }
    }

    public void LoadFullCover(GameMetadata metadata)
    {
        // First we reset the state to the default one
        ResetCovers();
        boxView.SetActive(true);

        // If the metadata or its console is null, then we setup a placeholder DVD cover
        bool isMissingConsole = metadata is null || metadata.console is null;
        BOXTYPE type = isMissingConsole ? BOXTYPE.DVD : metadata.console.boxSize;
        Vector3 size = isMissingConsole ? Vector3.one : metadata.console.modelSize;

        // We enable the main objects
        smallGameCase.gameObject.SetActive(type == BOXTYPE.CARTRIDGE);
        smallGameCase.gameObject.transform.localScale = size;
        bigGameCase.gameObject.SetActive(type == BOXTYPE.DVD);
        bigGameCase.gameObject.transform.localScale = size;
        genericGameCase.gameObject.SetActive(type == BOXTYPE.GENERIC);
        genericGameCase.gameObject.transform.localScale = size;

        if (isMissingConsole)
        {
            artMaterial.mainTexture = unknownCover;
            return;
        }

        // First we try to setup a full cover
        metadata.GetFullCover(tex =>
        {
            bool notFound = tex == null;
            artMaterial.mainTexture = notFound ? unknownCover : tex;
            if (notFound)
            {
                // Back luck. Gotta stitch together front and back
                // We also enable the front-back pieces in the model
                GameObject front = GetBoxObject(true, metadata);
                GameObject back = GetBoxObject(false, metadata);

                metadata.GetFrontCover(tex2 =>
                {
                    if (tex2 != null)
                    {
                        frontMaterial.mainTexture = tex2;
                        front.SetActive(true);
                        if (type == BOXTYPE.GENERIC)
                        {
                            ResizeBoxFromCover(genericGameCase, tex2);
                        }
                    }
                });
                metadata.GetBackCover(tex2 =>
                {
                    if (tex2 != null)
                    {
                        backMaterial.mainTexture = tex2;
                        back.SetActive(true);
                    }
                });
            }
        });


        // Finally we set the box's color
        Material mat = type == BOXTYPE.GENERIC ? boxNoTextureMaterial : boxMaterial;
        mat.color = metadata.boxColor;
    }

    private void ResizeBoxFromCover(GameObject box, Texture2D cover)
    {
        // We consider that the current size is valid for 1x1 boxes. Let's try with others
        float ratio = (float)cover.width / cover.height; // if this is = 2, it means size 2 on X per size 1 on Z

        float multiplier = 1f;
        Vector3 scale = new Vector3(ratio, 1, 1);

        float maxWidth = 1.2f;
        float minWidth = 0.8f;
        if (ratio > maxWidth)
        {
            // this works, however we don't want the box to get a width too big, so we expand it in that case
            multiplier = maxWidth / ratio;
            scale = new Vector3(ratio * multiplier, multiplier, multiplier);
        }
        else if (ratio < minWidth)
        {
            // we want to expand the case a little on Y instead, while keeping the Y the same
            float otherRatio = (float)cover.height / cover.width;
            multiplier = 1;
            scale = new Vector3(1, otherRatio * multiplier, 1);
        }

        // and we finally assign the size
        box.transform.localScale = scale;
    }

    public async Task DownloadCover(GameMetadata metadata)
    {
        if (metadata.consoleType == CONSOLETYPE.UNKNOWN)
        {
            Debug.Log($"Can't download covers if we don't know the console type for {metadata.gameId}!");
            return;
        }

        // The order of covers is this:
        // First we need to download a front cover in our language, if not possible then try others
        // After that, we try finding a full cover in our language
        // Then we try finding a back cover in our language.
        // If either the front or back isn't in our languages, we try to find a full cover in any language.
        // Finally, last resort is downloading a back texture in any language

        // We're gonna do a bunch of tries to download a valid cover (the names tend to vary a lot unfortunately).
        // First we try to download the front cover. A try in our language, or in all other languages if not possible
        bool foundFrontLanguage = await TryDownloadPartCover(metadata, true, metadata.language);
        if (foundFrontLanguage)
        {
            Debug.Log($"Successfully downloaded front cover in our language for {metadata.gameId}!");
        }
        else
        {
            // No luck. We try to find it in another language
            bool foundFront = await TryDownloadPartCover(metadata, true, null);
            if (foundFront)
            {
                Debug.Log($"Successfully downloaded front cover in ANOTHER language for {metadata.gameId}!");
            }
        }

        // If this is NOT a TDB cover, we return here! We can only retrieve front covers from Retro
        if (metadata.coverSource != COVERSOURCE.GAMETDB)
        {
            Debug.Log($"This is a retro console, can't download more covers for {metadata.gameId}!");
            return;
        }

        // Now we try to find a fullcover for our language, but if it's not possible, then we'll try to stitch together the front+backs
        bool foundFullLanguage = await TryDownloadFullCover(metadata, metadata.language);
        if (foundFullLanguage)
        {
            Debug.Log($"Successfully downloaded full cover in our language for {metadata.gameId}!");
        }
        else
        {
            // If we reach this part, then we couldn't find full art in our language. Let's try to find a back piece in our language together.
            Debug.Log($"Couldn't download full cover for {metadata.gameId}, trying front+back from all langs");
            bool foundBackLanguage = await TryDownloadPartCover(metadata, false, metadata.language);
            if (foundBackLanguage)
            {
                Debug.Log($"Successfully downloaded back texture in our language for {metadata.gameId}!");
            }

            // Finally we check if we have a front+back in our language. If not, we're gonna prefer a full cover in any language.
            if (!foundFrontLanguage || !foundBackLanguage)
            {
                Debug.Log(
                    $"Couldn't find matching language covers for {metadata.gameId}, trying full cover in any lang");
                bool foundFull = await TryDownloadFullCover(metadata, null);
                if (foundFull)
                {
                    Debug.Log($"Successfully downloaded full cover in ANOTHER language for {metadata.gameId}!");
                }
                else
                {
                    // If we couldn't find any texture, we download whatever back texture we can find online. This is the last resort.
                    bool foundBack = await TryDownloadPartCover(metadata, false, null);
                    if (foundBack)
                    {
                        Debug.Log($"Successfully downloaded back texture in ANOTHER language for {metadata.gameId}!");
                    }
                    else
                    {
                        Debug.Log($"No full nor back textures were found for {metadata.gameId}!");
                    }
                }
            }
        }
    }

    private List<string> GetLanguagesList(GameMetadata metadata, string preferredLanguage, string overrideLanguage)
    {
        // if it's not a GameTDB console, no point in allowing languages
        if (metadata.coverSource != COVERSOURCE.GAMETDB)
        {
            return new List<string>(new[] { "" });
        }

        List<string> allLanguages = new List<string>();
        if (!string.IsNullOrEmpty(overrideLanguage))
        {
            // We're only gonna attempt to find in one language
            allLanguages.Add(overrideLanguage);
        }
        else
        {
            // We create a list of all languages, priorizing ours
            allLanguages = new List<string>() { "EN", "US", "ES", "IT", "DE", "FR", "NL", "JA", "KO", "other" };
            if (!string.IsNullOrEmpty(preferredLanguage))
            {
                allLanguages.Remove(preferredLanguage);
                allLanguages.Insert(0, preferredLanguage); // We put max priority on our language
            }
        }

        return allLanguages;
    }

    private async Task<bool> TryDownloadFullCover(GameMetadata metadata, string onlyLanguage)
    {
        List<string> lang = GetLanguagesList(metadata, metadata.language, onlyLanguage);
        string[] types = metadata.coverSource == COVERSOURCE.GAMETDB
            ? new[] { "coverfullM", "coverfullHQ", "coverfull" }
            : new[] { "" };
        Texture2D tex = await DownloadCoverImage(metadata, lang.ToArray(), types);
        bool found = tex is not null;
        if (found)
        {
            metadata.fullCover = tex;
            artMaterial.mainTexture = tex;
        }

        SaveTextureToCache(metadata, tex, "full");
        return found;
    }

    private async Task<bool> TryDownloadPartCover(GameMetadata metadata, bool isFront, string onlyLanguage)
    {
        List<string> allLanguages = GetLanguagesList(metadata, metadata.language, onlyLanguage);

        // We search for front or back, but only add types if it's from TDB
        string[] types = metadata.coverSource == COVERSOURCE.GAMETDB
            ? isFront ? new[] { "coverM", "coverHQ", "cover" } : new[] { "backM", "backHQ", "back" }
            : new[] { "" };

        Texture2D texture = await DownloadCoverImage(metadata, allLanguages.ToArray(), types);
        if (isFront)
        {
            metadata.frontCover = texture;
        }
        else
        {
            metadata.backCover = texture;
        }

        SaveTextureToCache(metadata, texture, isFront ? "front" : "back");
        
        return texture is not null;
    }

    private void SaveTextureToCache(GameMetadata metadata, Texture2D texture, string suffix)
    {
        if (texture != null)
        {
            string path = SaveManager.SAVE_FOLDER_GAMECOVERS + metadata.GetCoverPath(suffix);
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            Texture2D copy = texture.DuplicateTexture();
            File.WriteAllBytes(path, copy.EncodeToPNG());
            Destroy(copy);
        }
    }

    private async Task<Texture2D> DownloadCoverImage(GameMetadata metadata, string[] langs, string[] types)
    {
        foreach (string lang in langs)
        {
            foreach (string type in types)
            {
                foreach (string ext in metadata.GetUrlExtensions())
                {
                    Texture2D tex;
                    if (metadata.consoleType == CONSOLETYPE.J2ME)
                    {
                        // we can recover it from the .jar file!
                        tex = RetrieveTextureFromJ2ME(metadata);
                    }
                    else
                    {
                        // let's download it online
                        string url = metadata.coverSource == COVERSOURCE.GAMETDB
                            ? $"https://art.gametdb.com/{metadata.console.urlName}/{type}/{lang}/{metadata.gameId}.{ext}"
                            : metadata.consoleType == CONSOLETYPE.STEAM
                                ? $"https://cdn.cloudflare.steamstatic.com/steam/apps/{metadata.gameId}/header.jpg"
                                : $"https://thumbnails.libretro.com/{metadata.console.urlName}/Named_Boxarts/{metadata.gameId}.png";
                        bool askThroughHttp = metadata.consoleType != CONSOLETYPE.STEAM && !metadata.IsConsoleTdbCover();
                        tex = await WebRequestController.SendDownloadCover(url, askThroughHttp);
                    }
                    if (tex is not null)
                    {
                        Debug.Log($"Successfully downloaded image for {metadata.gameId}!\nLang={lang}, Type={type}");
                        return tex;
                    }
                }
            }
        }

        return null;
    }

    private Texture2D RetrieveTextureFromJ2ME(GameMetadata metadata)
    {
        if (FileBrowserHelpers.FileExists(metadata.filePath))
        {
            // we copy it to our temp file
            string extractDirectory = SaveManager.TEMP_GAMES + Path.GetFileNameWithoutExtension(metadata.filePath);
            string zipPath = SaveManager.TEMP_GAMES + Path.DirectorySeparatorChar + "game.zip";
            SaveManager.RecreateFolder(extractDirectory);
            FileBrowserHelpers.CopyFile(metadata.filePath, zipPath);
            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries.Where(e => e.Name.Equals("icon.png")))
                {
                    SaveManager.UnzipEntry(entry, extractDirectory);
                    string filePath = Path.Combine(extractDirectory, entry.FullName);
                    Texture2D tex = new Texture2D(2, 2); // Empty texture
                    byte[] rawData = File.ReadAllBytes(filePath);
                    tex.LoadImage(rawData, false);
                    return tex;
                }
            }
        }

        return null;
    }

    public List<SteamGame> GetInstalledSteamGames()
    {
        List<SteamGame> list = new List<SteamGame>();
        string steamPath = PREFS.SteamLocation.GetString();
        if (string.IsNullOrEmpty(steamPath))
        {
            return list;
        }

        string games = Path.GetDirectoryName(steamPath) + Path.DirectorySeparatorChar + "steamapps";

        foreach (string file in Directory.GetFiles(games))
        {
            if (FileManager.GetFileName(file).StartsWith("appmanifest_"))
            {
                // it's a kind of stupid format, so we'll parse manually
                string text = File.ReadAllText(file);
                list.Add(new SteamGame()
                {
                    gameId = FindStringAcf(text, "\"appid\""),
                    gameTitle = FindStringAcf(text, "\"name\""),
                });
            }
        }

        return list;
    }

    private string FindStringAcf(string text, string key)
    {
        StringBuilder build = new StringBuilder();
        int idPos = text.IndexOf(key, StringComparison.Ordinal);
        if (idPos > 0)
        {
            bool foundComillas = false;
            for (int i = idPos + key.Length; i < text.Length; i++)
            {
                if (text[i] == '\"')
                {
                    if (!foundComillas)
                    {
                        foundComillas = true;
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }

                if (foundComillas)
                {
                    build.Append(text[i]);
                }
            }
        }
        return build.ToString();
    }


    [Button("Refresh ALL game covers")]
    public void BUTTON_FreshAllMetadata()
    {
        SaveManager.RecreateFolder(SaveManager.SAVE_FOLDER_GAMECOVERS);
        foreach (Channel channel in ChannelController._instance.loadedChannels)
        {
            if (channel.GetTarget() is ChannelEmulatorTarget target)
            {
                channel.iconAnimation = ChannelController._instance.GetOrLoadChannelAnimation(SaveManager.boxArtIcon);
                channel.bannerAnimation = ChannelController._instance.GetOrLoadChannelAnimation(SaveManager.boxArtBanner);
                
                target.metadata.RefreshMetadata();
            }
        }
    }

    [Button("Show emugame picker")]
    public void BUTTON_ShowEmugamePicker()
    {
        SettingsController._instance.setupEmulatorSettingView.ShowLoadDialog();
    }
}
