using System;
using UnityEngine;
using UnityEngine.UI;

public class BossAI : MonoBehaviour
{
    public static BossAI Instance { get; private set; }
    [SerializeField] protected Rigidbody rb;


    // 다른 오브젝트 관련
    public Transform player;
    [SerializeField] Slider slider;

    // 보스 상태 관련
    public bool onFloor = false;

    // 보스 능력치 관련
    private int hp = 300;
    private int maxHp = 300;
    public float speed = 5.0f;
	public float moveRotationSpeed = 10.0f;
	public int damage = 10;

	// 데미지와 사망 관련
	private bool isDie = false;
	private bool isDamaged = false; // 지금 맞은 상태인가


	// 경계 설정용
	public float stopRadius = 9.9f;     // 어느 반경에서 멈출지
	private float stopEpsilon = 0.05f;   // 경계에서 떨림 방지용


	// 공격 관련
	private bool isAttacking = false;
	public float attackRange = 10f;       // 공격 사거리
	private float attackCooldown = 2f;         // 공격 쿨타임
	private float attackDelay = 3f;
	Action[] skills;

    [Header("발사 설정")]
    public float spawnBulletDelay = 0.5f;
    public int bulletDamage = 10;
    public GameObject bulletPrefab;  // 발사할 총알의 프리팹
    public Transform firePoint;      // 총알이 발사될 위치 (보스의 손)

    [Header("연사 설정")]
    public int bulletsPerBurst = 1;         // 한 번에 발사할 총알 수
    public float timeBetweenShots = 0.05f;  // 각 총알 사이의 발사 간격
    public float timeBetweenBursts = 2f;    // 다음 3발 발사까지의 대기 시간

    private bool isFiring = false; // 현재 발사 중인지 확인하는 변수

    [Header("Gravity Settings")]
    public float gravityForce = 9.8f; // 적용할 중력의 크기
    public float orientationSpeed = 10f; // 플레이어의 'up' 방향을 따라가는 회전 속도

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // Rigidbody의 기본 중력은 꺼야 수동으로 제어할 수 있습니다.
        rb.useGravity = false;
    }

    private void Start()
    {
		skills = new Action[] { ThrowFileSkile };
        UpdateVisual();
    }

    Quaternion SnapRotation90(Quaternion rot)
    {
        Vector3 euler = rot.eulerAngles;

        euler.x = Mathf.Round(euler.x / 90f) * 90f;
        euler.y = Mathf.Round(euler.y / 90f) * 90f;
        euler.z = Mathf.Round(euler.z / 90f) * 90f;

        return Quaternion.Euler(euler);
    }


    private void ThrowFileSkile()
	{
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }


    private void FixedUpdate()
    {
        if (isDie) return;

        // --- 1. 중력 및 방향 동기화 (기존과 동일) ---
        if (player != null)
        {
            Vector3 gravityDirection = -player.transform.up;
            rb.AddForce(gravityDirection * gravityForce);

            Quaternion targetOrientation = Quaternion.FromToRotation(transform.up, player.transform.up) * rb.rotation;
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetOrientation, orientationSpeed * Time.fixedDeltaTime));
        }

        if (player == null) return;
        if (isAttacking) return;

        // --- 2. 필요한 모든 변수를 한 번만 계산 (로직 통합) ---
        Vector3 selfPos = transform.position;
        Vector3 targetPos = player.position;
        Vector3 upDir = transform.up; // 현재 나의 '위' 방향

        // '위' 방향을 무시한 평면상의 거리와 방향을 계산
        float centerDistance = Vector3.Distance(Vector3.ProjectOnPlane(selfPos, upDir), Vector3.ProjectOnPlane(targetPos, upDir));
        Vector3 flatDirToTarget = Vector3.ProjectOnPlane(targetPos - selfPos, upDir).normalized;


        // --- 3. 거리에 따라 행동 결정 (이동 또는 정지/공격) ---

        // 플레이어가 멈춤 반경(stopRadius) 밖에 있다면 -> 이동
        if (centerDistance > stopRadius)
        {
            // 목표 지점은 타겟 위치에서 멈춤 반경만큼 떨어진 곳
            Vector3 targetOnBoundary = targetPos - flatDirToTarget * stopRadius;

            // 이동할 방향 계산 및 이동
            Vector3 toTarget = Vector3.ProjectOnPlane(targetOnBoundary - selfPos, upDir);
            Vector3 nextPos = rb.position + toTarget.normalized * speed * Time.fixedDeltaTime;
            rb.MovePosition(nextPos);

            // 이동 방향으로 회전
            if (toTarget.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(toTarget.normalized, upDir);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, moveRotationSpeed * Time.fixedDeltaTime));
            }
        }
        // 플레이어가 멈춤 반경(stopRadius) 안에 있다면 -> 정지 & 공격 시도
        else
        {
            // 이동을 멈춤
            rb.MovePosition(rb.position);
            rb.linearVelocity = Vector3.Project(rb.linearVelocity, upDir); // 평면 이동 관성 제거

            // 플레이어를 바라보도록 회전
            if (flatDirToTarget.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(flatDirToTarget, upDir);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, moveRotationSpeed * Time.fixedDeltaTime));
            }

            // 이 상태에서 공격 쿨타임이 다 되었다면 공격
            // onFloor 같은 추가 조건이 있다면 여기에 추가
            if (attackCooldown <= 0 /*&& onFloor*/)
            {
                attackCooldown = attackDelay;
                Attack();
                // Attack() 후에 return할 필요 없음. 이미 이동은 멈춘 상태.
            }
        }
    }

    private void Update()
	{
		attackCooldown -= Time.deltaTime;
    }

	private void Attack()
	{
		if(!isAttacking && !isDie)
		{
            skills[0].Invoke();
		}
	}

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController.Instance.GetDamage(damage);
        }
    }

    private void OnTriggerEnter(Collider other)
	{
        if (other.CompareTag("Weapon"))
        {
            GetDamage();
        }
    }

	public void GetDamage()
	{
		hp -= PlayerController.Instance.damage;
        UpdateVisual();
        if (hp <= 0)
        {
            Destroy(gameObject);
            InGameManager.Instance.GameClear();
        }
	}

	public void SetAttackDamage(int attackDamage)
	{
		damage += attackDamage;
	}

	public void Heal(int heal)
	{
		hp += heal;
		// 보스 Hp UI작업 필요.
	}
    private void UpdateVisual()
    {
        slider.value = (float)hp / maxHp;
    }
}
