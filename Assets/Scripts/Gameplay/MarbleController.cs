using UnityEngine;

public class MarbleController : MonoBehaviour
{
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Camera insideCam;
    [SerializeField] private float torqueStrength = 500f;

    private Rigidbody _rb;
    private Vector2 _moveInput;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        inputReader.MoveEvent += OnMove;
        _moveInput = Vector2.zero;
    }

    private void OnDisable()
    {
        inputReader.MoveEvent -= OnMove;
        _moveInput = Vector2.zero;
    }

    private void OnMove(Vector2 input) => _moveInput = input;

    private void FixedUpdate()
    {
        if (_moveInput == Vector2.zero) return;

        Vector3 camForward = Vector3.ProjectOnPlane(insideCam.transform.forward, Vector3.up).normalized;
        Vector3 torque = (insideCam.transform.right * _moveInput.y - camForward * _moveInput.x) * torqueStrength;
        _rb.AddTorque(torque, ForceMode.Force);
    }
}
