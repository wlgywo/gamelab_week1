using UnityEngine;

public class BlueZoneScript : MonoBehaviour
{
    private int attackDamage = 10;
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
            //PlayerController.Instance.SetAttackDamage(-damage);
        }
        if (other.gameObject.CompareTag("Boss"))
        {
            BossAI.Instance.SetAttackDamage(attackDamage);
        }
    }
}
