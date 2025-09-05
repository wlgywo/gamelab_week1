using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class AI : MonoBehaviour
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
    protected float attackRange = 10f;
    protected float curAttackSpeed = 3f;
    protected float attackSpeed = 3f;
    protected float rotateSpeed = 10f;
    protected bool isAttack;

    [Header("능력치")]
    protected int curhp = 50;
    protected int hp = 50;
    protected float speed = 5f;
    protected int damage = 5;


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
    private void UpdateVisual()
    {
        slider.value = (float)curhp / hp;
    }

    private void FixedUpdate()
    {
        if (isAttack) return;

        // 일단 타겟 플레이어로 설정, 추후 거리가 가까워지면 타겟 변경
        target = PlayerController.Instance.transform;

        Vector3 dir = target.position - transform.position;
        Vector3 flatDir = Vector3.ProjectOnPlane(dir, transform.up); // 현재 내 Y축을 방향을 제외한 방향을 계산 (정사영을 통해xz평면에서만의 차이를 구함)

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


    protected void Attack()
    {
        Debug.Log("공격 실행");   
        anim.SetTrigger(ATTACKANIM);

        StartCoroutine(AttackCoroutine());
    }

    protected IEnumerator AttackCoroutine()
    {
        isAttack = true;
        float attackDelay = 1.5f;
        yield return new WaitForSeconds(attackDelay);

        curAttackSpeed = attackSpeed;
        isAttack = false;
    }

    public void SetMarble(Transform pos)
    {
        marble = pos;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player")) PlayerController.Instance.GetDamage(damage);
    }
}
