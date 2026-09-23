using UnityEngine;

public class JavaPluginBridge : AndroidJavaProxy
{
    AndroidJavaObject pluginObject;
    
    public JavaPluginBridge() : base("com.android.wiiplugin.PluginCallback")
    {
        // We create an instance of the JavaClass in the constructor
        // and pass the reference of this class to the JavaClass
        
        pluginObject = new AndroidJavaObject("com.android.wiiplugin.Plugin", this);
    }
    
    // Call the method in the plugin to invoke the callback
    public void RegisterCallbacks()
    {
        Debug.Log("Registering Android callbacks.");
        pluginObject.Call("registerEvents", AndroidLinker.currentActivity);
    }
    
    // This method will be invoked from the plugin
    public void OnAudioChange(int index)
    {
        Debug.Log($"Received callback with index {index}!");
        if (AudioController._instance != null)
        {
            AudioController._instance.TriggerAudioUpdate();
        }
    } 
}