using System.Collections;
using TMPro;
using UnityEngine;

public class LevelNameDisplay : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text label;

    [SerializeField] private float fadeInDelay = 0.5f;
    [SerializeField] private float fadeInDuration = 0.8f;
    [SerializeField] private float holdDuration = 2f;
    [SerializeField] private float fadeOutDuration = 0.8f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private void Awake()
    {
        canvasGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        GameManager.OnLevelReady += HandleLevelReady;
        GameManager.OnLevelCompleting += HandleLevelCompleting;
    }

    private void OnDisable()
    {
        GameManager.OnLevelReady -= HandleLevelReady;
        GameManager.OnLevelCompleting -= HandleLevelCompleting;
    }

    private void HandleLevelReady(int index)
    {
        StopAllCoroutines();
        label.text = GameManager.Instance.GetLevelName(index);
        StartCoroutine(ShowRoutine());
    }

    private void HandleLevelCompleting(int _)
    {
        StopAllCoroutines();
        if (canvasGroup.alpha > 0f)
            StartCoroutine(Fade(canvasGroup.alpha, 0f, fadeOutDuration));
    }

    private IEnumerator ShowRoutine()
    {
        if (fadeInDelay > 0f) yield return new WaitForSeconds(fadeInDelay);
        yield return Fade(0f, 1f, fadeInDuration);
        if (holdDuration > 0f) yield return new WaitForSeconds(holdDuration);
        yield return Fade(1f, 0f, fadeOutDuration);
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, fadeCurve.Evaluate(Mathf.Clamp01(elapsed / duration)));
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}
