using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MarbleRollSound : MonoBehaviour
{
    [SerializeField] private AudioSource source;

    [Header("Speed Range")]
    [SerializeField] private float minSpeed = 0.2f;
    [SerializeField] private float maxSpeed = 6f;

    [Header("Volume")]
    [Range(0f, 1f)] [SerializeField] private float minVolume = 0f;
    [Range(0f, 1f)] [SerializeField] private float maxVolume = 1f;

    [Header("Pitch")]
    [SerializeField] private float minPitch = 0.7f;
    [SerializeField] private float maxPitch = 1.6f;

    [Header("Smoothing")]
    [SerializeField] private float smoothing = 12f;

    [Header("Bump")]
    [SerializeField] private AudioSource bumpSource;
    [SerializeField] private AudioClip[] bumpClips;
    [SerializeField] private float bumpMinDelta = 1.5f;
    [SerializeField] private float bumpMaxDelta = 8f;
    [SerializeField] private float bumpCooldown = 0.08f;
    [Range(0f, 1f)] [SerializeField] private float bumpMinVolume = 0.3f;
    [Range(0f, 1f)] [SerializeField] private float bumpMaxVolume = 1f;
    [SerializeField] private float bumpMinPitch = 0.9f;
    [SerializeField] private float bumpMaxPitch = 1.1f;

    [Header("SFX Volume")]
    [SerializeField] private bool respectSfxVolume = true;

    private Rigidbody _rb;
    private int _contactCount;
    private Vector3 _prevVelocity;
    private float _bumpTimer;
    private AudioManager _audio;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (source == null) source = GetComponent<AudioSource>();
        source.loop = true;
        source.playOnAwake = false;
        source.volume = 0f;
        _prevVelocity = _rb.linearVelocity;
    }

    private void Start()
    {
        if (respectSfxVolume) _audio = AudioManager.Instance;
    }

    // Matches AudioManager's perceptual curve (v*v) so the marble tracks the SFX slider in step with other SFX.
    private float SfxMultiplier()
    {
        if (!respectSfxVolume || _audio == null) return 1f;
        float v = _audio.SFXVolume;
        return v * v;
    }

    private void OnCollisionEnter(Collision collision)
    {
        _contactCount++;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (_contactCount > 0) _contactCount--;
    }

    private void FixedUpdate()
    {
        if (_bumpTimer > 0f) _bumpTimer -= Time.fixedDeltaTime;

        // Kinematic phases (co-driven by SphereRotator) carry no meaningful velocity;
        // resync the baseline so re-activation doesn't register as a phantom impact.
        if (_rb.isKinematic)
        {
            _prevVelocity = _rb.linearVelocity;
            return;
        }

        Vector3 velocity = _rb.linearVelocity;
        float delta = (velocity - _prevVelocity).magnitude;
        _prevVelocity = velocity;

        if (_contactCount <= 0) return;
        if (delta < bumpMinDelta) return;
        if (_bumpTimer > 0f) return;

        PlayBump(delta);
        _bumpTimer = bumpCooldown;
    }

    private void PlayBump(float delta)
    {
        if (bumpSource == null || bumpClips == null || bumpClips.Length == 0) return;

        float t = Mathf.Clamp01(Mathf.InverseLerp(bumpMinDelta, bumpMaxDelta, delta));
        AudioClip clip = bumpClips[Random.Range(0, bumpClips.Length)];
        bumpSource.pitch = Random.Range(bumpMinPitch, bumpMaxPitch);
        float volume = Mathf.Lerp(bumpMinVolume, bumpMaxVolume, t) * SfxMultiplier();
        bumpSource.PlayOneShot(clip, volume);
    }

    private void Update()
    {
        bool grounded = _contactCount > 0 && !_rb.isKinematic;
        float speed = grounded ? _rb.linearVelocity.magnitude : 0f;
        float t = Mathf.Clamp01(Mathf.InverseLerp(minSpeed, maxSpeed, speed));

        float targetVolume = grounded ? Mathf.Lerp(minVolume, maxVolume, t) * SfxMultiplier() : 0f;
        float targetPitch = Mathf.Lerp(minPitch, maxPitch, t);

        float k = 1f - Mathf.Exp(-smoothing * Time.deltaTime);
        source.volume = Mathf.Lerp(source.volume, targetVolume, k);
        source.pitch = Mathf.Lerp(source.pitch, targetPitch, k);

        if (!source.isPlaying && source.volume > 0.001f) source.Play();
        else if (source.isPlaying && source.volume <= 0.001f && targetVolume <= 0f) source.Stop();
    }
}
