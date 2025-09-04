using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public event EventHandler OnLeftGravity;
    public event EventHandler OnRightGravity;
    public event EventHandler OnJump;
    public event EventHandler OnKitBoxDrop;
    public event EventHandler OnKitBoxGet;
    public event EventHandler OnAttack;

    [Header("Axes (Old Input)")]
    [SerializeField] private string horizontalAxis = "Horizontal";
    [SerializeField] private string verticalAxis = "Vertical";
    [SerializeField] private string mouseXAxis = "Mouse X";
    [SerializeField] private string mouseYAxis = "Mouse Y";

    [Header("Action Keys")]
    private KeyCode gravityLeftKey = KeyCode.Q;   // <— 필요시 인스펙터에서 변경
    private KeyCode gravityRightKey = KeyCode.E;
    private KeyCode jumpKey = KeyCode.Space;
    private KeyCode kitBoxDropKey = KeyCode.G;
    private KeyCode kitBoxGetKey = KeyCode.F;
    private KeyCode attackKey = KeyCode.Mouse0;      // 좌클릭

    [Header("Tuning")]
    [Range(0f, 1f)] public float moveDeadzone = 0.15f; // 미세 오입력 방지
    public bool useMouseDeltaForLook = true;           // true: Mouse X/Y, false: 화면좌표 정규화

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(gameObject); return; }
    }

    private void Update()
    {
        // 중력 좌
        if (Input.GetKeyDown(gravityLeftKey))
        {
            if (PlayerController.Instance.isGround &&
                !GravityManager.Instance.isGravity &&
                !InGameManager.Instance.isLevelUp)
            {
                GravityManager.Instance.GravityCheck(true);
                GravityManager.Instance.GravityChange(true);  // left
                OnLeftGravity?.Invoke(this, EventArgs.Empty);
                GravityManager.Instance.GravityCheck(false);
            }
        }

        // 중력 우
        if (Input.GetKeyDown(gravityRightKey))
        {
            if (PlayerController.Instance.isGround &&
                !GravityManager.Instance.isGravity &&
                !InGameManager.Instance.isLevelUp)
            {
                GravityManager.Instance.GravityCheck(true);
                GravityManager.Instance.GravityChange(false); // right
                OnRightGravity?.Invoke(this, EventArgs.Empty);
                GravityManager.Instance.GravityCheck(false);
            }
        }

        // 점프
        if (Input.GetKeyDown(jumpKey))
        {
            if (!InGameManager.Instance.isLevelUp)
                OnJump?.Invoke(this, EventArgs.Empty);
        }

        // 키트 드랍
        if (Input.GetKeyDown(kitBoxDropKey))
        {
            if (!InGameManager.Instance.isLevelUp)
                OnKitBoxDrop?.Invoke(this, EventArgs.Empty);
        }

        // 키트 줍기
        if (Input.GetKeyDown(kitBoxGetKey))
        {
            if (!InGameManager.Instance.isLevelUp)
                OnKitBoxGet?.Invoke(this, EventArgs.Empty);
        }

        // 공격
        if (Input.GetKeyDown(attackKey))
        {
            OnAttack?.Invoke(this, EventArgs.Empty);
        }
    }

    // 이동 입력(정규화)
    public Vector2 GetMoveDirNormalized()
    {
        float x = Input.GetAxisRaw(horizontalAxis);
        float y = Input.GetAxisRaw(verticalAxis);
        Vector2 dir = new Vector2(x, y);
        if (dir.sqrMagnitude < moveDeadzone * moveDeadzone) return Vector2.zero;
        return dir.normalized;
    }

    // 시점/포인터(정규화)
    public Vector2 GetPointerNormalized()
    {
        if (useMouseDeltaForLook)
        {
            // 마우스 델타 기반(에임/카메라 회전에 적합)
            float mx = Input.GetAxis(mouseXAxis);
            float my = Input.GetAxis(mouseYAxis);
            Vector2 delta = new Vector2(mx, my);
            if (delta.sqrMagnitude < Mathf.Epsilon) return Vector2.zero;
            return delta.normalized;
        }
        else
        {
            // 화면 좌표 기반(0~1 정규화)
            Vector2 pos = Input.mousePosition;
            return new Vector2(
                Mathf.Clamp01(pos.x / Screen.width),
                Mathf.Clamp01(pos.y / Screen.height)
            );
        }
    }
}
