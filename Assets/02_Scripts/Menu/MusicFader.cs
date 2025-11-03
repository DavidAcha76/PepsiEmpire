using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class MusicFader : MonoBehaviour
{
    [Header("Mixer")]
    public AudioMixer mixer;             // arrastra MainMixer
    public string musicParam = "MusicVolume";
    [Header("Volumen objetivo (dB)")]
    [Range(-80f, 0f)] public float targetDb = -12f;
    public float fadeInTime = 1.5f;

    AudioSource _src;

    void Awake() { _src = GetComponent<AudioSource>(); }

    void OnEnable()
    {
        // Inicia silencioso y hace fade-in
        if (mixer) mixer.SetFloat(musicParam, -80f);
        if (!_src.isPlaying) _src.Play();
        StartCoroutine(FadeTo(targetDb, fadeInTime));
    }

    public IEnumerator FadeTo(float db, float time)
    {
        if (!mixer) yield break;
        mixer.GetFloat(musicParam, out float from);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.001f, time);
            float v = Mathf.Lerp(from, db, t);
            mixer.SetFloat(musicParam, v);
            yield return null;
        }
        mixer.SetFloat(musicParam, db);
    }

    // Llama esto si quieres un fade-out antes de cambiar de escena (opcional)
    public void FadeOutAndStop(float time = 1f)
    {
        StartCoroutine(CoFadeOutStop(time));
    }

    IEnumerator CoFadeOutStop(float time)
    {
        mixer.GetFloat(musicParam, out float from);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.001f, time);
            float v = Mathf.Lerp(from, -80f, t);
            mixer.SetFloat(musicParam, v);
            yield return null;
        }
        _src.Stop();
    }
}
