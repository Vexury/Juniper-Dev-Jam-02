using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private InputReader inputReader;
    [SerializeField] private Rigidbody marble;
    [SerializeField] private Transform marbleStart;
    [SerializeField] private GlassShell glassShell;
    [SerializeField] private FinishZone finishZone;
    [SerializeField] private Rigidbody sphereRoot;
    [SerializeField] private CameraSwitcher cameraSwitcher;
    [SerializeField] private float winLoadDelay = 1.5f;
    [SerializeField] private float sphereResetDuration = 1f;
    [SerializeField] private float sphereResetMarbleDelay = 0.5f;

    [SerializeField] private AudioClip musicLoop;
    [SerializeField] private AudioClip ambienceLoop;

    private enum State { Playing, Failed, Won }
    private State _state;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        glassShell.OnMarbleFailed += ResetAll;
        finishZone.OnMarbleFinished += HandleWin;
    }

    private void OnDisable()
    {
        glassShell.OnMarbleFailed -= ResetAll;
        finishZone.OnMarbleFinished -= HandleWin;
    }

    private void Start()
    {
        _state = State.Playing;
        inputReader.EnableGameplayInput();

        AudioManager.Instance.PlayMusic(musicLoop);
        AudioManager.Instance.PlayAmbience(ambienceLoop);
    }

    private void HandleWin()
    {
        if (_state != State.Playing) return;
        _state = State.Won;
        StartCoroutine(WinRoutine());
    }

    private IEnumerator WinRoutine()
    {
        inputReader.DisableAllInput();
        yield return new WaitForSeconds(winLoadDelay);
        SceneController.Instance.LoadNextScene();
    }

    public void ResetAll()
    {
        if (_state != State.Playing) return;
        _state = State.Failed;
        cameraSwitcher.SwitchToOutside();
        marble.linearVelocity = Vector3.zero;
        marble.angularVelocity = Vector3.zero;
        marble.isKinematic = true;
        StartCoroutine(SphereResetRoutine());
    }

    private IEnumerator SphereResetRoutine()
    {
        inputReader.DisableAllInput();

        Quaternion start = sphereRoot.rotation;
        float elapsed = 0f;

        while (elapsed < sphereResetDuration)
        {
            elapsed += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(elapsed / sphereResetDuration);
            sphereRoot.MoveRotation(Quaternion.Slerp(start, Quaternion.identity, t));
            yield return new WaitForFixedUpdate();
        }

        sphereRoot.MoveRotation(Quaternion.identity);
        yield return new WaitForSeconds(sphereResetMarbleDelay);

        ResetMarble();
        // Let physics process the teleport so the resulting OnTriggerExit is
        // consumed while still in Failed state, instead of re-triggering a reset.
        yield return new WaitForFixedUpdate();

        inputReader.EnableGameplayInput();
        _state = State.Playing;
    }

    private void ResetMarble()
    {
        marble.position = marbleStart.position;
        marble.isKinematic = false;
        marble.linearVelocity = Vector3.zero;
        marble.angularVelocity = Vector3.zero;
    }
}
