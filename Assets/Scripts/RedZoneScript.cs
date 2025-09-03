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
            BossAI.Instance.GetDamage();
        }
    }
}
