using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set;}

    public event EventHandler OnLeftGravity;
    public event EventHandler OnRightGravity;
    public event EventHandler OnJump;
    public event EventHandler OnKitBoxDrop;
    public event EventHandler OnKitBoxGet;
    public event EventHandler OnAttack;

    public PlayerInput playerInput {  get; private set; }
    private bool connectGamePad = false;

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
        playerInput.Player.Attack.performed += Attack_performed;
    }

    private void Start()
    {
        foreach (var device in InputSystem.devices)
        {
            if (device is Gamepad)
            {
                //connectGamePad = true;
                ChangeDeviceState(true);
                break;
            }
        }
    }

    private void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void OnDestroy()
    {
        playerInput.Player.GravityLeft.performed -= GravityLeft_performed;
        playerInput.Player.GravityRight.performed -= GravityRight_performed;
        playerInput.Player.Jump.performed -= Jump_performed;
        playerInput.Player.KitBoxDrop.performed -= KitBoxDrop_performed;
        playerInput.Player.KitBoxGet.performed -= KitBoxGet_performed;
        playerInput.Player.Attack.performed -= Attack_performed;

        playerInput.Dispose();
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is Gamepad)
        {
            switch (change)
            {
                case InputDeviceChange.Added:
                    //connectGamePad = true;
                    ChangeDeviceState(true);
                    break;
                case InputDeviceChange.Removed:
                    //connectGamePad = false;
                    ChangeDeviceState(false);
                    break;
            }
        }
    }

    private void ChangeDeviceState(bool isController)
    {
        connectGamePad = isController;
        PostPlayerController.Instance.ChangeSensity(isController);
    }


    private void Attack_performed(InputAction.CallbackContext obj)
    {
        OnAttack?.Invoke(this, EventArgs.Empty);
    }

    private void KitBoxGet_performed(InputAction.CallbackContext obj)
    {
        if (InGameManager.Instance.isLevelUp) return;
        OnKitBoxGet.Invoke(this, EventArgs.Empty);
    }

    private void KitBoxDrop_performed(InputAction.CallbackContext obj)
    {
        if (InGameManager.Instance.isLevelUp) return;
        OnKitBoxDrop.Invoke(this, EventArgs.Empty);
    }

    private void Jump_performed(InputAction.CallbackContext obj)
    {
        if (InGameManager.Instance.isLevelUp) return;
        OnJump?.Invoke(this, EventArgs.Empty);
    }

    private void GravityLeft_performed(InputAction.CallbackContext obj)
    {
        //if (!PostPlayerController.Instance.isGround || GravityManager.Instance.isGravity) return;
        //if (InGameManager.Instance.isLevelUp) return;

        if (GravityManager.Instance.isGravity || !PlayerController.Instance.GravityReady()) return;

        // GravityManager.Instance.GravityCheck(true);
        GravityManager.Instance.GravityChange(true);

        OnLeftGravity?.Invoke(this, EventArgs.Empty);
        //GravityManager.Instance.GravityCheck(false);
    }

    private void GravityRight_performed(InputAction.CallbackContext obj)
    {
        //if (!PostPlayerController.Instance.isGround || GravityManager.Instance.isGravity) return;
        if (GravityManager.Instance.isGravity || !PlayerController.Instance.GravityReady()) return;

        //GravityManager.Instance.GravityCheck(true);
        GravityManager.Instance.GravityChange(false);

        OnRightGravity?.Invoke(this, EventArgs.Empty);
        //GravityManager.Instance.GravityCheck(false);
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
