using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

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
			player.GetComponent<PlayerController>().GetDamage(damage);
        }
    }

	public void GetDamage(int damage)
	{
		hp -= damage;
		{
			if(hp < 0)
			{
				InGameManager.Instance.GameClear();
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
