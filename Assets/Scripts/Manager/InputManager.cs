using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set;}

    public event EventHandler OnLeftGravity;
    public event EventHandler OnRightGravity;
    public event EventHandler OnJump;
    public event EventHandler OnKitBoxDrop;
    public event EventHandler OnKitBoxGet;

    public PlayerInput playerInput {  get; private set; }

    private void Awake()
    {
        if(Instance == null) Instance = this;

        playerInput = new PlayerInput();
        playerInput.Player.Enable();

        playerInput.Player.GravityLeft.performed += GravityLeft_performed;
        playerInput.Player.GravityRight.performed += GravityRight_performed;
        playerInput.Player.Jump.performed += Jump_performed;
        playerInput.Player.KitBoxDrop.performed += KitBoxDrop_performed;
        playerInput.Player.KitBoxGet.performed += KitBoxGet_performed;
    }

    private void KitBoxGet_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (InGameManager.Instance.isLevelUp) return;
        OnKitBoxGet.Invoke(this, EventArgs.Empty);
    }

    private void KitBoxDrop_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (InGameManager.Instance.isLevelUp) return;
        OnKitBoxDrop.Invoke(this, EventArgs.Empty);
    }

    private void Jump_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (InGameManager.Instance.isLevelUp) return;
        OnJump?.Invoke(this, EventArgs.Empty);
    }

    private void GravityLeft_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!PlayerController.Instance.isGround || GravityManager.Instance.isGravity) return;
        if (InGameManager.Instance.isLevelUp) return;

        GravityManager.Instance.GravityCheck(true);
        GravityManager.Instance.GravityChange(true);

        OnLeftGravity?.Invoke(this, EventArgs.Empty);
        GravityManager.Instance.GravityCheck(false);
    }

    private void GravityRight_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!PlayerController.Instance.isGround || GravityManager.Instance.isGravity) return;
        if (InGameManager.Instance.isLevelUp) return;

        GravityManager.Instance.GravityCheck(true);
        GravityManager.Instance.GravityChange(false);

        OnRightGravity?.Invoke(this, EventArgs.Empty);
        GravityManager.Instance.GravityCheck(false);
    }

    public Vector2 GetMoveDirNormalized()
    {
        Vector2 dir = playerInput.Player.Move.ReadValue<Vector2>();
        dir = dir.normalized;
        return dir;
    }

    public Vector2 GetPointerNormalized()
    {
        Vector2 dir = playerInput.Player.Look.ReadValue<Vector2>();
        dir = dir.normalized;
        return dir;
    }
}
