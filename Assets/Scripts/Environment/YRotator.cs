using UnityEngine;

public class YRotator : MonoBehaviour
{
    [SerializeField] float _speed = 90f;

    void Update()
    {
        transform.Rotate(0f, _speed * Time.deltaTime, 0f, Space.Self);
    }
}
