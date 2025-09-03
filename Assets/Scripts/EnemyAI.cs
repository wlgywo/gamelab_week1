using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
	[SerializeField] protected Rigidbody rb;
 
	// 다른 오브젝트 관련
	public Transform repairKit;
	public Transform player;

	// 몬스터 상태 관련
	public int hp = 100;
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

    private void Start()
    {
        player = PlayerController.Instance.transform;
		repairKit = InGameManager.Instance.kitBox.transform;
    }

    private void FixedUpdate()
	{
		if (isDie) return;
		//if (player == null && repairKit == null) return;
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

        Vector3 selfPos = transform.position;
        Vector3 flatTargetPos = new Vector3(target.position.x, selfPos.y, target.position.z);

        Vector3 dirFromCenter = (flatTargetPos - selfPos).normalized;

        if (centerDistance > stopRadius + stopEpsilon)
        {
            // 경계점 계산 (flatTargetPos 사용)
            Vector3 targetOnBoundary = flatTargetPos + dirFromCenter * stopRadius;

            Vector3 toTarget = targetOnBoundary - selfPos;
            Vector3 nextPos = rb.position + toTarget.normalized * speed * Time.fixedDeltaTime;
            rb.MovePosition(nextPos);

            // 높이 무시 후 transform.up 기준으로 회전
            Quaternion targetRot = Quaternion.LookRotation(toTarget, transform.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, moveRotationSpeed * Time.fixedDeltaTime));
        }
        else
        {
            // 멈출 때
            rb.MovePosition(rb.position);
            rb.linearVelocity = Vector3.zero; // 관성 제거

            Vector3 lookDir = flatTargetPos - selfPos; // y 무시된 방향
            if (lookDir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir, transform.up);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, moveRotationSpeed * Time.fixedDeltaTime));
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
			StartCoroutine(CoDashAndReturn());
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
		float baseY = startPos.y;
		Vector3 kitFlat = new Vector3(target.position.x, baseY, target.position.z);

		// 향할 방향 계산
		Vector3 fwdDir = (kitFlat - startPos).normalized;

		// 공격 안정화: 기존 속도/각속도 제거 + 드래그 임시 증가
		float _origDrag = rb.linearDamping;
		rb.linearDamping = attackDrag;
		rb.linearVelocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;

		// === 1) 전진 임펄스 ===
		var forceMode = useVelocityChange ? ForceMode.VelocityChange : ForceMode.Impulse;
		rb.AddForce(fwdDir * forwardImpulse, forceMode);

		// 타격 조건: 짧은 시간 대기 OR 목표 근접
		float t = 0f;
		while (t < hitPhaseTime && Vector3.Distance(rb.position, kitFlat) > impactDistance)
		{
			t += Time.fixedDeltaTime;
			yield return new WaitForFixedUpdate();
		}

		// ★ 여기서 1회 데미지/이펙트 처리(원하면)
		// repairKit.GetComponent<IHittable>()?.OnHit(damage);

		// 아주 짧은 간격 후 복귀 임펄스
		if (returnDelay > 0f) yield return new WaitForSeconds(returnDelay);

		// === 2) 복귀 임펄스 ===
		Vector3 backDir = (startPos - rb.position).normalized; // 현재 위치 기준 ‘시작점’ 방향
		rb.AddForce(backDir * backwardImpulse, forceMode);

		// 시작점 근접까지 감시(안전 타임아웃 포함)
		float elapsed = 0f;
		const float stopEps = 0.05f;
		while (Vector3.Distance(rb.position, startPos) > stopEps && elapsed < maxAttackTime)
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
}
