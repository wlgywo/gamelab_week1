using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MineralHpScript : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private int level;
    [SerializeField] private int color;
    private MineralSpawnManager manager;
    int maxHp = 0;
    int hp = 0;

    private void Start()
    {
        maxHp = level * 100;
        hp = maxHp;
        UpdateVisual();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Weapon"))
        {
            GetDamage(PlayerController.Instance.damage);
        }
    }

    public void SetManager(MineralSpawnManager spawnManager)
    {
        manager = spawnManager;
    }

    void GetDamage(int damage)
    {
        hp -= damage;
        if(hp <= 0)
        {
            switch (color)
            {
                case 0:
                    InGameManager.Instance.AddRedMineral();
                    Destroy(gameObject);
                    break;
                case 1:
                    InGameManager.Instance.AddOrangeMineral();
                    Destroy(gameObject);
                    break;
                case 2:
                    InGameManager.Instance.AddBlueMineral();
                    Destroy(gameObject);
                    break;
                case 3:
                    InGameManager.Instance.AddPurpleMineral();
                    Destroy(gameObject);
                    break;
                default:
                    break;
            }
       
        }
        UpdateVisual();
    }

    private void OnDestroy()
    {
        if(manager != null)
        {
            manager.ReSpawnMineral();
        }
    }
    private void UpdateVisual()
    {
        slider.value = (float)hp / maxHp;
    }
}
