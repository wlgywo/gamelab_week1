using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class AI : MonoBehaviour
{
    [Header("컴포넌트")]
    protected Rigidbody rb;
    [SerializeField]protected Slider slider;
    protected Animator anim;
    [SerializeField] protected ParticleSystem hitEffect;

    
    [Header("컴파일 변수")]
    protected const string CHASEANIM = "IsChase";
    protected const string ATTACKANIM = "IsAttack";

    [Header("상태")]
    [SerializeField] protected Transform target;
    [SerializeField] protected Transform marble;
    [SerializeField] protected float attackRange = 10f;
    [SerializeField] protected float curAttackSpeed = 3f;
    [SerializeField] protected float attackSpeed = 3f;
    [SerializeField] protected float moveWaitTime = 1.5f; // 공격 후 움직임 대기 시간
    [SerializeField] protected float rotateSpeed = 10f;
    protected bool isAttack;
    protected Vector3 flatDir;
    protected Coroutine attackCorutine;

    [Header("능력치")]
    protected int curhp = 50;
    [SerializeField] protected int hp = 50;
    [SerializeField] protected float speed = 5f;
    [SerializeField] protected int damage = 5;


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

    private void FixedUpdate()
    {
        if (isAttack) return;

        // 일단 타겟 플레이어로 설정, 추후 거리가 가까워지면 타겟 변경
        target = PlayerController.Instance.transform;

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
        hp -= PlayerController.Instance.damage;
        hitEffect.Play();
        UpdateVisual();

        if (hp <= 0)
        {
            StopCoroutine(attackCorutine);
            Destroy(gameObject);
        }
    }

    private void UpdateVisual()
    {
        slider.value = (float)curhp / hp;
    }

    public void SetMarble(Transform pos)
    {
        marble = pos;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player")) PlayerController.Instance.GetDamage(damage);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Weapon")) GetDamage();
    }
}
