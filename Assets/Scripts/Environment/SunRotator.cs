using UnityEngine;

public class SunRotator : MonoBehaviour
{
    [SerializeField] private float degreesPerSecond = 10f;

    private void Update()
    {
        transform.Rotate(0f, degreesPerSecond * Time.deltaTime, 0f, Space.World);
    }
}
