using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    private Animator anim;
    private Rigidbody rb;

    private const string WALKANIM = "IsWalk";
    private const string ATTACKANIM = "IsAttack";

    public bool isGround { get; private set; }

    private bool isRotate = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        InputManager.Instance.OnLeftGravity += InputManager_OnLeftGravity;
        InputManager.Instance.OnRightGravity += InputManager_OnRightGravity;
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
        // 기준 축 (현재 플레이어 up)
        Vector3 up = transform.up;

        // forward를 기준으로 좌/우 90도 회전
        Quaternion rot = Quaternion.AngleAxis(isLeft ? -90f : 90f, up);
        Vector3 gravityDir = rot * transform.forward;

        // Physics.gravity 변경
        //Physics.gravity = gravityDir.normalized * 9.81f;


        isRotate = true;

        // 플레이어 회전: 현재 up → -중력 방향
        Quaternion targetRot = Quaternion.FromToRotation(transform.up, -gravityDir) * transform.rotation;
        transform.rotation = targetRot;
    }

    private void FixedUpdate()
    {
        if(isRotate)
        {
            //Quaternion newRot = q
        }
    }

}
