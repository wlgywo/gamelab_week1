using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    // component
    private Animator anim;
    private Rigidbody rb;

    // const
    private const string WALKANIM = "IsWalk";

    // status
    [SerializeField] private float moveSpeed = 10f;
    private bool isGrounded = false;


    private void Awake()
    {
        if(Instance == null) Instance = this;
        
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        InputManager.Instance.OnLeftGravity += (a,b) => ChangeGravity(true);
        InputManager.Instance.OnRightGravity += (a, b) => ChangeGravity(false);
    }

    // 플레이어가 현재 보고 있는 forward방향에서 좌면 -90 우면 90으로 회전을 진행하는 함수
    private void ChangeGravity(bool isLeft)
    {
        float angle = isLeft ? -90f : 90;


        Quaternion rotation = Quaternion.AngleAxis(angle, transform.forward);
        // 2. Rigidbody의 회전 적용
        rb.MoveRotation(rb.rotation * rotation);

    }

    private void FixedUpdate()
    {
        Vector2 moveVec = InputManager.Instance.GetMoveDirNormalized();

        if(moveVec == Vector2.zero)
        {
            anim.SetBool(WALKANIM, false);
        }
        else
        {
            Vector3 dir = transform.forward * moveVec.y + transform.right * moveVec.x; // 현재 보는 방향 기준으로 입력 변경
            dir.Normalize();

            rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);
            anim.SetBool(WALKANIM, true);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if(!isGrounded && GravityManager.Instance.isGravity)
        {
            if (collision.gameObject.CompareTag("Ground"))
            {
                float dot = Vector3.Dot(transform.up.normalized, collision.transform.up.normalized); // 둘의 내적 이용
                if (dot > 0.9f) // 1에 가까우면 둘의 내적이 같은 방향이므로
                {
                    GravityManager.Instance.GravityCheck(false); // 중력 착지 완료
                }
            }
        }
    }
}
