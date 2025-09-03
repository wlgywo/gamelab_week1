using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    private Animator anim;
    private Rigidbody rb;
    private Renderer[] renderers;

    [SerializeField] private GameObject weapon;
    [SerializeField] private Slider slider;


    // 플레이어 상태 관련. 체력등)
    private int maxHp = 100;
    private int hp = 100;
    private int hpUpgrade = 20;

    private const string WALKANIM = "IsWalk";
    private const string ATTACKANIM1 = "IsAttack1";
    private const string ATTACKANIM2 = "IsAttack2";
    public int damage { get; private set; } = 10;
    private const int upgraeDamage = 10;

    [SerializeField] private float curAttackDelay = 0f;
    [SerializeField] private float attackDelay = 1.5f;

    private bool isDamaged = false; // 지금 맞은 상태인가
    private float invincibleTimer = 2f; // 무적 타이머
    private float curInvincibleTimer = 0f;

    public float knockback = 10;
    public const float knockbackUpgrade = 10f;

    private float moveSpeed = 5f;
    public float jumpPower = 15f;

    public bool isGround { get; private set; }

    private bool isRotate = false;
    private float rotateSpeed = 10f;
    private float mouseSpeed = 150f;
    Quaternion targetRot;

    // --- ▼ 아래 두 줄을 추가하세요 ▼ ---
    [Header("Gravity Rotation")]
    public float rotateDuration = 0.5f; // 중력 전환에 걸리는 시간 (초)
    private Coroutine rotationCoroutine; // 실행 중인 회전 코루틴을 저장할 변수
    // --- ▲ 여기까지 추가 ▲ ---

    Vector3 postUp; // 중력 전환 전 transform.up 방향
    Vector3 snapVec = Vector3.zero;

    private bool grabKitBox = true; // 현재 박스를 가지고 있는지
    private bool nearKitBox = false; // 현재 박스가 근처에 있는지
    public GameObject kitBoxObject; // 손에 있는 키트박스
    public Transform kitBoxPos; // 키트박스 떨어질 위치

    // --- 추가된 변수들 ---
    [Header("Camera Control")]
    public Transform cameraTransform; // 인스펙터에서 카메라 Transform을 할당해주세요.
    private float xRotation = 0f; // 카메라의 상하 회전 각도를 저장할 변수
    public float minXAngle = -80f; // 카메라의 최소 상하 회전 각
    public float maxXAngle = 80f; // 카메라의 최대 상하 회전 각

    // --- 아래 코드 추가 ---
    [Header("Camera Collision")]
    public LayerMask obstacleMask; // 장애물로 인식할 레이어
    public Vector3 cameraOffset; // 플레이어로부터 카메라가 떨어져 있을 기본 위치
    private float cameraDistance; // 카메라와 플레이어의 현재 거리
    public float cameraCollisionPadding = 0.2f; // 충돌 시 카메라를 벽에서 살짝 뗄 거리
    public float cameraReturnSpeed = 5f; // 카메라가 원래 위치로 돌아오는 속도


    private void Awake()
    {
        if (Instance == null) Instance = this;
         
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        //renderers = GetComponentsInChildren<Renderer>();

        renderers = GetComponentsInChildren<Renderer>()
        .Where(r => !(r is TrailRenderer)) // TrailRenderer 제외
        .ToArray();

        // 마우스 숨기고 중앙 고정
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start()
    {
        InputManager.Instance.OnLeftGravity += InputManager_OnLeftGravity;
        InputManager.Instance.OnRightGravity += InputManager_OnRightGravity;
        InputManager.Instance.OnJump += InputManager_OnJump;
        InputManager.Instance.OnKitBoxDrop += InputManager_OnKitBoxDrop;
        InputManager.Instance.OnKitBoxGet += InputManager_OnKitBoxGet;
        InputManager.Instance.OnAttack += InputManager_OnAttack;
        UpdateVisual();

        // --- 아래 코드 추가 ---
        // 카메라의 기본 거리를 오프셋의 크기로 설정
        cameraDistance = cameraOffset.magnitude;
        // --- 여기까지 추가 ---
    }

    private void InputManager_OnAttack(object sender, System.EventArgs e)
    {
        if (curAttackDelay < 0f)
        {
            curAttackDelay = attackDelay;

            int num = Random.Range(0, 2);

            if(num == 0) anim.SetTrigger(ATTACKANIM1);
            else anim.SetTrigger(ATTACKANIM2);

            weapon.SetActive(true);
            StartCoroutine(AttackReset());
        }
    }

    private IEnumerator AttackReset()
    {
        yield return new WaitForSeconds(1f);
        weapon.SetActive(false);
    }

    private void InputManager_OnKitBoxGet(object sender, System.EventArgs e)
    {
        if (grabKitBox || !nearKitBox) return;

        Debug.Log("키드 줍기");
        grabKitBox = true;
        InGameManager.Instance.GetKitBox();
        kitBoxObject.SetActive(true);
    }

    private void InputManager_OnKitBoxDrop(object sender, System.EventArgs e)
    {
        if (!grabKitBox) return;

        Debug.Log("키드 놓기");
        grabKitBox = false;
        InGameManager.Instance.DropKitBox();
        kitBoxObject.SetActive(false);
    }

    private void InputManager_OnJump(object sender, System.EventArgs e)
    {
        if(!isGround) return;

        isGround = false;
        Vector3 up = transform.up;
        rb.AddForce(up * jumpPower, ForceMode.Impulse);
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

        // --- ▼ 기존 isRotate = true; 를 아래 코드로 교체하세요 ▼ ---
        // 만약 이전에 실행 중이던 회전 코루틴이 있다면 중지시킵니다.
        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
        }
        // 새로운 목표 각도로 회전하는 코루틴을 시작하고, 변수에 저장합니다.
        rotationCoroutine = StartCoroutine(RotateGravityCoroutine(targetRot));
        // --- ▲ 여기까지 교체 ▲ ---
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
        if(InGameManager.Instance.isLevelUp) return;
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


        
    }

    private void Update()
    {
        curAttackDelay -= Time.deltaTime;


        if (InGameManager.Instance.isLevelUp) return;

        if (isDamaged)
        {
            curInvincibleTimer -= Time.deltaTime;
            if (curInvincibleTimer < 0)
            {
                isDamaged = false;
                curInvincibleTimer = invincibleTimer;
            }
        }

        Vector2 pointerDelta = InputManager.Instance.GetPointerNormalized(); // pointer.x 사용
        if (pointerDelta.sqrMagnitude > 0.01f && !isRotate)
        {
            //transform.Rotate(transform.up, pointerDelta.x * mouseSpeed * Time.deltaTime, Space.World);
            // [수정됨] 마우스 X축으로 플레이어 좌우 회전
            float mouseX = pointerDelta.x * mouseSpeed * Time.deltaTime;
            transform.Rotate(transform.up, mouseX, Space.World);

            // [추가됨] 마우스 Y축으로 카메라 상하 회전
            float mouseY = pointerDelta.y * mouseSpeed * Time.deltaTime;

            // 회전 값을 누적 (마우스를 위로 올릴 때 카메라가 위를 보도록 '-' 사용)
            xRotation -= mouseY;

            // 상하 회전 각도 제한
            xRotation = Mathf.Clamp(xRotation, minXAngle, maxXAngle);

            // 카메라의 로컬 회전 값 적용
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
        
    }

    private IEnumerator RotateGravityCoroutine(Quaternion targetRotation)
    {
        // 회전 시작을 알림 (이 시간 동안 마우스 회전이 멈춤)
        isRotate = true;

        Quaternion startRotation = rb.rotation;
        float elapsedTime = 0f;

        while (elapsedTime < rotateDuration)
        {
            // 경과 시간을 0과 1 사이의 값으로 정규화
            float t = elapsedTime / rotateDuration;

            // (선택사항) SmoothStep을 사용하면 시작과 끝에서 가감속 효과를 주어 더 부드러워 보입니다.
            t = Mathf.SmoothStep(0, 1, t);

            // Slerp를 사용하여 시작 각도와 목표 각도 사이를 부드럽게 보간
            rb.MoveRotation(Quaternion.Slerp(startRotation, targetRotation, t));

            // 경과 시간 업데이트
            elapsedTime += Time.deltaTime;

            // 다음 프레임까지 대기
            yield return null;
        }

        // 회전이 끝난 후, 정확한 목표 각도로 맞춰줌 (오차 보정)
        rb.MoveRotation(targetRotation);

        // 회전이 끝났음을 알림
        isRotate = false;
        rotationCoroutine = null;
    }
    // --- ▲ 여기까지 추가 ▲ ---

    private void LateUpdate()
    {/*
        // 카메라의 회전값(Quaternion)을 플레이어의 회전과 곱하여 최종 회전 방향을 계산
        Quaternion rotation = Quaternion.Euler(xRotation, transform.eulerAngles.y, 0);

        // 1. 카메라가 있어야 할 이상적인 위치 계산
        // 플레이어 위치 + 회전값을 적용한 오프셋
        Vector3 desiredPosition = transform.position + rotation * cameraOffset;

        // 2. 레이캐스트로 충돌 감지
        RaycastHit hit;
        // 플레이어 위치에서 이상적인 카메라 위치 방향으로 광선을 발사
        if (Physics.Raycast(transform.position, desiredPosition - transform.position, out hit, cameraDistance, obstacleMask))
        {
            // 3. 장애물이 감지되면, 충돌 지점에서 약간 앞으로 카메라 위치를 조정
            // hit.point는 광선이 부딪힌 정확한 위치
            // hit.normal은 부딪힌 표면의 법선(수직) 벡터. 카메라를 벽에서 밀어내는 데 사용
            cameraTransform.position = hit.point + hit.normal * cameraCollisionPadding;
        }
        else
        {
            // 4. 장애물이 없으면, 부드럽게(Lerp) 원래의 이상적인 위치로 카메라를 이동
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, desiredPosition, Time.deltaTime * cameraReturnSpeed);
        }

        // 카메라의 회전도 업데이트
        cameraTransform.rotation = rotation; */

        // 1. 플레이어의 로컬 좌표계 기준 cameraOffset을 월드 좌표로 변환하여
        //    카메라가 있어야 할 이상적인 위치를 계산합니다.
        //    transform.TransformPoint()가 이 모든 복잡한 계산을 한 번에 처리해 줍니다.
        Vector3 desiredPosition = transform.TransformPoint(cameraOffset);

        // 레이캐스트의 방향과 거리를 다시 계산
        Vector3 directionToCamera = desiredPosition - transform.position;
        float distanceToCamera = directionToCamera.magnitude;

        // 2. 레이캐스트로 충돌 감지
        RaycastHit hit;
        if (Physics.Raycast(transform.position, directionToCamera.normalized, out hit, distanceToCamera, obstacleMask))
        {
            // 3. 장애물이 감지되면, 충돌 지점에서 약간 앞으로 카메라 위치를 조정
            cameraTransform.position = hit.point + hit.normal * cameraCollisionPadding;
        }
        else
        {
            // 4. 장애물이 없으면, 부드럽게(Lerp) 원래의 이상적인 위치로 카메라를 이동
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, desiredPosition, Time.deltaTime * cameraReturnSpeed);
        }

        // 5. 카메라의 최종 회전값을 계산합니다.
        //    플레이어의 현재 회전(어느 방향이든)에 카메라의 상하 회전(xRotation)을 추가로 적용합니다.
        cameraTransform.rotation = transform.rotation * Quaternion.Euler(xRotation, 0, 0);
    
}


    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Ground"))
        {

            isGround = true;
        }
        else if(collision.gameObject.CompareTag("RepairKit"))
        {
            nearKitBox = true;
        }

    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("RepairKit"))
        {
            nearKitBox = false;
        }

    }

    public void GetDamage(int damage)
    {
        if (isDamaged) return;
        isDamaged = true;
        curInvincibleTimer = invincibleTimer;

        hp -= damage;
        UpdateVisual();
        if (hp <= 0)
        {
            hp = 0;
            InGameManager.Instance.GameOver();
        }
        else
        {
            StartCoroutine(InvincibleBlink());
        }
    }

    public void UpdateKnockback()
    {
        knockback += knockbackUpgrade;
    }

    public void UpdateDamage()
    {
        damage += upgraeDamage;
    }

    public void UpdateHp()
    {
        maxHp += hpUpgrade;
        hp = maxHp;
    }

    private IEnumerator InvincibleBlink()
    {
        float elapsed = 0f;
        while (elapsed < invincibleTimer)
        {
            // 깜빡임 효과 (렌더러 on/off)
            foreach (Renderer r in renderers)
                r.enabled = !r.enabled;

            yield return new WaitForSeconds(0.1f); // 깜빡임 간격
            elapsed += 0.1f;
        }

        // 무적 해제 + 렌더러 복구
        foreach (Renderer r in renderers)
            r.enabled = true;
    }
    private void UpdateVisual()
    {
        slider.value = (float)hp / maxHp;
    }
}
