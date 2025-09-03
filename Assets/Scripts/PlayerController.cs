using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    private Animator anim;
    private Rigidbody rb;

    private const string WALKANIM = "IsWalk";
    private const string ATTACKANIM = "IsAttack";

    private float moveSpeed = 5f;

    public bool isGround { get; private set; }

    private bool isRotate = false;
    private float rotateSpeed = 10f;
    private float mouseSpeed = 50f;
    Quaternion targetRot;

    Vector3 postUp; // 중력 전환 전 transform.up 방향
    Vector3 snapVec = Vector3.zero;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        // 마우스 숨기고 중앙 고정
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start()
    {
        InputManager.Instance.OnLeftGravity += InputManager_OnLeftGravity;
        InputManager.Instance.OnRightGravity += InputManager_OnRightGravity;
        InputManager.Instance.OnJump += InputManager_OnJump;
    }

    private void InputManager_OnJump(object sender, System.EventArgs e)
    {
        if(!isGround) return;

        isGround = false;
        Vector3 up = transform.up;
        rb.AddForce(up * 5f, ForceMode.Impulse);
    }

    private void InputManager_OnLeftGravity(object sender, System.EventArgs e)
    {
        ChangeGravity(true);
    }
    private void InputManager_OnRightGravity(object sender, System.EventArgs e)
    {
        ChangeGravity(false);
    }

    private void ChangeGravity(bool isLeft)
    {
        isGround = false;
        snapVec = Vector3.zero;

        // 1) 현재 up 축 기준의 yaw 계산 후 스냅
        Vector3 up = transform.up;

        // 기준 forward(월드 forward를 up 평면에 정사영). 만약 거의 평행이면 World right로 대체.
        Vector3 refFwd = Vector3.ProjectOnPlane(Vector3.forward, up);
        if (refFwd.sqrMagnitude < 1e-6f)
            refFwd = Vector3.ProjectOnPlane(Vector3.right, up);
        refFwd.Normalize();

        Vector3 curFwd = Vector3.ProjectOnPlane(transform.forward, up).normalized;

        float yawDeg = Vector3.SignedAngle(refFwd, curFwd, up);
        float snappedYaw = Snap90(yawDeg);
        float yawDelta = snappedYaw - yawDeg;

        // 스냅된 yaw를 적용한 회전
        Quaternion yawSnapRot = Quaternion.AngleAxis(yawDelta, up);
        Quaternion snappedRot = yawSnapRot * transform.rotation;

        // 2) 스냅된 forward를 기준으로 좌/우 90° 측면 방향을 "새 중력 방향"으로 사용
        Vector3 snappedFwd = (yawSnapRot * transform.forward).normalized;
        Quaternion sideRot = Quaternion.AngleAxis(isLeft ? -90f : 90f, up);
        Vector3 gravityDir = (sideRot * snappedFwd).normalized; // 캐릭터의 '옆' 방향

        // 3) 캐릭터의 up을 -gravityDir로 맞추는 회전(즉시/보간 중 택1)
        targetRot = Quaternion.FromToRotation(snappedRot * Vector3.up, -gravityDir) * snappedRot;

        isRotate = true;
    }

    // [-180,180) 구간으로 정규화하여 0/±90/180으로 스냅
    private static float Snap90(float angleDeg)
    {
        float a = Mathf.Repeat(angleDeg + 180f, 360f) - 180f; // [-180,180)
        if (a >= -45f && a < 45f) return 0f;
        if (a >= 45f && a < 135f) return 90f;
        if (a >= 135f || a < -135f) return 180f; // 180과 -180 동일
        return -90f;
    }

    private void FixedUpdate()
    {

        Vector2 InputVector = InputManager.Instance.GetMoveDirNormalized();

        if(InputVector == Vector2.zero)
        {
            anim.SetBool(WALKANIM, false);
        }
        else
        {
            Vector3 moveDir = transform.forward * InputVector.y + transform.right * InputVector.x;
            moveDir.Normalize();

            Vector3 targetPos = rb.position + moveDir * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(targetPos);
            anim.SetBool(WALKANIM, true);
        }

        // 마우스 회전
        /*Vector2 pointerDelta = InputManager.Instance.GetPointerNormalized(); // pointer.x 사용
        if (pointerDelta.sqrMagnitude > 0.0001f && !isRotate)
        {
            float turnAmount = pointerDelta.x * rotateSpeed * Time.fixedDeltaTime;
            Quaternion deltaRot = Quaternion.AngleAxis(turnAmount, transform.up);
            rb.MoveRotation(rb.rotation * deltaRot);
        }*/


        if (isRotate)
        {

            rb.MoveRotation(targetRot); // 즉시 적용
            isRotate = false;
            /*Quaternion newRot = Quaternion.Slerp(rb.rotation, targetRot, rotateSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(newRot);

            if (Quaternion.Angle(rb.rotation, targetRot) < 0.01f)
            {
                rb.MoveRotation(targetRot);
                isRotate = false;
            }*/
        }
    }

    private void Update()
    {
        Vector2 pointerDelta = InputManager.Instance.GetPointerNormalized(); // pointer.x 사용
        if (pointerDelta.sqrMagnitude > 0.01f && !isRotate && isGround)
        {
            transform.Rotate(transform.up, pointerDelta.x * mouseSpeed * Time.deltaTime, Space.World);
        }
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Ground"))
        {
            isGround = true;
        }    
    }

}
