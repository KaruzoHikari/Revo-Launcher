using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using Newtonsoft.Json;
using UnityEngine;

public class PreferencesSerializer
{
    public List<UnityPreference> preferences = new List<UnityPreference>();
    private static Mutex savingMutex = new Mutex();

    public static string GetPreferencesPath(string exportPath = null)
    {
        return (exportPath ?? Application.persistentDataPath) + Path.DirectorySeparatorChar + "preferences.json";
    }
    
    public static async void Save(string exportPath = null)
    {
        // weird concurrency errors here.. let's add the mutex
        savingMutex.WaitOne();
        try
        {
            PreferencesSerializer serializer = new PreferencesSerializer
            {
                preferences = PREFS.GetAllPreferences()
            };

            string finalPath = GetPreferencesPath(exportPath);
            JsonSerializerSettings settings = JsonConvert.DefaultSettings.Invoke();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            string content = JsonConvert.SerializeObject(serializer, settings);
            await File.WriteAllTextAsync(finalPath, content);
        }
        catch (Exception e)
        {
            // an error saving preferences
            Debug.Log("There was an error saving your preferences.");
            Debug.LogError(e);
        }
        savingMutex.ReleaseMutex();
    }

    public static void Load(string exportPath = null)
    {
        try
        {
            string preferencesPath = GetPreferencesPath(exportPath);
            if (File.Exists(preferencesPath))
            {
                JsonSerializerSettings settings = JsonConvert.DefaultSettings.Invoke();
                settings.TypeNameHandling = TypeNameHandling.Auto;
                PreferencesSerializer save = JsonConvert.DeserializeObject<PreferencesSerializer>(File.ReadAllText(preferencesPath), settings);
            
                // now we port over the value to our saved ones
                foreach (UnityPreference preference in save.preferences)
                {
                    preference.PortValue(PREFS.GetPreference(preference.GetName(),true));
                }
            }
        }
        catch (Exception e)
        {
            // corrupted json?
            Debug.Log("Corrupted preferences.json detected?");
            Debug.LogError(e);
        }
        
        // we save whatever status we're in
        Save();
    }
    
    /*[OnDeserialized]
    internal void OnDeserializedMethod(StreamingContext context)
    {
        Debug.Log("Preferences have been deserialized.");
        preferences.Clear();
    }*/
}