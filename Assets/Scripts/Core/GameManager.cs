using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private InputReader inputReader;
    [SerializeField] private Rigidbody marble;
    [SerializeField] private CameraSwitcher cameraSwitcher;
    [SerializeField] private SphereRotator sphereController;
    [SerializeField] private Level[] levels;
    [SerializeField] private float winLoadDelay = 1.5f;
    [SerializeField] private float sphereResetDuration = 1f;
    [SerializeField] private float sphereResetMarbleDelay = 0.5f;

    [SerializeField] private GameObject pauseMenuUI;

    [SerializeField] private GameObject allLevelsCompleteImage;
    [SerializeField] private GameObject fifthLevelChoiceImage;
    [SerializeField] private float allLevelsCompleteDisplayTime = 4f;
    [SerializeField] private float allLevelsCompleteFadeIn = 0.5f;

    [SerializeField] private AudioClip musicLoop;
    [SerializeField] private AudioClip ambienceLoop;
    [SerializeField] private AudioClip rotationSound;
    [SerializeField] private AudioClip starCollectSound;
    [SerializeField] private AudioClip levelCompleteSound;
    [SerializeField] private float starCollectVolume = 0.5f;
    [SerializeField] private float starPitchStep = 0.15f;

    public static event Action<int> OnLevelCompleting;
    public static event Action<int> OnLevelReady;

    public enum State { Loading, Playing, Failed, Won, Paused }
    public State CurrentState => _state;
    [SerializeField] private State _state;

    private Vector2 _moveInput;
    private float _rollInput;

    private int _currentLevelIndex;
    private Level _currentLevel;
    private StarCollectible[] _currentStars;
    private StarCollectible[] _currentDecoStars;
    private int _starsCollected;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        inputReader.MoveEvent += OnMove;
        inputReader.RollEvent += OnRoll;
        inputReader.ReloadEvent += ResetAll;
    }

    private void OnDisable()
    {
        inputReader.MoveEvent -= OnMove;
        inputReader.RollEvent -= OnRoll;
        inputReader.ReloadEvent -= ResetAll;
        UnsubscribeLevelEvents();
    }

    private void Start()
    {
        _state = State.Loading;
        pauseMenuUI.SetActive(false);
        if (allLevelsCompleteImage != null) allLevelsCompleteImage.SetActive(false);
        if (fifthLevelChoiceImage != null) fifthLevelChoiceImage.SetActive(false);
        inputReader.EnableGameplayInput();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        AudioManager.Instance.PlayMusic(musicLoop);
        AudioManager.Instance.PlayAmbience(ambienceLoop);

        foreach (var level in levels)
            level.gameObject.SetActive(false);

        LoadLevel(LevelProgress.RequestedLevel);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (_state == State.Playing)
                Pause();
            else if (_state == State.Paused)
                Resume();
        }

    }

    public void Pause()
    {
        _state = State.Paused;
        inputReader.DisableAllInput();
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        pauseMenuUI.SetActive(true);
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        inputReader.EnableGameplayInput();
        _state = State.Playing;
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f;
        SceneController.Instance.LoadScene("MainMenu", fade: true);
    }

    public string GetLevelName(int index) =>
        index >= 0 && index < levels.Length ? levels[index].levelName : "";

    private void LoadLevel(int index)
    {
        if (index >= levels.Length) return;

        _currentLevelIndex = index;
        _currentLevel = levels[index];
        _currentLevel.gameObject.SetActive(true);

        sphereController.SetRotation(Quaternion.identity);

        SubscribeLevelEvents();
        _currentStars = _currentLevel.GetComponentsInChildren<StarCollectible>();
        var decoList = new System.Collections.Generic.List<StarCollectible>();
        foreach (var star in _currentStars)
            if (star.DecoStar != null) decoList.Add(star.DecoStar);
        _currentDecoStars = decoList.ToArray();
        _starsCollected = 0;
        SubscribeStarEvents();
        ResetMarble();
        OnLevelReady?.Invoke(index);
    }

    private void UnloadCurrentLevel()
    {
        UnsubscribeLevelEvents();
        UnsubscribeStarEvents();
        _currentStars = null;
        if (_currentDecoStars != null)
            foreach (var deco in _currentDecoStars)
                deco.FadeOut();
        _currentDecoStars = null;
        _starsCollected = 0;
        if (_currentLevel != null)
            _currentLevel.gameObject.SetActive(false);
        _currentLevel = null;
    }

    private void SubscribeLevelEvents()
    {
        if (_currentLevel == null) return;
        _currentLevel.glassShell.OnMarbleFailed += ResetAll;
        _currentLevel.finishZone.OnMarbleFinished += HandleWin;
    }

    private void UnsubscribeLevelEvents()
    {
        if (_currentLevel == null) return;
        _currentLevel.glassShell.OnMarbleFailed -= ResetAll;
        _currentLevel.finishZone.OnMarbleFinished -= HandleWin;
    }

    private void HandleWin()
    {
        if (_state != State.Playing) return;
        _state = State.Won;
        AudioManager.Instance.PlaySFX(levelCompleteSound);
        LevelProgress.SaveStarHighscore(_currentLevelIndex, _starsCollected);
        StartCoroutine(WinRoutine());
    }

    private IEnumerator WinRoutine()
    {
        inputReader.DisableAllInput();
        cameraSwitcher.SwitchToOutside();
        OnLevelCompleting?.Invoke(_currentLevelIndex);
        yield return new WaitForSeconds(winLoadDelay);

        int nextIndex = _currentLevelIndex + 1;
        bool firstTimeFifth = _currentLevelIndex == levels.Length - 2 && LevelProgress.HighestUnlocked < nextIndex;
        LevelProgress.Unlock(nextIndex);
        UnloadCurrentLevel();

        if (firstTimeFifth)
        {
            yield return FadeInPanel(fifthLevelChoiceImage, allLevelsCompleteFadeIn);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            yield break;
        }

        if (nextIndex >= levels.Length)
        {
            if (!LevelProgress.AllLevelsCompleted)
            {
                LevelProgress.MarkAllLevelsCompleted();
                yield return ShowAllLevelsCompleteImage();
            }
            SceneController.Instance.LoadScene("MainMenu", fade: true);
        }
        else
            LoadLevel(nextIndex);
    }

    public void DareTheSixth()
    {
        if (fifthLevelChoiceImage != null) fifthLevelChoiceImage.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        LoadLevel(levels.Length - 1);
    }

    private IEnumerator ShowAllLevelsCompleteImage()
    {
        yield return FadeInPanel(allLevelsCompleteImage, allLevelsCompleteFadeIn);
        if (allLevelsCompleteImage == null) yield break;
        yield return new WaitForSeconds(allLevelsCompleteDisplayTime);
    }

    private IEnumerator FadeInPanel(GameObject panel, float fadeIn)
    {
        if (panel == null) yield break;

        panel.SetActive(true);
        var canvasGroup = panel.GetComponent<CanvasGroup>();

        if (canvasGroup != null && fadeIn > 0f)
        {
            float elapsed = 0f;
            canvasGroup.alpha = 0f;
            while (elapsed < fadeIn)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeIn);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }
    }

    public void ResetAll()
    {
        if (_state != State.Playing) return;
        _state = State.Failed;
        ResetStars();
        cameraSwitcher.SwitchToOutside();
        marble.linearVelocity = Vector3.zero;
        marble.angularVelocity = Vector3.zero;
        marble.isKinematic = true;
        StartCoroutine(SphereResetRoutine());
    }

    private IEnumerator SphereResetRoutine()
    {
        inputReader.DisableAllInput();

        Quaternion start = sphereController.CurrentRotation;
        float elapsed = 0f;

        while (elapsed < sphereResetDuration)
        {
            elapsed += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(elapsed / sphereResetDuration);
            sphereController.SetRotation(Quaternion.Slerp(start, Quaternion.identity, t));
            yield return new WaitForFixedUpdate();
        }

        sphereController.SetRotation(Quaternion.identity);
        yield return new WaitForSeconds(sphereResetMarbleDelay);

        ResetMarble();
        // Consume phantom GlassShell.OnTriggerExit from the teleport before re-enabling play.
        yield return new WaitForFixedUpdate();

        // No fade-in on reset, so release marble and restore play state here directly.
        ReleaseMarbleNow();
        // Consume phantom GlassShell.OnTriggerExit from kinematic->dynamic transition.
        yield return new WaitForFixedUpdate();
        _state = State.Playing;
    }

    public void ReleaseMarble()
    {
        StartCoroutine(ReleaseMarbleRoutine());
    }

    private IEnumerator ReleaseMarbleRoutine()
    {
        ReleaseMarbleNow();
        // Consume phantom GlassShell.OnTriggerExit from kinematic->dynamic transition.
        yield return new WaitForFixedUpdate();
        _state = State.Playing;
    }

    private void ReleaseMarbleNow()
    {
        // Push the kinematic track + teleported marble poses into PhysX before the marble
        // goes dynamic (Auto Sync Transforms is off), then keep the track awake so its
        // concave colliders generate contacts against the freshly-released marble on WebGL.
        Physics.SyncTransforms();
        marble.isKinematic = false;
        sphereController.PrimeAfterRelease();
        inputReader.EnableGameplayInput();
    }

    private void OnMove(Vector2 input) { _moveInput = input; UpdateRotationSound(); }
    private void OnRoll(float input) { _rollInput = input; UpdateRotationSound(); }

    private void UpdateRotationSound()
    {
        if (rotationSound == null) return;
        bool hasInput = _moveInput != Vector2.zero || _rollInput != 0f;
        if (hasInput && !cameraSwitcher.IsInsideMode) AudioManager.Instance.PlayLoopingSFX(rotationSound);
        else AudioManager.Instance.StopLoopingSFX();
    }

    private void SubscribeStarEvents()
    {
        if (_currentStars == null) return;
        foreach (var star in _currentStars)
            star.OnCollected += OnStarCollected;
    }

    private void UnsubscribeStarEvents()
    {
        if (_currentStars == null) return;
        foreach (var star in _currentStars)
            star.OnCollected -= OnStarCollected;
    }

    private void OnStarCollected()
    {
        _starsCollected++;
        float pitch = 1f + (_starsCollected - 1) * starPitchStep;
        AudioManager.Instance.PlaySFXWithPitchVariation(starCollectSound, pitch, pitch, starCollectVolume);
    }

    private void ResetStars()
    {
        _starsCollected = 0;
        if (_currentDecoStars != null)
            foreach (var deco in _currentDecoStars)
                deco.FadeOut();
        if (_currentStars != null)
            foreach (var star in _currentStars)
                star.gameObject.SetActive(true);
    }

    private void ResetMarble()
    {
        if (!marble.isKinematic)
        {
            marble.linearVelocity = Vector3.zero;
            marble.angularVelocity = Vector3.zero;
        }
        marble.isKinematic = true;
        marble.position = _currentLevel.marbleStart.position;
    }
}
