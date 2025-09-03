using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class BossAI : MonoBehaviour
{
    public static BossAI Instance { get; private set; }
    [SerializeField] protected Rigidbody rb;


    // 다른 오브젝트 관련
    public Transform player;
	int zoneCount = 3;
	[SerializeField] GameObject RedZone;
	[SerializeField] GameObject BlueZone;
	[SerializeField] GameObject GreenZone;

    // 보스 능력치 관련
    public int hp = 1000;
	public float speed = 5.0f;
	public float moveRotationSpeed = 10.0f;
	public int damage = 15;

	// 데미지와 사망 관련
	private bool isDie = false;
	private bool isDamaged = false; // 지금 맞은 상태인가


	// 경계 설정용
	private float stopRadius = 3.9f;     // 어느 반경에서 멈출지
	private float stopEpsilon = 0.05f;   // 경계에서 떨림 방지용


	// 공격 관련
	private bool isAttacking = false;
	private float attackRange = 4f;       // 공격 사거리
	private float attackCooldown;         // 공격 쿨타임
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
		skills = new Action[] { AoESkill, ThrowFileSkile };
    }

	private void AoESkill()
	{
		int num = UnityEngine.Random.Range(0, zoneCount);
		switch (num)
		{
			case 0:
				{
					Instantiate(RedZone, transform.position, SnapRotation90(transform.rotation));
					break;
				}
            case 1:
                {
                    Instantiate(BlueZone, transform.position, SnapRotation90(transform.rotation));
                    break;
                }
            case 2:
                {
                    Instantiate(GreenZone, transform.position, SnapRotation90(transform.rotation));
                    break;
                }
        }
	}

	Quaternion SnapRotation90(Quaternion rot)
	{
		Vector3 euler = rot.eulerAngles;

		euler.x = Mathf.Round(euler.x/90f) * 90f;
		euler.y = Mathf.Round(euler.y/90f) * 90f;
		euler.z = Mathf.Round(euler.z/90f) * 90f;

		return Quaternion.Euler(euler);
	}

	private void ThrowFileSkile()
	{
		Instantiate(bulletPrefab, transform.position + transform.forward * 2f + Vector3.up, Quaternion.identity);
    }


    private void FixedUpdate()
	{
		if (isDie) return;
        //if (player == null && repairKit == null) return;
        // --- ▼ [수정 1] 중력 및 방향 동기화 로직 추가 ▼ ---
        if (player != null)
        {
            // 1. 플레이어와 같은 방향으로 중력 적용
            Vector3 gravityDirection = -player.transform.up;
            rb.AddForce(gravityDirection * gravityForce);

            // 2. 플레이어의 'up' 벡터를 부드럽게 따라가도록 회전
            Quaternion targetOrientation = Quaternion.FromToRotation(transform.up, player.transform.up) * rb.rotation;
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetOrientation, orientationSpeed * Time.fixedDeltaTime));
        }
        // --- ▲ [수정 1] 종료 ▲ ---

        if (player == null) return;
		if (isAttacking) return;
		
        float centerDistance = Vector3.Distance(transform.position, player.position);

		if(attackCooldown < 0 && centerDistance <= attackRange) {
			attackCooldown = attackDelay;
			Attack();
			return;
		}

        // --- ▼ [수정 2] 거리 및 방향 계산 로직 변경 ▼ ---
        Vector3 selfPos = transform.position;
        Transform target = player;
        Vector3 targetPos = target.position;
        Vector3 upDir = transform.up; // 현재 나의 '위' 방향

        // '위' 방향을 무시한 평면상의 거리 계산
        centerDistance = Vector3.Distance(Vector3.ProjectOnPlane(selfPos, upDir), Vector3.ProjectOnPlane(targetPos, upDir));

        if (attackCooldown < 0 && centerDistance <= attackRange)
        {
            attackCooldown = attackDelay;
            Attack();
            return;
        }

        // '위' 방향을 무시하고 타겟을 향하는 방향 벡터 계산
        Vector3 dirToTarget = (targetPos - selfPos);
        Vector3 flatDirToTarget = Vector3.ProjectOnPlane(dirToTarget, upDir).normalized;

        if (centerDistance > stopRadius + stopEpsilon)
        {
            // 목표 지점은 타겟 위치에서 멈춤 반경만큼 떨어진 곳
            Vector3 targetOnBoundary = targetPos - flatDirToTarget * stopRadius;

            // 이동할 방향 계산
            Vector3 toTarget = Vector3.ProjectOnPlane(targetOnBoundary - selfPos, upDir);
            Vector3 nextPos = rb.position + toTarget.normalized * speed * Time.fixedDeltaTime;
            rb.MovePosition(nextPos);

            // '위' 방향을 기준으로 회전
            Quaternion targetRot = Quaternion.LookRotation(flatDirToTarget, upDir);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, moveRotationSpeed * Time.fixedDeltaTime));
        }
        else
        {
            // 멈출 때
            rb.MovePosition(rb.position);

            // 관성 제거 시, 현재 평면에 대해서만 제거하는 것이 더 안정적일 수 있습니다.
            rb.linearVelocity = Vector3.Project(rb.linearVelocity, upDir);

            if (flatDirToTarget.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(flatDirToTarget, upDir);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, moveRotationSpeed * Time.fixedDeltaTime));
            }
        }
        // --- ▲ [수정 2] 종료 ▲ ---


    }

    private void Update()
	{
		attackCooldown -= Time.deltaTime;

        if (!isFiring)
        {
            StartCoroutine(FireBurst());
        }
    }

	private void Attack()
	{
		if(!isAttacking && !isDie)
		{
			int idx = UnityEngine.Random.Range(0, skills.Length);
			skills[idx].Invoke();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerController.Instance.GetDamage(damage);
        }
    }

	public void GetDamage(int damage)
	{
		hp -= damage;
		{
			if(hp < 0)
			{
				// 보스가 죽었으니 게임 종료
			}
			else
			{
				// UI로 띄워야함.
			}
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

    IEnumerator FireBurst()
    {
        isFiring = true; // 발사 시작

        // 1. 3발을 0.05초 간격으로 발사
        for (int i = 0; i < bulletsPerBurst; i++)
        {
            // 총알 생성: (무엇을, 어디에, 어떤 방향으로)
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            // 다음 총알을 발사하기 전까지 0.05초 대기
            yield return new WaitForSeconds(timeBetweenShots);
        }
		Debug.Log("3발 발사 완료");
        // 2. 다음 연사까지 대기
        yield return new WaitForSeconds(timeBetweenBursts);
		Debug.Log("공격종료");
        isFiring = false; // 발사 완료, 다음 발사 준비
    }
}
