using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    private Animator anim;
    private Rigidbody rb;

    private void Awake()
    {
        if(Instance == null) Instance = this;

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
}
