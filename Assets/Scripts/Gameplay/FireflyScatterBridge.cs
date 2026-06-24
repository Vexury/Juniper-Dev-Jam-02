using UnityEngine;

public class FireflyScatterBridge : MonoBehaviour
{
    [SerializeField] private ParticleSystem fireflies;
    [SerializeField] private Renderer cloudRenderer;
    [SerializeField] private Light fireflyLight;
    [SerializeField] private int maxFireflies = 8;
    [SerializeField] private bool useParticleColor = true;
    [SerializeField] private bool alphaAffectsIntensity = true;
    [SerializeField] private bool sizeAffectsRange = true;
    [SerializeField, Min(1e-4f)] private float coreSoftness = 0.01f;

    private ParticleSystem.Particle[] _particles;
    private Vector4[] _positions;
    private Vector4[] _colors;
    private MaterialPropertyBlock _propBlock;

    private static readonly int PositionsId = Shader.PropertyToID("_FireflyPositions");
    private static readonly int ColorsId    = Shader.PropertyToID("_FireflyColors");
    private static readonly int CountId     = Shader.PropertyToID("_FireflyCount");
    private static readonly int SoftnessId  = Shader.PropertyToID("_FireflyCoreSoftness");

    private void Awake()
    {
        _particles = new ParticleSystem.Particle[maxFireflies];
        _positions = new Vector4[maxFireflies];
        _colors    = new Vector4[maxFireflies];
        _propBlock = new MaterialPropertyBlock();
    }

    private void Update()
    {
        int count = fireflies.GetParticles(_particles, maxFireflies);
        bool worldSpace = fireflies.main.simulationSpace == ParticleSystemSimulationSpace.World;

        // _FireflyPositions.w packs range, _FireflyColors.w packs intensity.
        Color baseColor = fireflyLight.color;
        float baseIntensity = fireflyLight.intensity;
        float baseRange = fireflyLight.range;

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = _particles[i].position;
            if (!worldSpace)
                pos = fireflies.transform.TransformPoint(pos);

            float size = _particles[i].GetCurrentSize(fireflies);
            Color pc   = _particles[i].GetCurrentColor(fireflies);

            float range     = sizeAffectsRange ? baseRange * size : baseRange;
            float intensity = alphaAffectsIntensity ? baseIntensity * pc.a : baseIntensity;
            Color col       = useParticleColor ? baseColor * pc : baseColor;

            _positions[i] = new Vector4(pos.x, pos.y, pos.z, range);
            _colors[i]    = new Vector4(col.r, col.g, col.b, intensity);
        }

        cloudRenderer.GetPropertyBlock(_propBlock);
        _propBlock.SetVectorArray(PositionsId, _positions);
        _propBlock.SetVectorArray(ColorsId, _colors);
        _propBlock.SetInt(CountId, count);
        _propBlock.SetFloat(SoftnessId, coreSoftness);
        cloudRenderer.SetPropertyBlock(_propBlock);
    }
}
