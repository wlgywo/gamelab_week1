using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnemyAI : MonoBehaviour
{
	[SerializeField] protected Rigidbody rb;
	[SerializeField] private Slider slider;
    private EnemySpawnManager manager;

   


    // 다른 오브젝트 관련
    public Transform player;
    private Animator animator;

    // 몬스터 상태 관련
    public int level;
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

    // 이동 관련
    private float wanderInterval = 3.0f;  // 3초마다 방향 갱신
    private Vector3 wanderDir = Vector3.zero;
    private float nextWanderTime = 0f;
    private float chaseSpeed = 5f;

    // 공격 관련
    private bool isAttacking = false;
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
    private float detectionRange = 30f;   // 추격 시작 범위
    private float attackRange = 4.0f;     // 공격 범위
    private float stoppingDistance = 3.9f;// 너무 붙지 않기
    private float bumpPower = 20f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        if (animator != null) { 
            Debug.Log("Animator component found.");
        }
    }

    
    private void Start()
    {
        player = PlayerController.Instance.transform;
        PickNewWanderDir();
        UpdateVisual();
    }

    private void FixedUpdate()
    {
        if (isDie) return;
        if (isAttacking) return;

        attackCooldown -= Time.fixedDeltaTime;

        Transform target = player;

        float dist = Vector3.Distance(transform.position, target.position);

        // 1)공격
        if (attackCooldown <= 0 && dist <= attackRange)
        {
            attackCooldown = attackDelay;
            Attack();
            return;
        }

        // 2) 추격
        if (dist <= detectionRange)
        {
            Vector3 dir = (target.position - transform.position);
            dir.y = 0f;
            float d = dir.magnitude;
            if (d > stoppingDistance) // 너무 붙었으면 멈춤
            {
                dir /= d; // normalized
                Move(dir, chaseSpeed);
                FaceTowards(dir);
            }
            else
            {
                // 정지 + 플레이어 바라보기만
                FaceTowards(dir.sqrMagnitude > 0.0001f ? dir.normalized : transform.forward);
            }
            return;

        }
    }

    void FaceTowards(Vector3 dir)
    {
        if (dir.sqrMagnitude < 1e-6f) return;
        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, 10f * Time.fixedDeltaTime)); // 회전 부드럽게
    }
    void Move(Vector3 dir, float speed)
    {
        if (dir.sqrMagnitude < 1e-6f) return;
        Vector3 targetPos = rb.position + dir * speed * Time.fixedDeltaTime;
        rb.MovePosition(targetPos);
    }

    public void SetManager(EnemySpawnManager spawnManager)
    {
        manager = spawnManager;
    }
    private void Attack()
	{
		if(!isAttacking && !isDie)
		{
            Vector3 dir = (player.position - transform.position).normalized;

            rb.AddForce(dir * bumpPower, ForceMode.Impulse);
        }
	}

    void PickNewWanderDir()
    {
        Vector2 v = Random.insideUnitCircle.normalized;
        wanderDir = new Vector3(v.x, 0f, v.y);
        nextWanderTime = Time.time + wanderInterval;
    }


    private void GetDamage()
	{
		hp -= PlayerController.Instance.damage;
		UpdateVisual();

        if (hp <= 0)
		{
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

    private void OnDestroy()
    {
        if (manager != null)
        {
            manager.ReSpawnEnemy();
        }
    }
}
