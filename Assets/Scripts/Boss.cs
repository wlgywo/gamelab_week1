using System.Collections;
using UnityEngine;

public class Boss : AI
{
    public static Boss Instance { get; private set; }

    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private GameObject weapon;
    [SerializeField] private Transform hand;

    // const
    protected const float gravityTimer = 4f;
    protected const float randomTimer = 4f;
    protected const float basicAttackTimer = 2f;
    private const string LEFTANIM = "isLeft";
    private const string RIGHTANIM = "isRight";
    private const string MELEEANIM = "isMelee";
    private const string RANDOMANIM = "isRandom";
    private const string CARDANIM = "isCard";

    [SerializeField] private GameObject card;

    bool isCritical = false;

    public bool isGravity = false;

    public bool isMarbleChange = false;
    public Vector3 marPos = Vector3.zero;

    protected override void Awake()
    {
        if(Instance == null) Instance = this;

        anim = GetComponentInChildren<Animator>();
        base.Awake();

        slider = InGameManager.Instance.bossSlider;
    }

    protected override void FixedUpdate()
    {
        if(isMarbleChange)
        {
            Vector3 dir = marPos - transform.position;

            if (dir.magnitude <= 0.4f)
            {
                rb.MovePosition(marPos);
                isMarbleChange = false;
            }
            else rb.MovePosition(rb.position + dir.normalized * speed * 3 * Time.deltaTime);

            return;
        }

        base.FixedUpdate();
    }

    protected override void Attack()
    {
        isAttack = true;

        AttackSelect();
    }

    private void AttackSelect()
    {
        int num = Random.Range(0, 100);
        
        if(num < 25)
        {
            TurnGraviry();
        }
        else if(num < 55)
        {
            RandomAttack();
        }
        else if(num < 75)
        {
            MeleeAttack();
        }
        else if(num < 100)
        {
            CardAttack();
        }
    }

    private void TurnGraviry()
    {
        curAttackSpeed = gravityTimer;

        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        attackCoroutine = StartCoroutine(TurnGravityCoroutine());
    }

    private IEnumerator TurnGravityCoroutine()
    {
        while(GravityManager.Instance.isGravity) yield return null;

        int num = Random.Range(0, 2);

        if (num == 0)
        {
            anim.SetTrigger(LEFTANIM);

            yield return new WaitForSeconds(1f);
            InGameManager.Instance.LeftGravity();
        }
        else
        {
            anim.SetTrigger(RIGHTANIM);

            yield return new WaitForSeconds(1f);
            InGameManager.Instance.RightGravity();
        }

        curAttackSpeed = gravityTimer;

        isAttack = false;
    }

    private void RandomAttack()
    {
        curAttackSpeed = randomTimer;

        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        attackCoroutine = StartCoroutine(StartRandomAttack());
    }

    private IEnumerator StartRandomAttack()
    {
        anim.SetTrigger(RANDOMANIM);
        yield return new WaitForSeconds(0.25f);

        Vector3 randomDir;

        int cnt = 0;

        while(cnt < 30) // 2초 동안 30회 공격 
        {
            randomDir = Random.onUnitSphere;

            // 캐릭터 local up 기준으로 반구 위쪽만 남기기
            if (Vector3.Dot(randomDir, transform.up) < 0f)
            {
                //randomDir = -randomDir; // 아래쪽이면 반전해서 위쪽으로
                //isReverse = true;

                randomDir = Vector3.Reflect(randomDir, transform.up);
            }

            Quaternion rotation = Quaternion.LookRotation(randomDir, transform.up);

            cnt++;

            yield return new WaitForSeconds(0.05f); // 0.05초 마다 공격

            GameObject cardObject = Instantiate(card, transform.position, rotation);

            // 3. 힘 주기
            Rigidbody rb = cardObject.GetComponent<Rigidbody>();
            rb.AddForce(randomDir * 15f, ForceMode.Impulse);
        }

        isAttack = false;
    }

    private void MeleeAttack()
    {
        curAttackSpeed = basicAttackTimer;

        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        attackCoroutine = StartCoroutine(AttackReset());
    }

    private IEnumerator AttackReset()
    {
        weapon.SetActive(true);
        trailRenderer.enabled = true;
        anim.SetTrigger(MELEEANIM);

        yield return new WaitForSecondsRealtime(1f);

        weapon.SetActive(false);
        trailRenderer.enabled = false;
        isAttack = false;
    }

    private void CardAttack()
    {
        curAttackSpeed = basicAttackTimer;

        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        attackCoroutine = StartCoroutine(CardAttackReset());
    }

    private IEnumerator CardAttackReset()
    {
        Vector3 dir = target.position - transform.position;
        dir.Normalize();
        anim.SetTrigger(CARDANIM);

        yield return new WaitForSecondsRealtime(0.2f);

        GameObject cardObject = Instantiate(card, hand.position, transform.rotation);
        Rigidbody rb = cardObject.GetComponent<Rigidbody>();
        rb.AddForce(dir * 15f, ForceMode.Impulse);

        yield return new WaitForSecondsRealtime(0.8f);

        isAttack = false;
    }


    protected override void GetDamage()
    {
        if (isHit) return;
        isHit = true;

        if (InGameManager.Instance.blood)
        {
            InGameManager.Instance.Heal(Random.Range(1, 4)); // 1~3만큼 랜덤 회복
        }

        int power = InGameManager.Instance.power;

        int ciritical = Random.Range(0, 100);
        if (ciritical <= InGameManager.Instance.ciritical || isCritical)
        {
            isCritical = false;
            power = Mathf.FloorToInt(power * 1.5f);
            criticalEffect.Play();
        }
        else hitEffect.Play();

        curhp -= power;

        UpdateVisual();

        if (hitCorutine != null) StopCoroutine(hitCorutine);
        hitCorutine = StartCoroutine(DamageCoroutine()); // 연속 공격 방지

        if (curhp <= 0)
        {
            //InGameManager.Instance.GetExp();
            InGameManager.Instance.GameClear();
            DestroySelf();
        }
    }

    protected override void StopAllCoroutine()
    {
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        base.StopAllCoroutine();
    }

    public void ChangeMarble(Vector3 pos, Quaternion rot, Transform marble, MapDirect mapDirect)
    {
        marPos = pos;
        //spawn
        rb.rotation = rot;
        anim.SetTrigger(RANDOMANIM);

        isMarbleChange = true;
        SetMarble(marble, mapDirect);
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            InGameManager.Instance.GetExp();
            DestroySelf();
            criticalEffect.Play();
        }

        base.OnTriggerEnter(other);
    }
}
