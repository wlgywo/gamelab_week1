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
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private Slider slider;


    // 플레이어 상태 관련. 체력등)
    private int maxHp = 100;
    private int hp = 100;
    private int hpUpgrade = 20;
    private bool isBorder;

    public bool isShopOpen = false;

    private const string WALKANIM = "IsWalk";
    private const string ATTACKANIM1 = "IsAttack1";
    private const string ATTACKANIM2 = "IsAttack2";
    public int damage { get; private set; } = 10;
    private const int upgraeDamage = 10;

    [SerializeField] private float curAttackDelay = 0f;
    private float attackDelay = 0.5f;

    private bool isDamaged = false; // 지금 맞은 상태인가
    private float invincibleTimer = 2f; // 무적 타이머
    private float curInvincibleTimer = 0f;

    public float knockback = 10;
    public const float knockbackUpgrade = 10f;


    private float moveSpeed = 15f;
    public float jumpPower = 15f;

    public bool isGround { get; private set; }

    private bool isRotate = false;
    private float rotateSpeed = 10f;
    private float mouseSpeed = 150f;
    Quaternion targetRot;

    private bool grabKitBox = true; // 현재 박스를 가지고 있는지
    public bool nearKitBox = false; // 현재 박스가 근처에 있는지
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

        renderers = GetComponentsInChildren<Renderer>()
        .Where(r => !(r is TrailRenderer)) // TrailRenderer 제외
        .ToArray();

        // 마우스 숨기고 중앙 고정
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start()
    {
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
            trailRenderer.enabled = true;
            StartCoroutine(AttackReset());
        }
    }

    private IEnumerator AttackReset()
    {
        yield return new WaitForSeconds(attackDelay);
        weapon.SetActive(false);
        trailRenderer.enabled = false;
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

            isBorder = Physics.Raycast(transform.position, moveDir, 1, LayerMask.GetMask("Wall"));
            if (isBorder) return;

            Vector3 targetPos = rb.position + moveDir * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(targetPos);
            anim.SetBool(WALKANIM, true);
        }
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

        if (isShopOpen)
        {
            return;
        }

        Vector2 pointerDelta = InputManager.Instance.GetPointerNormalized(); // pointer.x 사용
        if (pointerDelta.sqrMagnitude > 0.01f && !isRotate)
        {
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


    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Ground"))
        {

            isGround = true;
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

    public void UpdateMaxHp()
    {
        maxHp += hpUpgrade;
        hp = maxHp;
        UpdateVisual();
    }

    public void healHp(int heal)
    {
        hp += heal;
        UpdateVisual();
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
