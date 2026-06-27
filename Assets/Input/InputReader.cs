using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static Controls;

[CreateAssetMenu(fileName = "New InputReader", menuName = "Input/InputReader")]
public class InputReader : ScriptableObject, IPlayerActions, IUIActions
{
    private Controls controls;

    public event Action<Vector2> MoveEvent;
    public event Action<Vector2> LookEvent;
    public event Action<bool> PrimaryActionEvent;
    public event Action<bool> SecondaryActionEvent;
    public event Action InteractEvent;
    public event Action<bool> CrouchEvent;
    public event Action<bool> JumpEvent;
    public event Action PreviousEvent;
    public event Action NextEvent;
    public event Action<bool> SprintEvent;
    public event Action PauseEvent;
    public event Action<bool> UnlockCursorEvent;
    public event Action<float> RollEvent;
    public event Action<float> ZoomEvent;
    public event Action ReloadEvent;

    private void OnEnable()
    {
        if (controls == null)
        {
            controls = new Controls();
            controls.Player.SetCallbacks(this);
            controls.UI.SetCallbacks(this);
        }

        EnableGameplayInput();
    }

    private void OnDisable()
    {
        if (controls != null)
        {
            controls.Player.Disable();
            controls.UI.Disable();
        }
    }

    public void EnableGameplayInput()
    {
        controls.Player.Enable();
        controls.UI.Disable();
    }

    public void EnableUIInput()
    {
        controls.Player.Disable();
        controls.UI.Enable();
    }

    public void DisableAllInput()
    {
        controls.Player.Disable();
        controls.UI.Disable();
    }

    #region Player

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        LookEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnPrimaryAction(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            PrimaryActionEvent?.Invoke(true);
        }
        else if (context.canceled)
        {
            PrimaryActionEvent?.Invoke(false);
        }
    }

    public void OnSecondaryAction(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SecondaryActionEvent?.Invoke(true);
        }
        else if (context.canceled)
        {
            SecondaryActionEvent?.Invoke(false);
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            InteractEvent?.Invoke();
        }
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            CrouchEvent?.Invoke(true);
        }
        else if (context.canceled)
        {
            CrouchEvent?.Invoke(false);
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            JumpEvent?.Invoke(true);
        }
        else if (context.canceled)
        {
            JumpEvent?.Invoke(false);
        }
    }

    public void OnPrevious(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            PreviousEvent?.Invoke();
        }
    }

    public void OnNext(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            NextEvent?.Invoke();
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SprintEvent?.Invoke(true);
        }
        else if (context.canceled)
        {
            SprintEvent?.Invoke(false);
        }
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            PauseEvent?.Invoke();
        }
    }

    public void OnUnlockCursor(InputAction.CallbackContext context)
    {
        if (context.performed)
            UnlockCursorEvent?.Invoke(true);
        else if (context.canceled)
            UnlockCursorEvent?.Invoke(false);
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        RollEvent?.Invoke(context.ReadValue<float>());
    }

    public void OnScroll(InputAction.CallbackContext context)
    {
        ZoomEvent?.Invoke(context.ReadValue<Vector2>().y);
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (context.performed)
            ReloadEvent?.Invoke();
    }

    public void OnSecondaryInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
            PauseEvent?.Invoke();
    }

    #endregion

    #region UI

    public void OnNavigate(InputAction.CallbackContext context)
    {

    }

    public void OnSubmit(InputAction.CallbackContext context)
    {

    }

    public void OnCancel(InputAction.CallbackContext context)
    {

    }

    public void OnPoint(InputAction.CallbackContext context)
    {

    }

    public void OnClick(InputAction.CallbackContext context)
    {

    }

    public void OnRightClick(InputAction.CallbackContext context)
    {

    }

    public void OnMiddleClick(InputAction.CallbackContext context)
    {

    }

    public void OnScrollWheel(InputAction.CallbackContext context)
    {

    }

    public void OnTrackedDevicePosition(InputAction.CallbackContext context)
    {

    }

    public void OnTrackedDeviceOrientation(InputAction.CallbackContext context)
    {

    }

    #endregion

}
