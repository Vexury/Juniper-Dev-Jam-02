using UnityEngine;

public class FollowRotation : MonoBehaviour
{
    [SerializeField] private Rigidbody target;

    private Rigidbody _rb;

    private void Awake() => _rb = GetComponent<Rigidbody>();

    private void FixedUpdate() => _rb.MoveRotation(target.rotation);
}
