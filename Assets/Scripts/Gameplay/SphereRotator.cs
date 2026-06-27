using UnityEngine;

public class SphereRotator : MonoBehaviour
{
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Rigidbody[] bodies;
    [SerializeField] private float acceleration = 15f;
    [SerializeField] private float angularDamping = 8f;
    [SerializeField] private float releaseWakeDuration = 0.4f;
    [SerializeField] private float releaseWakeAmplitude = 20f;
    [SerializeField] private float releaseWakeFrequency = 20f;

    private Vector2 _moveInput;
    private float _rollInput;
    private Vector3 _angularVelocity;
    private Quaternion _rotation = Quaternion.identity;
    private float _wakeTimer;

    public Quaternion CurrentRotation => _rotation;
    public bool IsWobbling => _wakeTimer > 0f;

    private void OnEnable()
    {
        inputReader.MoveEvent += OnMove;
        inputReader.RollEvent += OnRoll;
    }

    private void OnDisable()
    {
        inputReader.MoveEvent -= OnMove;
        inputReader.RollEvent -= OnRoll;
        _moveInput = Vector2.zero;
        _rollInput = 0f;
        _angularVelocity = Vector3.zero;
    }

    private void OnMove(Vector2 input) => _moveInput = input;
    private void OnRoll(float input) => _rollInput = input;

    public void SetRotation(Quaternion rotation)
    {
        _rotation = rotation;
        _angularVelocity = Vector3.zero;
        ApplyToBodies();
    }

    public void PrimeAfterRelease() => _wakeTimer = releaseWakeDuration;

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        Vector3 accel = new Vector3(_moveInput.y, -_moveInput.x, -_rollInput) * acceleration;
        _angularVelocity += accel * dt;
        _angularVelocity *= 1f / (1f + angularDamping * dt);

        Vector3 angularStep = _angularVelocity;

        if (_wakeTimer > 0f)
        {
            _wakeTimer -= dt;
            foreach (var body in bodies)
                if (body != null) body.WakeUp();
            float wobble = Mathf.Sin(Time.time * releaseWakeFrequency) * releaseWakeAmplitude * Mathf.Deg2Rad;
            angularStep += new Vector3(0f, wobble, 0f);
        }

        float angle = angularStep.magnitude;
        if (angle > 1e-6f)
        {
            Quaternion delta = Quaternion.AngleAxis(angle * Mathf.Rad2Deg * dt, angularStep / angle);
            _rotation = delta * _rotation;
        }

        ApplyToBodies();
    }

    private void ApplyToBodies()
    {
        foreach (var body in bodies)
            if (body != null) body.MoveRotation(_rotation);
    }
}
