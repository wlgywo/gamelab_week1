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

		UpdateVisual();
    }

    private void FixedUpdate()
	{
		if (isDie) return;
        if (isAttacking) return;

		Transform target;
		target = player;
		

        float centerDistance = Vector3.Distance(transform.position, target.position);

		if(attackCooldown < 0 && centerDistance <= attackRange) {
			attackCooldown = attackDelay;
			Attack();
			return;
		}

        
    }

    private void Update()
	{
		attackCooldown -= Time.deltaTime;
	}
    public void SetManager(EnemySpawnManager spawnManager)
    {
        manager = spawnManager;
    }
    private void Attack()
	{
		if(!isAttacking && !isDie)
		{
            //Vector3 dir = (collision.transform.position - transform.position).normalized;

            //// 반대 방향으로 충격 주고 싶다면: (transform.position - collision.transform.position).normalized;

            //// Rigidbody에 순간적인 힘 가하기
            //rb.AddForce(-dir * bumpPower, ForceMode.Impulse);

            //// Player도 튕기게 하고 싶다면 Player의 Rigidbody에 Force 추가
            //Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
            //if (playerRb != null)
            //{
            //    playerRb.AddForce(dir * bumpPower, ForceMode.Impulse);
            //}
        }
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

    private void OnDestroy()
    {
        if (manager != null)
        {
            manager.ReSpawnEnemy();
        }
    }
}
