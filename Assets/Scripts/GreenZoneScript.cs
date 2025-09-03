using UnityEngine;

public class GreenZoneScript : MonoBehaviour
{
    private int heal = 10;
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
            //PlayerController.Instance.GetHeal(heal);
        }
        if (other.gameObject.CompareTag("Boss"))
        {
            // 보스도 체력 증가 하지만 증가량 많음.
        }
    }
}
