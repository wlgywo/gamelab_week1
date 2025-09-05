using UnityEngine;

public class BossBullet : MonoBehaviour
{
    private Rigidbody bulletRb;

    private int damage = 10;
    public float speed = 5f;
    public float maxTorque = 10f;
    Transform playerPos;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerPos = PlayerController.Instance.transform;
        bulletRb = GetComponent<Rigidbody>();
       // bulletRb.AddForce(RandomTorque(), RandomTorque(), RandomTorque(), ForceMode.Impulse);
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerController.Instance == null) return;
        Vector3 lookDirection = (playerPos.position - transform.position).normalized;
        bulletRb.AddForce(lookDirection * speed);
    }

    private float RandomTorque()
    {
        return Random.Range(-maxTorque, maxTorque);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") )
        {
            PlayerController.Instance.GetDamage(5);
            Destroy(gameObject);
        }
        if(other.CompareTag("Ground"))
        {
            Destroy(gameObject);

        }
    }

}
