using UnityEngine;

public class KitBox : MonoBehaviour
{
    public float curHp { get; private set; }
    private float MaxHp = 100;

    private void Awake()
    {
        curHp = MaxHp;
    }


    public void SetDamage(float damage)
    {
        curHp -= damage;
        if(curHp <= 0)
        {
            curHp = 0;
            Debug.Log("수리 키트 파괴");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player")) // 임시, 이걸 Enemy에서 실행하고 IngameManager에서 여기 SetDamager를 접근
        {
            SetDamage(20);
        }
    }
}
