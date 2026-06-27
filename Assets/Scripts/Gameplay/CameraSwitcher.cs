using Unity.Cinemachine;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    [SerializeField] private InputReader inputReader;
    [SerializeField] private CinemachineCamera closeCam;
    [SerializeField] private SphereRotator sphereRotator;
    [SerializeField] private MarbleController marbleController;

    public bool IsInsideMode => _isClose;

    private bool _isClose;

    private void OnEnable()
    {
        inputReader.SecondaryActionEvent += OnSecondaryAction;
    }

    private void OnDisable()
    {
        inputReader.SecondaryActionEvent -= OnSecondaryAction;
    }

    private void OnSecondaryAction(bool performed)
    {
        if (!performed) return;
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.State.Playing) return;
        if (sphereRotator.IsWobbling) return;
        SetClose(!_isClose);
    }

    public void SwitchToOutside()
    {
        SetClose(false);
    }

    private void SetClose(bool isClose)
    {
        _isClose = isClose;
        closeCam.enabled = _isClose;
        sphereRotator.enabled = !_isClose;
        marbleController.enabled = _isClose;
    }
}
