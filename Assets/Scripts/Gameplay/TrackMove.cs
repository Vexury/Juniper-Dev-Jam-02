using UnityEngine;

public class TrackMove : MonoBehaviour
{
    public enum Mode { Patrol, Rotate }
    public enum PatrolAxis { X, Y, Z }

    [SerializeField] private Mode mode = Mode.Patrol;

    [Header("Patrol")]
    [SerializeField] private PatrolAxis patrolAxis = PatrolAxis.X;
    [SerializeField] private float range = 2f;
    [SerializeField] private float speed = 1f;
    [SerializeField] private AnimationCurve curve;

    [Header("Rotate")]
    [SerializeField] private Transform pivot;
    [SerializeField] private Vector3 rotateAxis = Vector3.up;
    [SerializeField] private float degreesPerSecond = 90f;

    private Vector3 _startLocalPos;
    private Vector3 _patrolLocalDir;
    private float _t;
    private int _direction = 1;

    private Vector3 _initialOffset;
    private Vector3 _centerLocalOffset;
    private float _totalAngle;

    private void Reset()
    {
        curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    }

    private void Awake()
    {
        _startLocalPos = transform.localPosition;

        _patrolLocalDir = patrolAxis switch
        {
            PatrolAxis.X => Vector3.right,
            PatrolAxis.Y => Vector3.up,
            PatrolAxis.Z => Vector3.forward,
            _ => Vector3.right
        };

        if (mode == Mode.Rotate)
        {
            if (pivot != null)
            {
                _initialOffset = transform.position - pivot.position;
            }
            else
            {
                var rend = GetComponentInChildren<Renderer>();
                _centerLocalOffset = rend != null
                    ? transform.InverseTransformPoint(rend.bounds.center)
                    : Vector3.zero;
            }
        }
    }

    private void FixedUpdate()
    {
        if (mode == Mode.Patrol)
            DoPatrol();
        else
            DoRotate();
    }

    private void DoPatrol()
    {
        _t += _direction * speed * Time.fixedDeltaTime;
        if (_t >= 1f) { _t = 1f; _direction = -1; }
        else if (_t <= 0f) { _t = 0f; _direction = 1; }

        float curved = curve.Evaluate(_t);
        transform.localPosition = _startLocalPos + _patrolLocalDir * Mathf.Lerp(-range * 0.5f, range * 0.5f, curved);
    }

    private void DoRotate()
    {
        if (pivot == null)
        {
            Vector3 worldCenter = transform.TransformPoint(_centerLocalOffset);
            Vector3 worldAxis = transform.TransformDirection(rotateAxis.normalized);
            transform.RotateAround(worldCenter, worldAxis, degreesPerSecond * Time.fixedDeltaTime);
            return;
        }

        _totalAngle += degreesPerSecond * Time.fixedDeltaTime;
        transform.position = pivot.position + Quaternion.AngleAxis(_totalAngle, rotateAxis.normalized) * _initialOffset;
    }
}
