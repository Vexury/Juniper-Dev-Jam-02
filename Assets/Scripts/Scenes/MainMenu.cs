using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip menuMusicLoopClip;
    [SerializeField] private AudioClip menuAmbienceLoopClip;
    [SerializeField] private bool fadeAudio = true;
    [SerializeField] private float audioFadeInDuration = 2f;

    [Header("Scene Transition")]
    [SerializeField] private bool fadeOnPlay = true;


    private void Start()
    {
        if (menuMusicLoopClip != null)
        {
            if (fadeAudio) AudioManager.Instance.FadeInMusic(menuMusicLoopClip, audioFadeInDuration);
            else AudioManager.Instance.PlayMusic(menuMusicLoopClip);
        }
        if (menuAmbienceLoopClip != null)
        {
            if (fadeAudio) AudioManager.Instance.FadeInAmbience(menuAmbienceLoopClip, audioFadeInDuration);
            else AudioManager.Instance.PlayAmbience(menuAmbienceLoopClip);
        }
    }


    public void OnPlayClicked()
    {
        SceneController.Instance.LoadNextScene(fade: fadeOnPlay);
    }

    public void OnSettingsClicked() 
    { 
    
    }

    public void OnQuitClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
