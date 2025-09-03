using UnityEngine;

public class KitBox : MonoBehaviour
{
    public float curHp { get; private set; }
    private float MaxHp = 100;
    private const float hpUpgradeRange = 20f;

    // 슬라이더 추가

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
            InGameManager.Instance.GameOver();
            Debug.Log("수리 키트 파괴");
        }
    }

    public void HPUp()
    {
        MaxHp += hpUpgradeRange;
        curHp = MaxHp; // 슬라이더 갱신
    }

}
