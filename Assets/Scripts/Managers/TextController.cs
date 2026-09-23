using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TriInspector;
using UnityEngine;
using UnityEngine.Networking;

public class TextController : MonoBehaviour
{
    public static TextController _instance;
    public bool forceNewLanguages;
    public int currentVersion;
    public List<string> embeddedFiles = new List<string>();
    public Dictionary<string, string> availableLanguages = new Dictionary<string, string>();
    private string langInfoPath;
    private string fontSize = "<size=100%>";
    [HideInInspector] public Dictionary<string, string> themeTranslations = new Dictionary<string, string>();
    [HideInInspector] public Dictionary<SystemLanguage, Dictionary<string, string>> allTranslations = new();
    private Dictionary<string, string> englishTranslations = new Dictionary<string, string>();
    private Dictionary<string, string> localeTranslations = new Dictionary<string, string>();
    [HideInInspector] public SystemLanguage chosenLanguage;
    public bool enableDebug;
    public SystemLanguage debugLanguage;

    [Title("Special lang-refresh objects")]
    public Date date;

    private void Awake()
    {
        _instance = this;
        langInfoPath = SaveManager.SAVE_FOLDER_LANG + "_info.json";
    }

    private void Start()
    {
        InitializeLang();
    }

    private void SaveEmbeddedTranslations()
    {
        Debug.Log("Saving embedded translations!");
        try
        {
            foreach (string fileName in embeddedFiles)
            {
                string originalPath = Application.streamingAssetsPath + "/DefaultTranslations/" + fileName;
                string destinationPath = SaveManager.SAVE_FOLDER_LANG + fileName;
                SaveManager.SendStreamingCopyRequest(originalPath, destinationPath, true);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public async void InitializeLang()
    {
        if (AppController._instance.IsFirstLoadCurrentBundle() || !Directory.EnumerateFiles(SaveManager.SAVE_FOLDER_LANG).Any() || Application.isEditor)
        {
            // We export the files from the apk
            SaveEmbeddedTranslations();
        }

        try
        {
            // We used to await this call, but it'd sometimes cause issues when opening the app (it'd download the langs terribly slowly, so it'd take forever to startup the app)
            // Now it'll download them while the app is running, and apply on next restart
            UpdateServerTranslations();
        }
        catch (Exception ignored)
        {
            // Ignored (we want to make sure that, whatever happens here, the app loads)
            Debug.LogError(ignored);
        }

        FindChosenLanguage();   
    }

    private void FindChosenLanguage()
    {
        string chosen = PREFS.Language.GetString();
        Debug.Log($"Found language is {chosen}!");
        if (chosen.Equals("Default"))
        {
            chosen = enableDebug ? debugLanguage.ToString() : Application.systemLanguage.ToString();
        }
        
        SetNewLanguage(chosen);
    }

    public void SetNewLanguage(string language)
    {
        try
        {
            chosenLanguage = (SystemLanguage) Enum.Parse(typeof(SystemLanguage), language);
        }
        catch (Exception e)
        {
            // we set it back to english
            Debug.Log($"Failed to set lang to {language}!");
            chosenLanguage = SystemLanguage.English;
        }
        
        PREFS.Language.SetString(chosenLanguage.ToString());
        Debug.Log($"Setting language to {chosenLanguage.ToString()}!");
        
        LoadTranslations();
        
        if (localeTranslations.Count == 0)
        {
            // This means that your language has no translation in Revo, so we default to English
            localeTranslations = englishTranslations;
        }
        
        // and finally we trigger a theme reload just in case they changed it
        StartCoroutine(CallThemeReload());
    }

    private IEnumerator CallThemeReload()
    {
        yield return null; // we wait 1 frames
        ThemeController._instance.TriggerThemeReload();
    }

    private void LoadTranslations(bool refreshAll = true)
    {
        englishTranslations.Clear();
        localeTranslations.Clear();
        availableLanguages.Clear();
        
        Debug.Log("Loading translation files!");
        if (File.Exists(langInfoPath))
        {
            JObject info = JObject.Parse(File.ReadAllText(langInfoPath));
            JObject languages = (JObject) info["languages"];
            foreach (var token in languages)
            {
                SystemLanguage language = (SystemLanguage) Enum.Parse(typeof(SystemLanguage), token.Key);
                string path = token.Value.ToString();
                availableLanguages[language.ToString()] = path;
                
                if (language == SystemLanguage.English || language == chosenLanguage)
                {
                    Dictionary<string, string> dictionary = language == SystemLanguage.English ? englishTranslations : localeTranslations;
                    LoadLanguage(language, dictionary);
                }
            }
        }
        
        fontSize = GetTranslation("general.fontsize") ?? "<size=100%>";
        if (refreshAll)
        {
            RefreshThemeDictionary();
        }
    }
    
    private void LoadLanguage(SystemLanguage language, Dictionary<string, string> dictionary)
    {
        string filePath = SaveManager.SAVE_FOLDER_LANG + availableLanguages[language.ToString()] + ".json";
        ProcessLanguage(filePath, dictionary);
        if (!allTranslations.ContainsKey(language))
        {
            allTranslations[language] = dictionary;
        }
    }

    private void RefreshTextObjects()
    {
        foreach (var translatedText in AppController._instance.gameCanvas.gameObject.GetComponentsInChildren<TranslatedText>(true))
        {
            translatedText.Refresh();
        }
        foreach (var translatedText in AppController._instance.overlayCanvas.gameObject.GetComponentsInChildren<TranslatedText>(true))
        {
            translatedText.Refresh();
        }
        date.Refresh();
    }

    public void RefreshThemeDictionary()
    {
        // we call this every time we want to bring the translations from the themes to the app!
        // like when closing the editor or saving a theme
        
        // first we clear the dictionary
        themeTranslations.Clear();
        
        // now we load them if found, only for our language (and English for the not found)
        Theme currentTheme = ThemeController._instance.GetCurrentTheme();
        if (currentTheme is null)
        {
            return;
        }
        
        LoadThemeDictionary(currentTheme.GetThemeText(SystemLanguage.English));
        LoadThemeDictionary(currentTheme.GetThemeText(chosenLanguage));
        
        RefreshTextObjects();
    }

    private void LoadThemeDictionary(ThemeText themeText)
    {
        if (themeText != null)
        {
            Dictionary<string, string> currentDictionary = themeText.GetDictionary();
            foreach (string text in currentDictionary.Keys)
            {
                themeTranslations[text] = currentDictionary[text];
            }
        }
    }

    private async Task UpdateServerTranslations()
    {
        // Here we're gonna retrieve the app translations from the server
        // (Although we're still embedding some translations in the app, in case the user has no connection)
        if (File.Exists(langInfoPath))
        {
            currentVersion = (int) JObject.Parse(File.ReadAllText(langInfoPath))["version"];
        }
        
        // We download the lang info JSON, or we use the cache one if no internet connection
        Debug.Log("Sending translation check.");
        UnityWebRequest response = await WebRequestController.SendGetTranslationsInfo();
        JObject json = response.GetJsonResponse();
        if (!response.isNetworkError && !response.isHttpError)
        {
            // In this case the query was successful
            int version = (int) json["version"];
            Debug.Log($"Checking server translations! (Mine={currentVersion}, Server={version})");
            bool isNewer = version > currentVersion;
            if (isNewer)
            {
                // We download and export the zip with the new translations
                await DownloadLanguageZip();
                
                // And finally, we save the new info
                File.WriteAllText(langInfoPath, json.ToString());
            }
        }
    }

    public void ProcessLanguage(string filePath, Dictionary<string, string> dictionary)
    {
        if (File.Exists(filePath))
        {
            JObject jsonObject = JObject.Parse(File.ReadAllText(filePath));
            foreach(var token in jsonObject)
            {
                dictionary[token.Key] = (string) token.Value;
            }
        }
    }

    private async Task DownloadLanguageZip()
    {
        Debug.Log("Downloading server translations!");
        SaveManager.RecreateFolder(SaveManager.SAVE_FOLDER_LANG);
        UnityWebRequest response = await WebRequestController.SendDownloadTranslations();
        if (!response.isNetworkError && !response.isHttpError)
        {
            byte[] results = response.downloadHandler.data;
            string finalPath = SaveManager.SAVE_FOLDER_LANG + "langfiles.zip";
            File.WriteAllBytes(finalPath,results);
            SaveManager.UnzipFile(finalPath,SaveManager.SAVE_FOLDER_LANG);
            File.Delete(finalPath);
        }
    }

    public static string GetTranslation(string id, bool includeSize = true)
    {
        return GetTranslation(id, _instance.localeTranslations, true, includeSize, null);
    }

    public static string GetTranslation(string id, string[] replacementArray, bool includeSize = true)
    {
        return GetTranslation(id, _instance.localeTranslations, true, includeSize, replacementArray);
    }

    public static string GetTranslation(string id, SystemLanguage targetLanguage)
    {
        // we load the language if not loaded already
        if (!_instance.allTranslations.ContainsKey(targetLanguage))
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            _instance.LoadLanguage(targetLanguage, dict);
        }

        return GetTranslation(id, _instance.allTranslations[targetLanguage], false, false, null);
    }

    private static string GetTranslation(string id, Dictionary<string, string> locale, bool includeTheme, bool includeSize, string[] replacementArray)
    {
        if (id == null || _instance.englishTranslations == null || _instance.englishTranslations.Count == 0)
        {
            return null;
        }

        // we obtain the required text
        string text = FindTranslation(id, locale, includeTheme, includeSize);
        
        // and replace the parameters we need
        if (replacementArray != null)
        {
            text = String.Format(text, replacementArray);
        }

        return text;
    }
    
    private static string FindTranslation(string id, Dictionary<string, string> locale, bool includeTheme, bool includeSize)
    {
        // first we search in theme
        if (includeTheme)
        {
            // first we try to return the current one
            if (_instance.themeTranslations.ContainsKey(id))
            {
                return _instance.themeTranslations[id];
            }

            // otherwise we try to return a legacy one
            string replacement = ThemeController._instance.GetLegacyName(id);
            if (!string.IsNullOrEmpty(replacement) && _instance.themeTranslations.ContainsKey(replacement))
            {
                return _instance.themeTranslations[replacement];
            }
        }
        
        // now we search in our locale
        if (locale.ContainsKey(id))
        {
            string size = includeSize ? _instance.fontSize : "";
            return size + locale[id];
        }

        // finally we check in english
        if (_instance.englishTranslations.ContainsKey(id))
        {
            return _instance.englishTranslations[id];
        }

        return id;
    }

    public bool HasLoadedTranslations()
    {
        return englishTranslations != null && englishTranslations.Count != 0;
    }

    public Dictionary<string, string> GetAllTranslations(SystemLanguage chosenLanguage)
    {
        // we're gonna return a dictionary that uses the theme translations, then current and then english
        // for that we need to load the chosenLanguage if we haven't loaded it already
        Dictionary<string, string> fullDictionary = new Dictionary<string, string>();

        // we re-cache the dictionaries cause i need this to be as light as possible
        Dictionary<string, string> local = new Dictionary<string, string>();
        if (!_instance.allTranslations.ContainsKey(chosenLanguage))
        {
            _instance.LoadLanguage(chosenLanguage, local);
        }
        else
        {
            local = allTranslations[chosenLanguage];
        }

        // we have the 3 dictionaries loaded. now we iterate this
        foreach (string id in englishTranslations.Keys)
        {
            string value = null;
            themeTranslations.TryGetValue(id, out value);

            if (string.IsNullOrEmpty(value))
            {
                // now we try with the local
                local.TryGetValue(id, out value);
            }

            if (string.IsNullOrEmpty(value))
            {
                // now we retrieve from english
                value = englishTranslations[id];
            }
            
            fullDictionary[id] = value;
        }

        return fullDictionary;
    }
}
