using System.Collections;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    // component
    private Animator anim;
    private Rigidbody rb;
    private Renderer[] renderers;

    // const
    private const string WALKANIM = "IsWalk";
    private const string ATTACKANIM1 = "IsAttack1";
    private const string ATTACKANIM2 = "IsAttack2";

    private const string ATTACKSPEED = "AttackSpeed";


    [Header("Gravity Rotation")]
    public float rotateDuration = 0.5f; // 중력 전환에 걸리는 시간 (초)
    private Coroutine rotationCoroutine; // 실행 중인 회전 코루틴을 저장할 변수
    Quaternion targetRot;

    [Header("Camera Control")]
    public Transform cameraTransform; // 인스펙터에서 카메라 Transform을 할당해주세요.
    private float xRotation = 0f; // 카메라의 상하 회전 각도를 저장할 변수
    public float minXAngle = -80f; // 카메라의 최소 상하 회전 각
    public float maxXAngle = 20f; // 카메라의 최대 상하 회전 각
    private float cameraXSpeed = 200f;
    private float cameraYSpeed = 120f;
    private int wallCounter= 0; // 현재 벽 카운트

    [Header("Camera Collision")]
    public LayerMask obstacleMask; // 장애물로 인식할 레이어
    public Vector3 cameraOffset; // 플레이어로부터 카메라가 떨어져 있을 기본 위치
    public float cameraCollisionPadding = 0.2f; // 충돌 시 카메라를 벽에서 살짝 뗄 거리
    public float cameraReturnSpeed = 5f; // 카메라가 원래 위치로 돌아오는 속도

    [Header("Weapon")]
    [SerializeField] private GameObject weapon;
    [SerializeField] private TrailRenderer trailRenderer;

    [Header("Status")]
    [SerializeField] private float curAttackDelay = 0f;
    private float invincibleTimer = 2f; // 무적 타이머

    //public int damage { get; private set; } = 10;

    [Header("State")]
    public MapDirect mapDirect;
    private bool isRotate = false;
    private bool isGrounded = false;
    private bool isDamaged = false;
    private bool isBorder;
    //private float gravityTimer = 0f;



    private void Awake()
    {
        if(Instance == null) Instance = this;
        
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        renderers = GetComponentsInChildren<Renderer>()
       .Where(r => !(r is TrailRenderer)) // TrailRenderer 제외
       .ToArray();

        // 마우스 숨기고 중앙 고정
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start()
    {
        InputManager.Instance.OnLeftGravity += (a,b) => ChangeGravity(true);
        InputManager.Instance.OnRightGravity += (a, b) => ChangeGravity(false);
        InputManager.Instance.OnAttack += InputManager_OnAttack;
    }

    // 플레이어가 현재 보고 있는 forward방향에서 좌면 -90 우면 90으로 회전을 진행하는 함수
    /*private void ChangeGravity(bool isLeft)
    {
        isRotate = true;
        float angle = isLeft ? -90f : 90;

        Quaternion rotation = Quaternion.AngleAxis(angle, transform.forward);
        // 2. Rigidbody의 회전 적용
        rb.MoveRotation(rb.rotation * rotation);
        isRotate = false;
    }*/

    private void InputManager_OnAttack(object sender, System.EventArgs e)
    {
        if (curAttackDelay < 0f)
        {
            curAttackDelay = InGameManager.Instance.attackSpeed;

            int num = Random.Range(0, 2);

            if (num == 0) anim.SetTrigger(ATTACKANIM1);
            else anim.SetTrigger(ATTACKANIM2);

            weapon.SetActive(true);
            trailRenderer.enabled = true;
            StartCoroutine(AttackReset());
        }
    }
    private IEnumerator AttackReset()
    {
        yield return new WaitForSeconds(InGameManager.Instance.attackSpeed);
        weapon.SetActive(false);
        trailRenderer.enabled = false;
    }

    private void ChangeGravity(bool isLeft)
    {
        //gravityTimer = InGameManager.Instance.gravityTimer;

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
    private static float Snap90(float angleDeg)
    {
        float a = Mathf.Repeat(angleDeg + 180f, 360f) - 180f; // [-180,180)
        if (a >= -45f && a < 45f) return 0f;
        if (a >= 45f && a < 135f) return 90f;
        if (a >= 135f || a < -135f) return 180f; // 180과 -180 동일
        return -90f;
    }

    private IEnumerator RotateGravityCoroutine(Quaternion targetRotation)
    {
        // 회전 시작을 알림 (이 시간 동안 마우스 회전이 멈춤)
        isRotate = true;
        isGrounded = false;

        yield return null; // 한프레임 대기

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

            isBorder = Physics.Raycast(transform.position, dir, 1, LayerMask.GetMask("Wall"));
            if (isBorder) return;

            rb.MovePosition(rb.position + dir * InGameManager.Instance.moveSpeed * Time.fixedDeltaTime);
            anim.SetBool(WALKANIM, true);
        }
    }

    private void Update()
    {
        curAttackDelay -= Time.deltaTime;
        //gravityTimer -= Time.deltaTime;

        // IngameManager에서 1- 현재 남은값으로 처리
        //InGameManager.Instance.UpdateVisual(StatusType.gravity, gravityTimer / InGameManager.Instance.gravityTimer);

        Vector2 pointerDelta = InputManager.Instance.GetPointerNormalized(); // pointer.x 사용
        if (pointerDelta.sqrMagnitude > 0.01f && !isRotate)
        {
            //transform.Rotate(transform.up, pointerDelta.x * mouseSpeed * Time.deltaTime, Space.World);
            // [수정됨] 마우스 X축으로 플레이어 좌우 회전
            float mouseX = pointerDelta.x * cameraXSpeed * Time.deltaTime;
            transform.Rotate(transform.up, mouseX, Space.World);

            // [추가됨] 마우스 Y축으로 카메라 상하 회전
            float mouseY = pointerDelta.y * cameraYSpeed * Time.deltaTime;

            // 회전 값을 누적 (마우스를 위로 올릴 때 카메라가 위를 보도록 '-' 사용)
            xRotation -= mouseY;

            // 상하 회전 각도 제한
            xRotation = Mathf.Clamp(xRotation, minXAngle, maxXAngle);

            // 카메라의 로컬 회전 값 적용
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
    }

    private void LateUpdate()
    {
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

    public bool GravityReady()
    {
        return isGrounded && wallCounter == 1 && InGameManager.Instance.GravityCoolTime;
    }

    public void GetDamage(int damage)
    {
        if (isDamaged) return;
        isDamaged = true;

        InGameManager.Instance.GetDamage(damage);
        //hp -= damage; 
        //InGameManager.Instance.UpdateVisual(StatusType.hp, (float)hp / maxHp);

        StartCoroutine(invincibleTime());
    }

    private IEnumerator invincibleTime()
    {
        float elapsed = 0f;
        while (elapsed < invincibleTimer)
        {
            // 깜빡임 효과 (렌더러 on/off)
            foreach (Renderer r in renderers)
                r.enabled = !r.enabled;

            yield return new WaitForSeconds(0.3f); // 깜빡임 간격
            elapsed += 0.3f;
        }

        // 무적 해제 + 렌더러 복구
        foreach (Renderer r in renderers)
            r.enabled = true;

        isDamaged = false;
    }

    /*public void Heal()
    {
        hp += InGameManager.Instance.healValue;
        if(hp > maxHp) hp = maxHp;

        InGameManager.Instance.UpdateVisual(StatusType.hp, (float)hp/ maxHp);
    }*/

    /*private void UpdateVisual()
    {
        slider.value = (float)hp / maxHp;
    }*/

    public void SetAttackAnim(float speed)
    {
        anim.SetFloat(ATTACKSPEED, 1.15f - speed);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            wallCounter++;

            float dot = Vector3.Dot(transform.up.normalized, collision.transform.up.normalized); // 둘의 내적 이용
            if (dot > 0.9f)
            {
                mapDirect = collision.gameObject.GetComponentInChildren<Spawner>().mapDirect;
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if(!isGrounded && !isRotate) // 공중에서 회전중이 아닐때
        {
            if (collision.gameObject.CompareTag("Ground")) // 땅에 닿았을 때
            {
                // 둘이 같은 방향이라면
                float dot = Vector3.Dot(transform.up.normalized, collision.transform.up.normalized); // 둘의 내적 이용
                if (dot > 0.9f) // 1에 가까우면 둘의 내적이 같은 방향이므로
                {
                    if (GravityManager.Instance.isGravity) // 중력을 사용중이고 회전이 끝난 상태라면
                    {
                        GravityManager.Instance.GravityCheck(false);
                    }
                    isGrounded = true; // 착지로 변환

                }
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground")) wallCounter--;
    }
}
