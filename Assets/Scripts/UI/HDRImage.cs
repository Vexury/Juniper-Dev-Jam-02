using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[ExecuteAlways]
public class HDRImage : MonoBehaviour
{
    [ColorUsage(true, true)]
    public Color hdrColor = Color.white;

    static readonly int ColorId = Shader.PropertyToID("_Color");

    Image _image;
    Material _source;
    Material _instance;

    void OnEnable() => Apply();
    void OnValidate() => Apply();

    void OnDisable()
    {
        // Restore the shared material and drop our private clone.
        if (_image != null && _instance != null && _image.material == _instance)
            _image.material = _source;
        if (_instance != null)
        {
            if (Application.isPlaying) Destroy(_instance);
            else DestroyImmediate(_instance);
            _instance = null;
        }
    }

    void Apply()
    {
        if (_image == null) _image = GetComponent<Image>();
        if (_image == null || _image.material == null) return;

        // Give this Image its OWN material clone so setting the HDR color never
        // writes back to the shared asset / default UI material every Image uses.
        if (_image.material != _instance)
        {
            _source = _image.material;
            _instance = new Material(_source) { hideFlags = HideFlags.DontSave };
            _image.material = _instance;
        }

        _instance.SetColor(ColorId, hdrColor);
    }
}
