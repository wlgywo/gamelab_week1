using System.Collections;
using UnityEngine;

public class Spider : AI
{
    private Vector3 curPos;
    [SerializeField] private float dashRange = 5f;
    [SerializeField] private float dashSpeed = 0.5f;  // 도달 시간

    protected override void Awake()
    {
        anim = GetComponent<Animator>();
        base.Awake();
    }

    protected override void Attack()
    {
        base.Attack();

        curPos = transform.position;
        flatDir = Vector3.ProjectOnPlane((target.position - curPos), transform.up);

        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        attackCoroutine = StartCoroutine(DashAttack());
    }
    protected override void FixedUpdate()
    {
        if (isHit && InGameManager.Instance.knockBack) return;

        base.FixedUpdate();
    }

    private IEnumerator DashAttack()
    {
        // 1. 전진
        float elapsed = 0f;

        Vector3 dashTarget = transform.position + flatDir.normalized * dashRange; // dashRange만큼 전진 (공격 거리)

        while (elapsed < dashSpeed)
        {
            elapsed += Time.deltaTime;
            rb.MovePosition(Vector3.Lerp(curPos, dashTarget, elapsed/dashSpeed));
            yield return null;
        }

        // 2. 복귀
        elapsed = 0f;
        float returnTime = 0.3f;

        while (elapsed < returnTime)
        {
            elapsed += Time.deltaTime;
            rb.MovePosition(Vector3.Lerp(transform.position, curPos, elapsed / returnTime));
            yield return null;
        }

        curAttackSpeed = attackSpeed;
        isAttack = false;
    }

    protected override void GetDamage()
    {
        if (InGameManager.Instance.knockBack)
        {
            Vector3 dir = transform.position - PlayerController.Instance.transform.position;
            dir = Vector3.ProjectOnPlane(dir, transform.up); // 뜨는거 방지

            rb.linearVelocity = Vector3.zero;
            rb.AddForce(dir.normalized * InGameManager.Instance.knockBackPower, ForceMode.Impulse);
        }

        base.GetDamage();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            InGameManager.Instance.GetExp();
            criticalEffect.Play();
            DestroySelf();
        }

        base.OnTriggerEnter(other);
    }
}
