using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class AI : MonoBehaviour
{
    [Header("Compnent")]
    [SerializeField] protected Slider slider;
    [SerializeField] protected ParticleSystem hitEffect;
    [SerializeField] protected ParticleSystem criticalEffect;
    protected Rigidbody rb;
    protected Animator anim;

    // 컴파일 변수
    protected const string CHASEANIM = "IsChase";
    protected const string ATTACKANIM = "IsAttack";

    [Header("State")]
    [SerializeField] protected Transform target;
    [SerializeField] protected Transform marble;
    [SerializeField] protected float detectionRange = 7.5f;
    [SerializeField] protected float attackRange = 5f;
    [SerializeField] protected float curAttackSpeed = 3f;
    [SerializeField] protected float attackSpeed = 3f;
    [SerializeField] protected float moveWaitTime = 1.5f; // 공격 후 움직임 대기 시간
    [SerializeField] protected float rotateSpeed = 10f;
    protected bool isAttack;
    protected Vector3 flatDir;
    protected Coroutine attackCorutine;
    protected Coroutine hitCorutine;
    public MapDirect mapDir {  get; protected set; }

    [Header("Status")]
    [SerializeField] protected int curhp = 50;
    [SerializeField] protected int hp = 50;
    [SerializeField] protected float speed = 5f;
    [SerializeField] protected int damage = 5;
    protected bool isHit = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        rb.useGravity = false;
    }

    private void Start()
    {
        UpdateVisual();
    }

    protected virtual void FixedUpdate()
    {
        if (isAttack) return;

        if (mapDir == PlayerController.Instance.mapDirect && Vector3.Distance(
            PlayerController.Instance.transform.position, transform.position) <= detectionRange)
        {
            target = PlayerController.Instance.transform;
        }
        else target = marble;

        Vector3 dir = target.position - transform.position;
        flatDir = Vector3.ProjectOnPlane(dir, transform.up); // 현재 내 Y축을 방향을 제외한 방향을 계산 (정사영을 통해xz평면에서만의 차이를 구함)

        if(flatDir.magnitude <= attackRange)
        {
            anim.SetBool(CHASEANIM, false);

            if (curAttackSpeed <= 0f)
            {
                Attack();
                return;
            }
        }
        else
        {
            anim.SetBool(CHASEANIM, true);
            rb.MovePosition(rb.position + flatDir.normalized * speed * Time.deltaTime);

            Quaternion targetRot = Quaternion.LookRotation(flatDir, transform.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotateSpeed * Time.fixedDeltaTime));
        }
    }

    private void Update()
    {
        curAttackSpeed -= Time.deltaTime;
    }


    protected virtual void Attack()
    {
        anim.SetTrigger(ATTACKANIM);
        isAttack = true;

        Debug.Log("공격 실행");
    }
    private void GetDamage()
    {
        if (isHit) return;
        isHit = true;

        if(InGameManager.Instance.knockBack)
        {
            Vector3 dir = transform.position - PlayerController.Instance.transform.position;
            dir = Vector3.ProjectOnPlane(dir, transform.up); // 뜨는거 방지

            rb.linearVelocity = Vector3.zero;
            rb.AddForce(dir.normalized * InGameManager.Instance.knockBackPower, ForceMode.Impulse);
        }

        if (InGameManager.Instance.blood)
        {
            InGameManager.Instance.Heal(Random.Range(1, 4)); // 1~3만큼 랜덤 회복
        }

        int damage = InGameManager.Instance.power;

        int ciritical = Random.Range(0, 100);
        if (ciritical <= InGameManager.Instance.ciritical)
        {
            damage = Mathf.FloorToInt(damage * 1.5f);
            criticalEffect.Play();
        }
        else hitEffect.Play();

        curhp -= damage;
        
        UpdateVisual();

        if (hitCorutine != null) StopCoroutine(hitCorutine);
        hitCorutine = StartCoroutine(DamageCoroutine()); // 연속 공격 방지

        if (curhp <= 0)
        {
            InGameManager.Instance.GetExp();
            DestroySelf();
        }
    }

    private IEnumerator DamageCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        isHit = false;
        if (InGameManager.Instance.knockBack) rb.linearVelocity = Vector3.zero; // 밀림 방지
    }

    private void UpdateVisual()
    {
        slider.value = (float)curhp / hp;
    }

    public void SetMarble(Transform pos, MapDirect dir)
    {
        marble = pos;
        mapDir = dir;
    }

    // 파괴된 marble에 위치한 ai 제거
    public void DestroySelf()
    {
        StopAllCoroutine();

        Destroy(gameObject);
    }

    protected void StopAllCoroutine()
    {
        if (attackCorutine != null) StopCoroutine(attackCorutine);
        if (hitCorutine != null) StopCoroutine(hitCorutine);
    }


    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player")) PlayerController.Instance.GetDamage(damage);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Marble"))
        {
            if (target == PlayerController.Instance.transform) return;
            SpawnManager.Instance.Spawners[(int)mapDir].marble.Damage(damage);
        }
        else if (other.CompareTag("Weapon")) GetDamage();
    }
}
