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
    [SerializeField] private float minBumpSpeed = 1f;
    [SerializeField] private float maxBumpSpeed = 8f;
    [Range(0f, 1f)] [SerializeField] private float bumpMinVolume = 0.3f;
    [Range(0f, 1f)] [SerializeField] private float bumpMaxVolume = 1f;
    [SerializeField] private float bumpMinPitch = 0.9f;
    [SerializeField] private float bumpMaxPitch = 1.1f;

    [Header("Debug")]
    [SerializeField] private bool logSpeed = false;

    private Rigidbody _rb;
    private int _contactCount;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (source == null) source = GetComponent<AudioSource>();
        source.loop = true;
        source.playOnAwake = false;
        source.volume = 0f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        _contactCount++;
        PlayBump(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (_contactCount > 0) _contactCount--;
    }

    private void PlayBump(Collision collision)
    {
        if (bumpSource == null || bumpClips == null || bumpClips.Length == 0) return;

        ContactPoint contact = collision.GetContact(0);
        float impact = Mathf.Abs(Vector3.Dot(collision.relativeVelocity, contact.normal));
        if (impact < minBumpSpeed) return;

        float t = Mathf.Clamp01(Mathf.InverseLerp(minBumpSpeed, maxBumpSpeed, impact));
        AudioClip clip = bumpClips[Random.Range(0, bumpClips.Length)];
        bumpSource.pitch = Random.Range(bumpMinPitch, bumpMaxPitch);
        bumpSource.PlayOneShot(clip, Mathf.Lerp(bumpMinVolume, bumpMaxVolume, t));
    }

    private void Update()
    {
        bool grounded = _contactCount > 0 && !_rb.isKinematic;
        float speed = grounded ? _rb.linearVelocity.magnitude : 0f;

        if (logSpeed) Debug.Log($"Marble speed: {_rb.linearVelocity.magnitude:F2} (grounded: {grounded})");
        float t = Mathf.Clamp01(Mathf.InverseLerp(minSpeed, maxSpeed, speed));

        float targetVolume = grounded ? Mathf.Lerp(minVolume, maxVolume, t) : 0f;
        float targetPitch = Mathf.Lerp(minPitch, maxPitch, t);

        float k = 1f - Mathf.Exp(-smoothing * Time.deltaTime);
        source.volume = Mathf.Lerp(source.volume, targetVolume, k);
        source.pitch = Mathf.Lerp(source.pitch, targetPitch, k);

        if (!source.isPlaying && source.volume > 0.001f) source.Play();
        else if (source.isPlaying && source.volume <= 0.001f && targetVolume <= 0f) source.Stop();
    }
}
