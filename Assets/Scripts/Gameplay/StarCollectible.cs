using System;
using System.Collections;
using UnityEngine;

public class StarCollectible : MonoBehaviour
{
    public event Action OnCollected;

    [SerializeField] bool _isDecorative;
    [SerializeField] StarCollectible _decoStar;
    [SerializeField] float _fadeInDuration = 0.5f;

    public bool IsDecorative => _isDecorative;
    public StarCollectible DecoStar => _decoStar;

    static readonly int _ditherThresholdId = Shader.PropertyToID("_DitherThreshold");

    MeshRenderer _meshRenderer;
    MaterialPropertyBlock _mpb;
    float _currentThreshold = 1f;

    void Awake()
    {
        _meshRenderer = GetComponentInChildren<MeshRenderer>();
        _mpb = new MaterialPropertyBlock();
        SetThreshold(1f);
    }

    void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(FadeIn());
    }

    public void FadeOut()
    {
        if (!gameObject.activeInHierarchy) return;
        StopAllCoroutines();
        StartCoroutine(FadeOutRoutine());
    }

    IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < _fadeInDuration)
        {
            t += Time.deltaTime;
            SetThreshold(1f - Mathf.Clamp01(t / _fadeInDuration));
            yield return null;
        }
        SetThreshold(0f);
    }

    IEnumerator FadeOutRoutine()
    {
        float start = _currentThreshold;
        float t = 0f;
        while (t < _fadeInDuration)
        {
            t += Time.deltaTime;
            SetThreshold(Mathf.Lerp(start, 1f, Mathf.Clamp01(t / _fadeInDuration)));
            yield return null;
        }
        SetThreshold(1f);
        gameObject.SetActive(false);
    }

    void SetThreshold(float value)
    {
        _currentThreshold = value;
        _meshRenderer.GetPropertyBlock(_mpb);
        _mpb.SetFloat(_ditherThresholdId, value);
        _meshRenderer.SetPropertyBlock(_mpb);
    }

    void OnTriggerEnter(Collider other)
    {
        if (_isDecorative || !other.CompareTag("Marble")) return;
        if (_decoStar != null) _decoStar.gameObject.SetActive(true);
        OnCollected?.Invoke();
        gameObject.SetActive(false);
    }
}
