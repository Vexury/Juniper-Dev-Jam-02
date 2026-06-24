using UnityEngine;

public class SphereRotator : MonoBehaviour
{
    [SerializeField] private InputReader inputReader;
    [SerializeField] private float torqueStrength = 500f;

    private Rigidbody _rb;
    private Vector2 _moveInput;
    private float _rollInput;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

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
    }

    private void OnMove(Vector2 input) => _moveInput = input;
    private void OnRoll(float input) => _rollInput = input;

    private void FixedUpdate()
    {
        if (_moveInput == Vector2.zero && _rollInput == 0f) return;

        Vector3 torque = new Vector3(_moveInput.y, -_moveInput.x, -_rollInput) * torqueStrength;
        _rb.AddTorque(torque, ForceMode.Force);
    }
}
