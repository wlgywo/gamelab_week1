using UnityEngine;
using UnityEngine.UI;

public class KitBox : MonoBehaviour
{
    [SerializeField] private Slider slider;

    [field:SerializeField] public float curHp { get; private set; }
    [SerializeField] private float MaxHp = 100;
    private const float hpUpgradeRange = 20f;

    private bool isDamaged = false;

    private float curTimer = 0;
    private float damageTimer = 0.1f;

    [SerializeField] private float kitRange = 2f;

    // 슬라이더 추가

    private void Awake()
    {
        curHp = MaxHp;
        UpdateVisual();
    }

    private void Update()
    {
        if(Vector3.Distance(PlayerController.Instance.transform.position, transform.position) <= kitRange)
        {
            PlayerController.Instance.nearKitBox = true;
        }
        else PlayerController.Instance.nearKitBox = false;

        if (isDamaged)
        {
            curTimer += Time.deltaTime;
            if (damageTimer <= curTimer)
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

        UpdateVisual();
    }

    public void UpdateVisual()
    {
        slider.value = curHp / MaxHp;
    }

}
