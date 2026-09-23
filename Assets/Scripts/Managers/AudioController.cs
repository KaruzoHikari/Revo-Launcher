using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class AudioController : MonoBehaviour
{
    public static AudioController _instance;
    public AudioSource soundEffects;
    
    public AudioSource mainAudio;
    private float mainLoopPoint = -1;
    private bool isMainAudioPaused = false;
    
    public AudioSource backgroundAudio;
    public AudioSource shopLoadingAudio;
    private float defaultBackgroundVolume;
    private float defaultSfxVolume = 1f;
    private Coroutine audioReloadCoroutine;
    private Coroutine backCoroutine;

    public bool isMenuMusic = true;
    private bool startedBackgroundMusic = false;

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        RefreshBackgroundVolume();
        RefreshSfxVolume();
    }

    private void Update()
    {
        // we update the looping point of animations!
        if (!isMainAudioPaused && mainAudio.clip is not null && mainLoopPoint >= 0 && !mainAudio.isPlaying)
        {
            mainAudio.time = mainLoopPoint;
            mainAudio.Play();
        }
    }

    public void RegisterAudioCallback()
    {
        if (Application.isMobilePlatform)
        {
            AudioSettings.OnAudioConfigurationChanged += OnAudioConfigurationChanged;
        }
    }

    private void OnAudioConfigurationChanged(bool deviceWasChanged)
    {
        Debug.Log("Audio source event!");
        if (deviceWasChanged)
        {
            TriggerAudioUpdate();
        }
    }

    public void TriggerAudioUpdate(bool showPopup = false)
    {
        if (AppController._instance is null || !AppController._instance.finishedLoading)
        {
            return;
        }

        if (audioReloadCoroutine != null)
        {
            StopCoroutine(audioReloadCoroutine);
        }
        audioReloadCoroutine = StartCoroutine(_TriggerAudioUpdate(showPopup ? 0 : 2, showPopup));
    }

    private IEnumerator _TriggerAudioUpdate(int waitTime, bool showPopup)
    {
        yield return new WaitForSeconds(waitTime);
        Debug.Log("Performing audio update.");
        /*AudioConfiguration config = AudioSettings.GetConfiguration();
        AudioSettings.Reset(config);*/
        
        foreach (ChannelAnimation anim in ChannelController._instance.GetAllLoadedChannelAnimations())
        {
            // not needed i believe
            // anim.LoadAudio();
        }

        RestartBackgroundAudio();

        if (showPopup)
        {
            PopupController.ShowPopup("popup.reloadedaudio");
        }
    }

    public static void RestartBackgroundAudio()
    {
        Debug.Log("Restarting background audio 1!");
        if (_instance.startedBackgroundMusic)
        {
            Debug.Log("Restarting background audio 2!");
            // this is a replacement to a background audio. we need to restart it
            if (_instance.isMenuMusic)
            {
                PlayBackgroundMenuMusic();
            }
            else
            {
                PlayBackgroundShopMusic();
            }
        }
    }

    public static void RefreshBackgroundVolume()
    {
        _instance.defaultBackgroundVolume = PREFS.MusicVolume.GetFloat();
        _instance.backgroundAudio.volume = _instance.defaultBackgroundVolume;
    }
    
    public static void RefreshSfxVolume()
    {
        _instance.defaultSfxVolume = PREFS.SfxVolume.GetFloat();
        _instance.soundEffects.volume = _instance.defaultSfxVolume;
        _instance.shopLoadingAudio.volume = _instance.defaultSfxVolume;
    }

    public static void PlayShopLoadingAudio()
    {
        AudioClip clip = ThemeController.GetAudio(AudioLibrary._instance.SHOP_LOADING) ?? AudioLibrary._instance.SHOP_LOADING;
        _instance.shopLoadingAudio.clip = clip;
        _instance.shopLoadingAudio.loop = true;
        _instance.shopLoadingAudio.Play();
    }

    public static void StopShopLoadingAudio()
    {
        _instance.shopLoadingAudio.Stop();
    }

    public static void PlaySoundEffect(AudioClip audioClip, bool shouldFindThemeReplacement = true)
    {
        AudioClip clip = shouldFindThemeReplacement ? (ThemeController.GetAudio(audioClip) ?? audioClip) : audioClip;
        _instance.soundEffects.PlayOneShot(clip);
    }

    public static void PauseMainAudio()
    {
        _instance.isMainAudioPaused = true;
        _instance.mainAudio.Pause();
    }

    public static void ResumeMainAudio()
    {
        _instance.isMainAudioPaused = false;
        _instance.mainAudio.UnPause();
    }

    public static void PlayMainAudio(AudioClip audioClip, bool shouldLoop = false, float delay = 0f, float loopingPoint = 0f)
    {
        StopMainAudio();
        if (audioClip != null)
        {
            _instance.mainAudio.loop = false;
            _instance.mainLoopPoint = shouldLoop ? loopingPoint : -1;
            AudioClip clip = ThemeController.GetAudio(audioClip) ?? audioClip;
            _instance.mainAudio.clip = clip;
            _instance.mainAudio.PlayDelayed(delay);
        }
    }

    public static void StopMainAudio()
    {
        _instance.mainAudio.Stop();
        _instance.mainAudio.time = 0;
        _instance.mainAudio.loop = false;
        _instance.mainLoopPoint = -1;
        _instance.isMainAudioPaused = false;
    }

    public static void MuteBackgroundAudio(bool fade = false)
    {
        if (fade)
        {
            _instance.backgroundAudio.DOFade(0f, 0.25f);
        }
        else
        {
            _instance.backgroundAudio.mute = true;
        }
    }

    public static void UnmuteBackgroundAudio()
    {
        if (_instance.backgroundAudio.volume < 0.1f)
        {
            _instance.backgroundAudio.volume = _instance.defaultBackgroundVolume;
        }
        _instance.backgroundAudio.mute = false;
    }

    public static void PlayBackgroundMenuMusic(float startingPoint = 0f, bool playJingle = true)
    {
        _instance.isMenuMusic = true;
        if (playJingle)
        {
            PlaySoundEffect(AudioLibrary._instance.MENU_START_JINGLE);
        }
        _instance.PlayBackgroundAudio(AudioLibrary._instance.MUSIC_MENU_START,AudioLibrary._instance.MUSIC_MENU_LOOP, startingPoint: startingPoint);
    }
    
    public static void PlayBackgroundShopMusic(float startingPoint = 0f, bool skipStart = false)
    {
        _instance.isMenuMusic = false;
        _instance.PlayBackgroundAudio(skipStart ? null : AudioLibrary._instance.MUSIC_SHOP_START, AudioLibrary._instance.MUSIC_SHOP_LOOP, startingPoint);
    }

    private void PlayBackgroundAudio(AudioClip startClip = null, AudioClip loopClip = null, float startingPoint = 0f)
    {
        if (backCoroutine != null)
        {
            StopCoroutine(backCoroutine);
        }
        backCoroutine = _instance.StartCoroutine(_PlayMainMenuMusic(startClip,loopClip,startingPoint));
    }

    private IEnumerator _PlayMainMenuMusic(AudioClip originalStart, AudioClip originalLoop, float startingPoint)
    {
        AudioClip start = ThemeController.GetAudio(originalStart) ?? originalStart;
        AudioClip loop = ThemeController.GetAudio(originalLoop) ?? originalLoop;
        backgroundAudio.Stop();
        backgroundAudio.loop = false;
        backgroundAudio.clip = start;
        startedBackgroundMusic = true;
        backgroundAudio.Play();
        backgroundAudio.time = startingPoint;
        float wait = (start is null || start.length - startingPoint < 0) ? 0 : start.length - startingPoint;
        yield return new WaitForSeconds(wait);
        if (start is null || (backgroundAudio.clip != null && backgroundAudio.clip.Equals(start)))
        {
            backgroundAudio.Stop();
            backgroundAudio.loop = true;
            backgroundAudio.clip = loop;
            backgroundAudio.Play();
        }
    }


    public static void LoadAudio(string path, UnityAction<AudioClip> action, bool compressed = true)
    {
        _instance.StartCoroutine(_LoadAudio(path, action, compressed));
    }

    private static IEnumerator _LoadAudio(string soundPath, UnityAction<AudioClip> action, bool compressed)
    {
        // First we fix the url
        string newPath = soundPath.Replace("content://", "file://");
        newPath = FixOggUrl(newPath);
        
        // Then we determine the audio path
        AudioType audioType;
        if (newPath.EndsWith(".wav") || newPath.EndsWith(".bin"))
        {
            audioType = AudioType.WAV;
        }
        else if (newPath.EndsWith(".mp3"))
        {
            audioType = AudioType.MPEG;
        }
        else if (newPath.EndsWith(".ogg"))
        {
            audioType = AudioType.OGGVORBIS;
        }
        else
        {
            audioType = AudioType.UNKNOWN;
        }

        bool shouldAppendFile = Application.isMobilePlatform;
        
        // we need to add the "file" prefix in certain platforms's standalone versions
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX || UNITY_STANDALONE_LINUX
        shouldAppendFile = true; // this was for some reason: !Application.isEditor
#endif
        
        if (shouldAppendFile && !newPath.StartsWith("file://"))
        {
            newPath = "file://" + newPath;
        }

        // and we ask for the file
        using (UnityWebRequest audioFiles = UnityWebRequestMultimedia.GetAudioClip(newPath, audioType))
        {
            ((DownloadHandlerAudioClip)audioFiles.downloadHandler).streamAudio = false;
            // the compressed variable sometimes causes issues and the files don't load properly
            // so first we try to load it compressed, but if it doesn't work then we load it uncompressed
            ((DownloadHandlerAudioClip)audioFiles.downloadHandler).compressed = compressed;
            yield return audioFiles.SendWebRequest();
            try
            {
                if (!audioFiles.isNetworkError)
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(audioFiles);
                    if (clip.length == 0 && compressed)
                    {
                        // it didn't load (probably) so we load it again uncompressed
                        LoadAudio(soundPath, action, false);
                        yield break;
                    }
                    clip.name = StaticUtils.SanitizeString(FileManager.GetFileName(newPath));
                    action?.Invoke(clip);
                }
                else
                {
                    Debug.LogError("Failed to load audio at network request " + newPath);
                    action?.Invoke(null);
                }
            }
            catch (Exception e)
            {
                Debug.Log("Failed to load audio at " + newPath);
                Debug.LogError(e);
                action?.Invoke(null);
            }
        }
    }

    private static string FixOggUrl(string path)
    {
        return path;
        
        // i'm not sure anymore :(
        return path.Replace(".ogg.oga", ".ogg");
    }
}
