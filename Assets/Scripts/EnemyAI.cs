using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnemyAI : MonoBehaviour
{
	[SerializeField] protected Rigidbody rb;
	[SerializeField] private Slider slider;

	// 다른 오브젝트 관련
	public Transform repairKit;
	public Transform player;

	// 몬스터 상태 관련
	public int hp = 100;
	public int maxHp = 100;
	public float speed = 5.0f;
	public float moveRotationSpeed = 10.0f;
	public int damage = 5;

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
    private float forwardImpulse = 50f;     // 전진(박치기) 임펄스
    private float backwardImpulse = 60f;    // 복귀 임펄스 (약간 더 크게 해서 확실히 복귀)
    private float impactDistance = 4f;  // 타격 거리
    private float hitPhaseTime = 0.06f;    // 전진 후 타격 구간 유지 시간
    private float returnDelay = 0.05f;     // 타격 직후 복귀 임펄스까지의 짧은 간격
    private float maxAttackTime = 0.6f;    // 전체 공격 안전시간(무한 표류 방지)
    private float attackDrag = 2.0f;       // 공격 중에만 드래그를 잠깐 높여 관성 억제
    private bool useVelocityChange = false; // true면 질량 무시하고 속도변화 기반(일관성↑)

	private Coroutine dashCorutine;

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
        player = PlayerController.Instance.transform;
		repairKit = InGameManager.Instance.kitBox.transform;

		UpdateVisual();
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

        if (isAttacking) return;

		Transform target;
		if(!repairKit.gameObject.activeSelf)
		{
			target = player;
		}
        else
        {
			target = repairKit;
        }

        float centerDistance = Vector3.Distance(transform.position, target.position);

		if(attackCooldown < 0 && centerDistance <= attackRange) {
			attackCooldown = attackDelay;
			Attack();
			return;
		}

        // --- ▼ [수정 2] 거리 및 방향 계산 로직 변경 ▼ ---
        Vector3 selfPos = transform.position;
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
	}

	private void Attack()
	{
		if(!isAttacking && !isDie)
		{
			if (dashCorutine != null) StopCoroutine(dashCorutine);
            dashCorutine = StartCoroutine(CoDashAndReturn());
		}
	}

    private IEnumerator CoDashAndReturn()
    {
        isAttacking = true;
        Transform target;
        if (!repairKit.gameObject.activeSelf)
        {
            target = player;
        }
        else
        {
            target = repairKit;
        }

        // 시작 상태 저장
        Vector3 startPos = rb.position;
        Vector3 upDir = transform.up; // 현재 나의 '위' 방향

        // --- ▼ [수정] 방향 계산 로직 변경 ▼ ---
        // '위' 방향을 무시하고 타겟을 향하는 방향 벡터 계산
        Vector3 dirToTarget = target.position - startPos;
        Vector3 fwdDir = Vector3.ProjectOnPlane(dirToTarget, upDir).normalized;
        // --- ▲ [수정] 종료 ▲ ---

        // 공격 안정화... (이하 기존 코드와 거의 동일)
        float _origDrag = rb.linearDamping;
        rb.linearDamping = attackDrag;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // === 1) 전진 임펄스 ===
        var forceMode = useVelocityChange ? ForceMode.VelocityChange : ForceMode.Impulse;
        rb.AddForce(fwdDir * forwardImpulse, forceMode);

        // 타격 조건: 짧은 시간 대기 OR 목표 근접
        float t = 0f;
        // 거리 계산도 평면 기준으로 변경
        while (t < hitPhaseTime && Vector3.Distance(Vector3.ProjectOnPlane(rb.position, upDir), Vector3.ProjectOnPlane(target.position, upDir)) > impactDistance)
        {
            t += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // ★ 여기서 1회 데미지/이펙트 처리

        if (returnDelay > 0f) yield return new WaitForSeconds(returnDelay);

        // === 2) 복귀 임펄스 ===
        // 복귀 방향도 평면 기준으로 계산
        Vector3 backDir = (Vector3.ProjectOnPlane(startPos, upDir) - Vector3.ProjectOnPlane(rb.position, upDir)).normalized;
        rb.AddForce(backDir * backwardImpulse, forceMode);

        // 시작점 근접까지 감시
        float elapsed = 0f;
        const float stopEps = 0.05f;
        while (Vector3.Distance(Vector3.ProjectOnPlane(rb.position, upDir), Vector3.ProjectOnPlane(startPos, upDir)) > stopEps && elapsed < maxAttackTime)
        {
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // 스냅 & 잔류 속도 제거
        rb.MovePosition(startPos);
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.linearDamping = _origDrag;

        isAttacking = false;
    }

    private void GetDamage()
	{
		hp -= PlayerController.Instance.damage;
		UpdateVisual();

        if (hp <= 0)
		{
            StopCoroutine(dashCorutine);
            Destroy(gameObject);
		}
	}

	private void UpdateVisual()
	{
		slider.value = (float)hp / maxHp;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController.Instance.GetDamage(damage);
        }
        else if (collision.gameObject.CompareTag("RepairKit"))
        {
            InGameManager.Instance.kitBox.SetDamage(damage);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Weapon"))
		{
			GetDamage();
        }
    }
}
