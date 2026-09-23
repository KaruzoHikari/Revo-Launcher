using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioLibrary : MonoBehaviour
{
    public AudioClip CLICK_CHANNEL;
    public AudioClip OPEN_CHANNEL;
    public AudioClip HOVER_CHANNEL;
    public AudioClip LEAVE_CHANNEL;
    public AudioClip START_CHANNEL;
    public AudioClip MUSIC_MENU_START;
    public AudioClip MUSIC_MENU_LOOP;
    public AudioClip MUSIC_SHOP_START;
    public AudioClip MUSIC_SHOP_LOOP;
    public AudioClip MENU_START_JINGLE;
    public AudioClip HOVER_BUTTON;
    public AudioClip CLICK_BUTTON;
    public AudioClip CLICK_BUTTON_BACK;
    public AudioClip CLICK_PLUSLESS;
    public AudioClip CLICK_WARNING;
    public AudioClip SHOP_LOADING;

    public static AudioLibrary _instance;

    private void Awake()
    {
        _instance = this;
    }

    // TODO triinspector actually check if the new method here works, that is if we can see all audio in the theme controller
    public static List<AudioClip> GetAllAudioClips()
    {
        return _instance.GetType().GetFields().Select(field => field.GetValue(_instance)).OfType<AudioClip>().ToList();
    } 
}
