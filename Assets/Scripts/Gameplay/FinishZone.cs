using System;
using System.Collections;
using UnityEngine;

public class FinishZone : MonoBehaviour
{
    [SerializeField] private float requiredTime = 1f;

    public event Action OnMarbleFinished;

    private Coroutine _timer;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Marble")) return;
        _timer = StartCoroutine(FinishTimer());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Marble")) return;
        if (_timer != null) StopCoroutine(_timer);
        _timer = null;
    }

    private IEnumerator FinishTimer()
    {
        yield return new WaitForSeconds(requiredTime);
        OnMarbleFinished?.Invoke();
    }
}
