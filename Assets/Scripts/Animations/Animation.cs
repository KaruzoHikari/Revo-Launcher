using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;

public abstract class Animation : HasOptions
{
    [JsonIgnore] public AnimatedImage animInfo;
    public float startTime = 0f;
    public float endTime = 0f;
    
    public bool loops = false;
    public float loopDelay = 0f;
    
    protected bool isActive = false;
    private float lastTime = 0f;
    
    protected virtual void _Start() { }
    protected virtual void _Stop() { }
    protected virtual void _Update() { }
    
    public virtual void Pause() { }
    public virtual void Resume() { }
    
    public void Start()
    {
        isActive = true;
        try
        {
            _Start();
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    public void Stop()
    {
        isActive = false;
        try
        {
            _Stop();
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    public virtual bool IsInTime()
    {
        float time = GetTimelineTime();
        return time > startTime && time < endTime;
    }

    public void Update()
    {
        float time = GetTimelineTime();
        bool isTime = IsInTime();
        if (isTime && (!isActive || time < lastTime))
        {
            Start();
        } else if (isActive && !isTime)
        {
            Stop();
        }

        if (isActive)
        {
            _Update();
        }

        lastTime = time;
    }

    public float GetTimelineTime()
    {
        float time = animInfo.GetTimelineTime();
        if (loops)
        {
            // This returns a looped time between the start time and the end time (+ delay)
            return ((time - startTime) % GetDuration(true)) + startTime;
        }

        return time;
    }

    public float GetDuration(bool countDelay = false)
    {
        if (countDelay)
        {
            return endTime - startTime + loopDelay;
        }
        return endTime - startTime;
    }

    public Animation DeepClone()
    {
        // We're gonna clone it the lazy way, like the images (aka through serialization <-> deserialization)
        string extractedPath = SaveManager.TEMP_SERIALIZE + "anim_" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + ".json";
        Animation cloned = null;
        try
        {
            // -- SERIALIZATION --
            Debug.Log("Deep cloning anim from " + animInfo.channelAnimation.name + ": Serializing...");
            JsonSerializerSettings settings = JsonConvert.DefaultSettings.Invoke();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            File.WriteAllText(extractedPath, JsonConvert.SerializeObject(new DeepSerializationAnimation() { anim = this }, settings));
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

        try
        {
            // -- DESERIALIZATION --
            Debug.Log("Deep cloning anim from " + extractedPath + ": Deserializing...");
            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            DeepSerializationAnimation deep = JsonConvert.DeserializeObject<DeepSerializationAnimation>(File.ReadAllText(extractedPath), settings);
            cloned = deep.anim;
            cloned.animInfo = animInfo;
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }

        return cloned;
    }

    public virtual List<Option> GetOptions()
    {
        List<Option> optionList = new List<Option>();
        
        optionList.Add(Option.Create("editor.title.starttime", () => startTime, x => startTime = x));
        optionList.Add(Option.Create("editor.title.endtime.simple", () => endTime, x => endTime = x));
        optionList.Add(Option.Create("editor.title.shouldloop", () => loops, x => loops = x));
        optionList.Add(Option.Create("editor.title.loopdelay", () => loopDelay, x => loopDelay = x));
        
        return optionList;
    }

    private class DeepSerializationAnimation
    {
        // just a holder to serialize
        public Animation anim;
    }
}
