using UnityEngine;
using UnityEngine.UI;

public class KitBox : MonoBehaviour
{
    [SerializeField] private Slider slider;

    public float curHp { get; private set; }
    private float MaxHp = 100;
    private const float hpUpgradeRange = 20f;

    private bool isDamaged = false;

    private float curTimer = 0;
    private float damageTimer = 0.1f;


    // 슬라이더 추가

    private void Awake()
    {
        curHp = MaxHp;
        UpdateVisual();
    }

    private void Update()
    {
       if(isDamaged)
        {
            curTimer += Time.deltaTime;
            if(damageTimer <= curTimer)
            {
                curTimer = 0;
                isDamaged = false;
            }
        }
    }


    public void SetDamage(float damage)
    {
        if (isDamaged) return;

        isDamaged = true;

        curHp -= damage;
        UpdateVisual();
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

    public void UpdateVisual()
    {
        slider.value = curHp / MaxHp;
    }

}
