using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider ambienceSlider;
    [SerializeField] Slider sfxSlider;
    [SerializeField] AudioClip sfxPreviewClip;

    private void Start()
    {
        musicSlider.value    = AudioManager.Instance.MusicVolume;
        ambienceSlider.value = AudioManager.Instance.AmbienceVolume;
        sfxSlider.value      = AudioManager.Instance.SFXVolume;

        musicSlider.onValueChanged.AddListener(AudioManager.Instance.SetMusicVolume);
        ambienceSlider.onValueChanged.AddListener(AudioManager.Instance.SetAmbienceVolume);
        sfxSlider.onValueChanged.AddListener(AudioManager.Instance.SetSFXVolume);

        EventTrigger trigger = sfxSlider.gameObject.AddComponent<EventTrigger>();
        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        entry.callback.AddListener(_ => PlaySFXPreview());
        trigger.triggers.Add(entry);
    }

    private void PlaySFXPreview()
    {
        if (sfxPreviewClip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(sfxPreviewClip);
    }
}
