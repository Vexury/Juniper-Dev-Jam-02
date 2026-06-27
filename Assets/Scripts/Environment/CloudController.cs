using System;
using System.Collections;
using UnityEngine;

public class CloudController : MonoBehaviour
{
    [Serializable]
    private struct FloatAnim
    {
        [Header("Level Start")]
        public float startValue;
        public float endValue;
        public float delay;
        public float duration;
        public AnimationCurve curve;
        [Header("Level Complete")]
        public float completeDuration;
        public AnimationCurve completeCurve;
    }

    [Serializable]
    private struct ColorAnim
    {
        [Header("Level Start")]
        public Color startValue;
        public Color endValue;
        public float delay;
        public float duration;
        public AnimationCurve curve;
        [Header("Level Complete")]
        public float completeDuration;
        public AnimationCurve completeCurve;
    }

    [SerializeField] private Renderer cloudRenderer;

    [Header("Properties")]
    [SerializeField] private FloatAnim density;
    [SerializeField] private FloatAnim scale;
    [SerializeField] private ColorAnim color;
    [SerializeField] private FloatAnim cloudSpeed;
    [SerializeField] private FloatAnim scatterStrength;

    private static readonly int CloudDensityID    = Shader.PropertyToID("_CloudDensity");
    private static readonly int CloudScaleID      = Shader.PropertyToID("_CloudScale");
    private static readonly int CloudColorID      = Shader.PropertyToID("_CloudColor");
    private static readonly int CloudSpeedID      = Shader.PropertyToID("_CloudSpeed");
    private static readonly int ScatterStrengthID = Shader.PropertyToID("_ScatterStrength");

    private MaterialPropertyBlock _block;

    private Coroutine _densityCoroutine;
    private Coroutine _scaleCoroutine;
    private Coroutine _colorCoroutine;
    private Coroutine _speedCoroutine;
    private Coroutine _scatterCoroutine;

    private float _curDensity;
    private float _curScale;
    private Color _curColor;
    private float _curSpeed;
    private float _curScatter;

    private void Awake()
    {
        _block = new MaterialPropertyBlock();
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

    private void HandleLevelReady(int _)
    {
        StopAll();
        SetFloat(CloudDensityID, _curDensity = density.startValue);
        SetFloat(CloudScaleID,   _curScale   = scale.startValue);
        SetColor(CloudColorID,   _curColor   = color.startValue);
        SetFloat(CloudSpeedID,   _curSpeed   = cloudSpeed.startValue);
        SetFloat(ScatterStrengthID, _curScatter = scatterStrength.startValue);
        cloudRenderer.SetPropertyBlock(_block);

        _densityCoroutine = StartCoroutine(AnimateFloat(CloudDensityID, density.startValue,       density.endValue,       density.delay,       density.duration,       density.curve,       v => _curDensity = v));
        _scaleCoroutine   = StartCoroutine(AnimateFloat(CloudScaleID,   scale.startValue,         scale.endValue,         scale.delay,         scale.duration,         scale.curve,         v => _curScale   = v));
        _colorCoroutine   = StartCoroutine(AnimateColor(CloudColorID,   color.startValue,         color.endValue,         color.delay,         color.duration,         color.curve,         v => _curColor   = v));
        _speedCoroutine   = StartCoroutine(AnimateFloat(CloudSpeedID,   cloudSpeed.startValue,    cloudSpeed.endValue,    cloudSpeed.delay,    cloudSpeed.duration,    cloudSpeed.curve,    v => _curSpeed   = v));
        _scatterCoroutine = StartCoroutine(AnimateFloat(ScatterStrengthID, scatterStrength.startValue, scatterStrength.endValue, scatterStrength.delay, scatterStrength.duration, scatterStrength.curve, v => _curScatter = v));
    }

    private void HandleLevelCompleting(int _)
    {
        StopAll();
        _densityCoroutine = StartCoroutine(AnimateFloat(CloudDensityID,    _curDensity, density.startValue,       0f, density.completeDuration,       density.completeCurve,       v => _curDensity = v));
        _scaleCoroutine   = StartCoroutine(AnimateFloat(CloudScaleID,      _curScale,   scale.startValue,         0f, scale.completeDuration,         scale.completeCurve,         v => _curScale   = v));
        _colorCoroutine   = StartCoroutine(AnimateColor(CloudColorID,      _curColor,   color.startValue,         0f, color.completeDuration,         color.completeCurve,         v => _curColor   = v));
        _speedCoroutine   = StartCoroutine(AnimateFloat(CloudSpeedID,      _curSpeed,   cloudSpeed.startValue,    0f, cloudSpeed.completeDuration,    cloudSpeed.completeCurve,    v => _curSpeed   = v));
        _scatterCoroutine = StartCoroutine(AnimateFloat(ScatterStrengthID, _curScatter, scatterStrength.startValue, 0f, scatterStrength.completeDuration, scatterStrength.completeCurve, v => _curScatter = v));
    }

    private void StopAll()
    {
        if (_densityCoroutine != null) StopCoroutine(_densityCoroutine);
        if (_scaleCoroutine   != null) StopCoroutine(_scaleCoroutine);
        if (_colorCoroutine   != null) StopCoroutine(_colorCoroutine);
        if (_speedCoroutine   != null) StopCoroutine(_speedCoroutine);
        if (_scatterCoroutine != null) StopCoroutine(_scatterCoroutine);
    }

    private IEnumerator AnimateFloat(int id, float from, float to, float delay, float duration, AnimationCurve curve, Action<float> setField)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float val = Mathf.Lerp(from, to, curve.Evaluate(Mathf.Clamp01(elapsed / duration)));
            setField(val);
            _block.SetFloat(id, val);
            cloudRenderer.SetPropertyBlock(_block);
            yield return null;
        }
        setField(to);
        _block.SetFloat(id, to);
        cloudRenderer.SetPropertyBlock(_block);
    }

    private IEnumerator AnimateColor(int id, Color from, Color to, float delay, float duration, AnimationCurve curve, Action<Color> setField)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Color val = Color.Lerp(from, to, curve.Evaluate(Mathf.Clamp01(elapsed / duration)));
            setField(val);
            _block.SetColor(id, val);
            cloudRenderer.SetPropertyBlock(_block);
            yield return null;
        }
        setField(to);
        _block.SetColor(id, to);
        cloudRenderer.SetPropertyBlock(_block);
    }

    private void SetFloat(int id, float value) => _block.SetFloat(id, value);
    private void SetColor(int id, Color value)  => _block.SetColor(id, value);
}
