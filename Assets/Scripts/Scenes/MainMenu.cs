using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct LevelStarDisplay
{
    public MeshRenderer[] stars;
}

public class MainMenu : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip menuMusicLoopClip;
    [SerializeField] private AudioClip menuAmbienceLoopClip;
    [SerializeField] private bool fadeAudio = true;
    [SerializeField] private float audioFadeInDuration = 2f;

    [Header("Level Select")]
    [SerializeField] private Button[] levelButtons;

    [Header("Star Display")]
    [SerializeField] private LevelStarDisplay[] levelStarDisplays;
    [SerializeField] private Color starEarnedColor = Color.white;
    [SerializeField] private Color starUnearnedColor = new Color(0.2f, 0.2f, 0.2f, 1f);

    private static readonly int StarColorId = Shader.PropertyToID("_Color");

    [Header("Scene Transition")]
    [SerializeField] private bool fadeOnPlay = true;


    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

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

        int highest = LevelProgress.HighestUnlocked;
        for (int i = 0; i < levelButtons.Length; i++)
        {
            if (levelButtons[i] == null) continue;
            levelButtons[i].interactable = i <= highest;
            int captured = i;
            levelButtons[i].onClick.AddListener(() => OnLevelClicked(captured));
        }

        var mpb = new MaterialPropertyBlock();
        for (int i = 0; i < levelStarDisplays.Length; i++)
        {
            int best = LevelProgress.GetStarHighscore(i);
            MeshRenderer[] stars = levelStarDisplays[i].stars;
            for (int j = 0; j < stars.Length; j++)
            {
                if (stars[j] == null) continue;
                stars[j].GetPropertyBlock(mpb);
                mpb.SetColor(StarColorId, j < best ? starEarnedColor : starUnearnedColor);
                stars[j].SetPropertyBlock(mpb);
            }
        }
    }


    public void OnPlayClicked()
    {
        SceneController.Instance.LoadNextScene(fade: fadeOnPlay);
    }

    private void OnLevelClicked(int index)
    {
        LevelProgress.RequestedLevel = index;
        SceneController.Instance.LoadScene("MarbleGame", fade: fadeOnPlay);
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
