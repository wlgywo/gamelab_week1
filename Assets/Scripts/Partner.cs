using UnityEngine;

public class Partner : MonoBehaviour
{
    private Animator anim;

    public GameObject bulletPrefabs;
    public AI ai;
    public Transform attackPos;
    private float attackTimer =3f;
    private float curAttackTimer;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        curAttackTimer -= Time.deltaTime;

        if(curAttackTimer < 0)
        {
            DetectAI();
        }
    }

    public void DetectAI()
    {
        int num = (int)PlayerController.Instance.mapDirect;

        foreach(var i in SpawnManager.Instance.Spawners[num].AIList)
        {
            if(i != null)
            {
                ai = i;
                break;
            }
        }

        if(ai != null)
        {
            AttackBullet();
            ai = null;
            curAttackTimer = attackTimer;
        }
    }

    public void AttackBullet()
    {
        anim.SetTrigger("isAttack");
        Vector3 dir = ai.transform.position - transform.position;

        Bullet bullet = Instantiate(bulletPrefabs, attackPos.position, attackPos.rotation).GetComponent<Bullet>();
        bullet.Attack(dir.normalized);
    }
}
