using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set;}

    public event EventHandler OnLeftGravity;
    public event EventHandler OnRightGravity;
    public event EventHandler OnJump; 

    public PlayerInput playerInput {  get; private set; }

    private void Awake()
    {
        if(Instance == null) Instance = this;

        playerInput = new PlayerInput();
        playerInput.Player.Enable();

        playerInput.Player.GravityLeft.performed += GravityLeft_performed;
        playerInput.Player.GravityRight.performed += GravityRight_performed;
        playerInput.Player.Jump.performed += Jump_performed;
    }

    private void Jump_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnJump?.Invoke(this, EventArgs.Empty);
    }

    private void GravityLeft_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!PlayerController.Instance.isGround) return;

        GravityManager.Instance.GravityChange(true);

        OnLeftGravity?.Invoke(this, EventArgs.Empty);
    }

    private void GravityRight_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!PlayerController.Instance.isGround) return;

        GravityManager.Instance.GravityChange(false);

        OnRightGravity?.Invoke(this, EventArgs.Empty);
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
