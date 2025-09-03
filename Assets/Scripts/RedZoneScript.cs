using UnityEngine;

public class RedZoneScript : MonoBehaviour
{
    private int damage = 10;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerController.Instance.GetDamage(damage);
        }
        if(other.gameObject.CompareTag("Boss"))
        {
            // 보스도 체력 감소 하지만 감소량 적음.
        }
    }
}
