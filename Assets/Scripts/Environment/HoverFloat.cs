using UnityEngine;

public class HoverFloat : MonoBehaviour
{
    public float amplitude = 0.3f;
    public float speed = 1.5f;

    private Vector3 _origin;

    void Start()
    {
        _origin = transform.localPosition;
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * speed) * amplitude;
        transform.localPosition = _origin + new Vector3(0f, offset, 0f);
    }
}
