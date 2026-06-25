using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    [SerializeField] private InputReader inputReader;
    [SerializeField] private float zoomStep = 1f;
    [SerializeField] private float minRadius = 4f;
    [SerializeField] private float maxRadius = 20f;

    [Header("Transition In")]
    [SerializeField] private float transitionStartRadius = 30f;
    [SerializeField] private float transitionEndRadius = 10f;
    [SerializeField] private float transitionDelay = 0.5f;
    [SerializeField] private float transitionDuration = 1.5f;

    private CinemachineOrbitalFollow _orbitalFollow;

    private void Awake()
    {
        _orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
    }

    private void Start()
    {
        StartCoroutine(TransitionIn());
    }

    private IEnumerator TransitionIn()
    {
        _orbitalFollow.Radius = transitionStartRadius;
        yield return new WaitForSeconds(transitionDelay);

        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            _orbitalFollow.Radius = Mathf.Lerp(transitionStartRadius, transitionEndRadius, elapsed / transitionDuration);
            yield return null;
        }

        _orbitalFollow.Radius = transitionEndRadius;
    }

    private void OnEnable()
    {
        inputReader.ZoomEvent += OnZoom;
    }

    private void OnDisable()
    {
        inputReader.ZoomEvent -= OnZoom;
    }

    private void OnZoom(float scrollY)
    {
        if (scrollY == 0f) return;
        _orbitalFollow.Radius = Mathf.Clamp(
            _orbitalFollow.Radius - Mathf.Sign(scrollY) * zoomStep,
            minRadius,
            maxRadius
        );
    }
}
