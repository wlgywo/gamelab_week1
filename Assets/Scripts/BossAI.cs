using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

public class BossAI : MonoBehaviour
{
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

	}


    private void FixedUpdate()
	{
		if (isDie) return;
		if (player == null) return;
		if (isAttacking) return;
		
        float centerDistance = Vector3.Distance(transform.position, player.position);

		if(attackCooldown < 0 && centerDistance <= attackRange) {
			attackCooldown = attackDelay;
			Attack();
			return;
		}

		Vector3 dirFromCenter = (transform.position - player.transform.position).normalized;
		if(centerDistance > stopRadius + stopEpsilon)
		{
			Vector3 targetOnBoundary = player.position + dirFromCenter * stopRadius;

			Vector3 toTarget = targetOnBoundary - transform.position;
			Vector3 nextPos = rb.position + toTarget.normalized * speed * Time.fixedDeltaTime;
			rb.MovePosition(nextPos);

			Quaternion targetRot = Quaternion.LookRotation(toTarget, transform.up);
			rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, moveRotationSpeed * Time.fixedDeltaTime));

		}
		else
		{
			rb.MovePosition(rb.position);
			rb.linearVelocity = Vector3.zero; // 관성 잔류 제거(필요시)

			Vector3 lookDir = (player.position - transform.position);
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
			int idx = UnityEngine.Random.Range(0, skills.Length);
			skills[idx].Invoke();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
        if (other.gameObject.CompareTag("Player"))
        {
			player.GetComponent<PlayerController>().GetDamage(damage);
        }
    }
}
