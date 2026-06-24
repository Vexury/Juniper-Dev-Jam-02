using System.Collections;
using UnityEngine;

public class BootLoader : MonoBehaviour
{
    [SerializeField] private float bootDelay = 0.5f;

    private void Start()
    {
        InitializeManagers();
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