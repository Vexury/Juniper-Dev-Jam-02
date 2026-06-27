using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class AudioManager : Singleton<AudioManager>
{
    [Header("Audio Sources")]
    [SerializeField] private AudioClip buttonClickClip;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource ambienceSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource loopingSfxSource;

    [Header("Volume Settings")]
    [Range(0f, 1f)] [SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float musicVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float ambienceVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;

    private AudioSource musicIntroSource;

    private Coroutine musicFadeCoroutine;
    private Coroutine ambienceFadeCoroutine;

    private const string KeyMusic    = "Vol_Music";
    private const string KeyAmbience = "Vol_Ambience";
    private const string KeySFX      = "Vol_SFX";

    protected override void Awake()
    {
        base.Awake();

        // Create audio sources if they don't exist
        if (musicSource == null)
        {
            musicSource = CreateAudioSource("MusicSource");
            musicSource.loop = true;
        }

        if (ambienceSource == null)
        {
            ambienceSource = CreateAudioSource("AmbienceSource");
            ambienceSource.loop = true;
        }

        if (sfxSource == null)
        {
            sfxSource = CreateAudioSource("SFXSource");
        }

        if (loopingSfxSource == null)
        {
            loopingSfxSource = CreateAudioSource("LoopingSFXSource");
            loopingSfxSource.loop = true;
        }

        musicVolume    = PlayerPrefs.GetFloat(KeyMusic,    musicVolume);
        ambienceVolume = PlayerPrefs.GetFloat(KeyAmbience, ambienceVolume);
        sfxVolume      = PlayerPrefs.GetFloat(KeySFX,      sfxVolume);

        ApplyVolumes();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Only apply if we're in play mode and sources exist
        if (Application.isPlaying && musicSource != null && ambienceSource != null && sfxSource != null)
        {
            ApplyVolumes();
        }
    }
#endif

    private AudioSource CreateAudioSource(string name)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(transform);
        AudioSource source = obj.AddComponent<AudioSource>();
        source.playOnAwake = false;
        return source;
    }

    #region UI
    public void PlayButtonClick()
    {
        if (buttonClickClip != null)
        {
            PlaySFX(buttonClickClip);
        }
    }
    #endregion

    #region Music

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.volume = Curve(musicVolume) * Curve(masterVolume);
        musicSource.Play();
    }

    public void PlayMusicWithIntro(AudioClip intro, AudioClip loop, double loopStartOffset = 0.0)
    {
        StopMusic();

        if (musicIntroSource == null)
            musicIntroSource = CreateAudioSource("MusicIntroSource");

        double startTime = AudioSettings.dspTime + 0.1;
        double introDuration = (double)intro.samples / intro.frequency;
        double loopStartTime = startTime + introDuration + loopStartOffset;

        float vol = Curve(musicVolume) * Curve(masterVolume);

        musicIntroSource.clip = intro;
        musicIntroSource.loop = false;
        musicIntroSource.volume = vol;
        musicIntroSource.PlayScheduled(startTime);
        musicIntroSource.SetScheduledEndTime(loopStartTime);

        musicSource.clip = loop;
        musicSource.loop = true;
        musicSource.volume = vol;
        musicSource.PlayScheduled(loopStartTime);
    }

    public void StopMusic()
    {
        musicSource.Stop();
        if (musicIntroSource != null) musicIntroSource.Stop();
    }

    public void PauseMusic()
    {
        musicSource.Pause();
        if (musicIntroSource != null) musicIntroSource.Pause();
    }

    public void UnpauseMusic()
    {
        musicSource.UnPause();
        if (musicIntroSource != null) musicIntroSource.UnPause();
    }

    public void FadeOutMusic(float duration)
    {
        if (musicFadeCoroutine != null) StopCoroutine(musicFadeCoroutine);
        musicFadeCoroutine = StartCoroutine(FadeOutCoroutine(musicSource, duration));
        if (musicIntroSource != null && musicIntroSource.isPlaying)
            StartCoroutine(FadeOutCoroutine(musicIntroSource, duration));
    }

    public void FadeOutMusicWithPitch(float duration)
    {
        if (musicFadeCoroutine != null) StopCoroutine(musicFadeCoroutine);
        musicFadeCoroutine = StartCoroutine(FadeOutWithPitchCoroutine(musicSource, duration));
        if (musicIntroSource != null && musicIntroSource.isPlaying)
            StartCoroutine(FadeOutWithPitchCoroutine(musicIntroSource, duration));
    }

    public void FadeInMusic(AudioClip clip, float duration)
    {
        if (musicFadeCoroutine != null) StopCoroutine(musicFadeCoroutine);
        musicSource.clip = clip;
        musicSource.volume = 0f;
        musicSource.Play();
        musicFadeCoroutine = StartCoroutine(FadeInCoroutine(musicSource, duration, Curve(musicVolume) * Curve(masterVolume)));
    }

    public void FadeInMusicWithPitch(AudioClip clip, float duration)
    {
        if (musicFadeCoroutine != null) StopCoroutine(musicFadeCoroutine);
        musicSource.clip = clip;
        musicSource.volume = 0f;
        musicSource.pitch = 0f;
        musicSource.Play();
        musicFadeCoroutine = StartCoroutine(FadeInWithPitchCoroutine(musicSource, duration, Curve(musicVolume) * Curve(masterVolume)));
    }

    #endregion

    #region Ambience

    public void PlayAmbience(AudioClip clip, bool loop = true)
    {
        if (ambienceSource.clip == clip && ambienceSource.isPlaying) return;

        ambienceSource.clip = clip;
        ambienceSource.loop = loop;
        ambienceSource.volume = Curve(ambienceVolume) * Curve(masterVolume);
        ambienceSource.Play();
    }

    public void StopAmbience()
    {
        ambienceSource.Stop();
    }

    public void PauseAmbience()
    {
        ambienceSource.Pause();
    }

    public void UnpauseAmbience()
    {
        ambienceSource.UnPause();
    }

    public void FadeOutAmbience(float duration)
    {
        if (ambienceFadeCoroutine != null) StopCoroutine(ambienceFadeCoroutine);
        ambienceFadeCoroutine = StartCoroutine(FadeOutCoroutine(ambienceSource, duration));
    }

    public void FadeOutAmbienceWithPitch(float duration)
    {
        if (ambienceFadeCoroutine != null) StopCoroutine(ambienceFadeCoroutine);
        ambienceFadeCoroutine = StartCoroutine(FadeOutWithPitchCoroutine(ambienceSource, duration));
    }

    public void FadeInAmbience(AudioClip clip, float duration)
    {
        if (ambienceFadeCoroutine != null) StopCoroutine(ambienceFadeCoroutine);
        ambienceSource.clip = clip;
        ambienceSource.volume = 0f;
        ambienceSource.Play();
        ambienceFadeCoroutine = StartCoroutine(FadeInCoroutine(ambienceSource, duration, Curve(ambienceVolume) * Curve(masterVolume)));
    }

    public void FadeInAmbienceWithPitch(AudioClip clip, float duration)
    {
        if (ambienceFadeCoroutine != null) StopCoroutine(ambienceFadeCoroutine);
        ambienceSource.clip = clip;
        ambienceSource.volume = 0f;
        ambienceSource.pitch = 0f;
        ambienceSource.Play();
        ambienceFadeCoroutine = StartCoroutine(FadeInWithPitchCoroutine(ambienceSource, duration, Curve(ambienceVolume) * Curve(masterVolume)));
    }

    #endregion

    #region SFX

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, Curve(sfxVolume) * Curve(masterVolume));
    }

    public void PlaySFX(AudioClip clip, float volumeScale)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, volumeScale * Curve(sfxVolume) * Curve(masterVolume));
    }

    public void PlaySFXAtPoint(AudioClip clip, Vector3 position, float minDistance = 10f, float maxDistance = 50f)
    {
        if (clip == null) return;

        GameObject go = new GameObject("TempSFX");
        go.transform.position = position;
        AudioSource src = go.AddComponent<AudioSource>();
        src.clip = clip;
        src.spatialBlend = 1f;
        src.minDistance = minDistance;
        src.maxDistance = maxDistance;
        src.volume = Curve(sfxVolume) * Curve(masterVolume);
        src.Play();

        Destroy(go, clip.length);
    }

    public void PlaySFXWithPitchVariation(AudioClip clip, float minPitch = 0.9f, float maxPitch = 1.1f, float volumeScale = 1f)
    {
        if (clip == null) return;

        GameObject tempAudio = new GameObject("TempAudio");
        AudioSource tempSource = tempAudio.AddComponent<AudioSource>();
        tempSource.clip = clip;
        tempSource.volume = Mathf.Clamp01(Curve(sfxVolume) * Curve(masterVolume) * volumeScale);
        float pitch = Random.Range(minPitch, maxPitch);
        tempSource.pitch = pitch;
        tempSource.Play();

        Destroy(tempAudio, clip.length / Mathf.Abs(pitch));
    }

    #endregion

    #region Volume Control

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        ApplyVolumes();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(KeyMusic, musicVolume);
        ApplyVolumes();
    }

    public void SetAmbienceVolume(float volume)
    {
        ambienceVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(KeyAmbience, ambienceVolume);
        ApplyVolumes();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(KeySFX, sfxVolume);
        ApplyVolumes();
    }

    public float MusicVolume    => musicVolume;
    public float AmbienceVolume => ambienceVolume;
    public float SFXVolume      => sfxVolume;

    public void SetMusicPitch(float pitch)
    {
        musicSource.pitch = pitch;
        if (musicIntroSource != null && musicIntroSource.isPlaying)
            musicIntroSource.pitch = pitch;
    }

    public void PlayLoopingSFX(AudioClip clip, float volume = 1f)
    {
        if (loopingSfxSource.clip == clip && loopingSfxSource.isPlaying) return;
        loopingSfxSource.clip = clip;
        loopingSfxSource.volume = Curve(sfxVolume) * Curve(masterVolume) * volume;
        loopingSfxSource.Play();
    }

    public void StopLoopingSFX()
    {
        loopingSfxSource.Stop();
    }

    private static float Curve(float v) => v * v;

    private void ApplyVolumes()
    {
        float master = Curve(masterVolume);
        musicSource.volume       = Curve(musicVolume)    * master;
        ambienceSource.volume    = Curve(ambienceVolume) * master;
        sfxSource.volume         = Curve(sfxVolume)      * master;
        loopingSfxSource.volume  = Curve(sfxVolume)      * master;
        if (musicIntroSource != null)
            musicIntroSource.volume = Curve(musicVolume) * master;
    }

    #endregion

    #region Fade Coroutines

    private IEnumerator FadeOutWithPitchCoroutine(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float startPitch  = source.pitch;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            source.volume = Mathf.Lerp(startVolume, 0f, t);
            source.pitch  = Mathf.Lerp(startPitch,  0f, t);
            yield return null;
        }

        source.Stop();
        source.volume = startVolume;
        source.pitch  = startPitch;
    }

    private IEnumerator FadeOutCoroutine(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        source.Stop();
        source.volume = startVolume;
    }

    private IEnumerator FadeInWithPitchCoroutine(AudioSource source, float duration, float targetVolume)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            source.volume = Mathf.Lerp(0f, targetVolume, t);
            source.pitch  = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        source.volume = targetVolume;
        source.pitch  = 1f;
    }

    private IEnumerator FadeInCoroutine(AudioSource source, float duration, float targetVolume)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
            yield return null;
        }

        source.volume = targetVolume;
    }

    #endregion
}