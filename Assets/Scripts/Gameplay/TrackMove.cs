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
    [SerializeField] private bool startReversed = false;
    [SerializeField] private AnimationCurve curve;

    [Header("Rotate")]
    [SerializeField] private Transform pivot;
    [SerializeField] private Vector3 rotateAxis = Vector3.up;
    [SerializeField] private float degreesPerSecond = 90f;

    private Rigidbody _rb;
    private Quaternion _initialLocalRotation;

    private Vector3 _startLocalPos;
    private Vector3 _patrolLocalDir;
    private float _t;
    private int _direction;

    private Vector3 _centerLocalOffset;
    private Vector3 _centerParentLocalPos;

    private Vector3 _initialOffset;
    private Vector3 _pivotParentLocal;
    private Vector3 _initialOffsetParentLocal;

    private float _totalAngle;

    private void Reset()
    {
        curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _initialLocalRotation = transform.localRotation;
        _startLocalPos = transform.localPosition;
        _direction = startReversed ? -1 : 1;
        _t = startReversed ? 1f : 0f;

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
                if (transform.parent != null)
                {
                    _pivotParentLocal = transform.parent.InverseTransformPoint(pivot.position);
                    _initialOffsetParentLocal = _startLocalPos - _pivotParentLocal;
                }
            }
            else
            {
                var rend = GetComponentInChildren<Renderer>();
                _centerLocalOffset = rend != null
                    ? transform.InverseTransformPoint(rend.bounds.center)
                    : Vector3.zero;
                if (transform.parent != null)
                    _centerParentLocalPos = _startLocalPos + _initialLocalRotation * _centerLocalOffset;
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
        Vector3 targetLocalPos = _startLocalPos + _patrolLocalDir * Mathf.Lerp(-range * 0.5f, range * 0.5f, curved);

        if (_rb != null)
        {
            Vector3 worldPos = transform.parent != null
                ? transform.parent.TransformPoint(targetLocalPos)
                : targetLocalPos;
            Quaternion worldRot = transform.parent != null
                ? transform.parent.rotation * _initialLocalRotation
                : _initialLocalRotation;
            _rb.MovePosition(worldPos);
            _rb.MoveRotation(worldRot);
        }
        else
        {
            transform.localPosition = targetLocalPos;
        }
    }

    private void DoRotate()
    {
        if (pivot == null)
        {
            if (_rb != null && transform.parent != null)
            {
                _totalAngle += degreesPerSecond * Time.fixedDeltaTime;
                Quaternion parentRot = transform.parent.rotation;
                Vector3 parentPos = transform.parent.position;
                Quaternion worldRot = parentRot * _initialLocalRotation * Quaternion.AngleAxis(_totalAngle, rotateAxis.normalized);
                Vector3 worldCenter = parentPos + parentRot * _centerParentLocalPos;
                _rb.MoveRotation(worldRot);
                _rb.MovePosition(worldCenter - worldRot * _centerLocalOffset);
            }
            else if (_rb != null)
            {
                Vector3 worldCenter = transform.TransformPoint(_centerLocalOffset);
                Vector3 worldAxis = transform.TransformDirection(rotateAxis.normalized);
                Quaternion delta = Quaternion.AngleAxis(degreesPerSecond * Time.fixedDeltaTime, worldAxis);
                _rb.MovePosition(worldCenter + delta * (transform.position - worldCenter));
                _rb.MoveRotation(delta * _rb.rotation);
            }
            else
            {
                Vector3 worldCenter = transform.TransformPoint(_centerLocalOffset);
                Vector3 worldAxis = transform.TransformDirection(rotateAxis.normalized);
                transform.RotateAround(worldCenter, worldAxis, degreesPerSecond * Time.fixedDeltaTime);
            }
            return;
        }

        _totalAngle += degreesPerSecond * Time.fixedDeltaTime;

        if (_rb != null && transform.parent != null)
        {
            Quaternion parentRot = transform.parent.rotation;
            Vector3 parentPos = transform.parent.position;
            Vector3 worldPivot = parentPos + parentRot * _pivotParentLocal;
            Vector3 worldPos = worldPivot + parentRot * (Quaternion.AngleAxis(_totalAngle, rotateAxis.normalized) * _initialOffsetParentLocal);
            _rb.MovePosition(worldPos);
            _rb.MoveRotation(parentRot * _initialLocalRotation);
        }
        else if (_rb != null)
        {
            _rb.MovePosition(pivot.position + Quaternion.AngleAxis(_totalAngle, rotateAxis.normalized) * _initialOffset);
        }
        else
        {
            transform.position = pivot.position + Quaternion.AngleAxis(_totalAngle, rotateAxis.normalized) * _initialOffset;
        }
    }
}
