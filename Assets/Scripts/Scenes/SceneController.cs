using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : Singleton<SceneController>
{
    
    [SerializeField] float fadeDuration = 0.4f;
    CanvasGroup fadeGroup;
    
    private bool isLoading = false;

    protected override void Awake()
    {
        base.Awake();
        BuildFadeCanvas();
    }

    public void LoadScene(string sceneName, bool fade = false)
    {
        if (isLoading) return;
        if (fade) StartCoroutine(FadeAndLoad(sceneName));
        else SceneManager.LoadScene(sceneName);
    }

    public void LoadScene(int sceneIndex, bool fade = false)
    {
        if (isLoading) return;
        if (fade) StartCoroutine(FadeAndLoad(sceneIndex));
        else SceneManager.LoadScene(sceneIndex);
    }

    public void LoadNextScene(bool fade = false)
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings) LoadScene(nextSceneIndex, fade);
        else Debug.LogWarning("No next scene! This is the last scene in Build Settings.");
    }

    public void LoadSceneAsync(string sceneName)
    {
        if (!isLoading)
            StartCoroutine(LoadSceneAsyncCoroutine(sceneName));
    }

    public void ReloadCurrentScene(bool fade = false)
    {
        LoadScene(SceneManager.GetActiveScene().name, fade);
    }

    private IEnumerator LoadSceneAsyncCoroutine(string sceneName)
    {
        isLoading = true;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
            yield return null;

        isLoading = false;
    }

    void BuildFadeCanvas()
    {
        var canvasGO = new GameObject("FadeCanvas");
        canvasGO.transform.SetParent(transform);

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        fadeGroup = canvasGO.AddComponent<CanvasGroup>();
        fadeGroup.alpha = 0f;
        fadeGroup.blocksRaycasts = false;
        fadeGroup.interactable = false;

        var imageGO = new GameObject("FadeImage");
        imageGO.transform.SetParent(canvasGO.transform, false);

        var rt = imageGO.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        var image = imageGO.AddComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = false;
    }

    IEnumerator FadeAndLoad(string sceneName)
    {
        isLoading = true;
        fadeGroup.blocksRaycasts = true;
        yield return StartCoroutine(Fade(0f, 1f));
        yield return SceneManager.LoadSceneAsync(sceneName);
        yield return StartCoroutine(Fade(1f, 0f));
        fadeGroup.blocksRaycasts = false;
        isLoading = false;
    }

    IEnumerator FadeAndLoad(int sceneIndex)
    {
        isLoading = true;
        fadeGroup.blocksRaycasts = true;
        yield return StartCoroutine(Fade(0f, 1f));
        yield return SceneManager.LoadSceneAsync(sceneIndex);
        yield return StartCoroutine(Fade(1f, 0f));
        fadeGroup.blocksRaycasts = false;
        isLoading = false;
    }

    IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeGroup.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }
        fadeGroup.alpha = to;
    }
}
