using System.Collections;
using UnityEngine;

public class LevelDitherTransition : MonoBehaviour
{
    [SerializeField] private Material material;
    [SerializeField] private float fadeInDuration = 0.8f;
    [SerializeField] private float fadeOutDuration = 0.8f;

    private static readonly int DitherThreshold = Shader.PropertyToID("_DitherThreshold");

    private void OnEnable()
    {
        material.SetFloat(DitherThreshold, 1f);
        GameManager.OnLevelCompleting += HandleCompleting;
        GameManager.OnLevelReady += HandleReady;
    }

    private void OnDisable()
    {
        GameManager.OnLevelCompleting -= HandleCompleting;
        GameManager.OnLevelReady -= HandleReady;
    }

    private void HandleCompleting(int _) => StartCoroutine(Fade(0f, 1f, fadeOutDuration));
    private void HandleReady(int _) => StartCoroutine(FadeInAndRelease());

    private IEnumerator FadeInAndRelease()
    {
        yield return Fade(1f, 0f, fadeInDuration);
        GameManager.Instance.ReleaseMarble();
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            material.SetFloat(DitherThreshold, Mathf.Lerp(from, to, elapsed / duration));
            yield return null;
        }
        material.SetFloat(DitherThreshold, to);
    }
}
