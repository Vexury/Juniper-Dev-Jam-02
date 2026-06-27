using System.Collections;
using UnityEngine;

public class BootLoader : MonoBehaviour
{
    [SerializeField] private float bootDelay = 0.5f;
    [SerializeField] private AudioClip introClip;

    private void Start()
    {
        InitializeManagers();

        AudioManager.Instance.PlaySFX(introClip, 0.5f);

        StartCoroutine(LoadFirstScene());
    }

    private void InitializeManagers()
    {
        var _ = AudioManager.Instance;
        var __ = SceneController.Instance;
    }

    private IEnumerator LoadFirstScene()
    {
        yield return new WaitForSeconds(bootDelay);
        SceneController.Instance.LoadNextScene(fade: true);
    }
}