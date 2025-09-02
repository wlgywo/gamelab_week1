using UnityEngine;

public class BossAI : MonoBehaviour
{
    [SerializeField] protected Rigidbody rb;

    public int hp = 100;

    protected const string ISCHASE = "IsChase";
    protected const string ISAttack = "IsAttack";

    public float attackRange = 2f;       // 공격 사거리

    protected Transform player;
    protected float attackCooldown;      // 공격 쿨타임
    public float attackDelay = 3f;

    public float speed = 5.0f;
    public float moveRotationSpeed = 10.0f;

    protected bool isDamaged = false; // 지금 맞은 상태인가
    //private float curInvincibleTimer = 0f;

    protected Coroutine rotateCoroutine;

    protected bool isDie = false;

    protected virtual void FixedUpdate()
    {
        if (isDie) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (attackCooldown < 0)
        {
            attackCooldown = attackDelay;
            Attack();
        }
        else
        {
            Vector3 dir = player.transform.position - transform.position;
            rb.MovePosition(rb.position + dir.normalized * speed * Time.fixedDeltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(dir, transform.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, moveRotationSpeed * Time.fixedDeltaTime));
        }
    }

    protected virtual void Update()
    {
        attackCooldown -= Time.deltaTime;
    }

    protected virtual void Attack()
    {
        // 공격 어떻게 할건지 넣기
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            //Tapasse12_PlayerController.Instance.GetDamage(bodyDamage);
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerHand"))
        {
            if (isDamaged) return;

            //Hp -= Tapasse12_PlayerController.Instance.Damage;
            isDamaged = true;

            if (hp <= 0)
            {
                isDie = true;
                //Tapasse12_IngameManager.Instance.GetItem(transform);

                //Tapasse12_IngameManager.Instance.UpdateDoor();
                Destroy(gameObject);
            }
        }
    }
}
