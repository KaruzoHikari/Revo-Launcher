using UnityEngine;

public class AudioData
{
    public string name;
    public int channels;
    public int samples;
    public int frequency;
    public float[] data;

    public static AudioData FromAudioClip(AudioClip audioClip)
    {
        AudioData result = new AudioData();
        result.name = audioClip.name;
        result.channels = audioClip.channels;
        result.samples = audioClip.samples;
        result.frequency = audioClip.frequency;
        result.data = new float[audioClip.samples * audioClip.channels];
        audioClip.GetData(result.data, 0);

        return result;
    }

    public static AudioClip ToAudioClip(AudioData audio)
    {
        if (audio == null)
        {
            return null;
        }
        
        AudioClip result = AudioClip.Create(audio.name, audio.samples, audio.channels, audio.frequency, false);
        result.SetData(audio.data, 0);

        return result;
    }
}