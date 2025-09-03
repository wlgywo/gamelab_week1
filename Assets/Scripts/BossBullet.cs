using UnityEngine;

public class BossBullet : MonoBehaviour
{
    private Rigidbody bulletRb;

    public float speed = 10f;
    public float maxTorque = 10f;

    Transform playerPos = PlayerController.Instance.transform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        bulletRb = GetComponent<Rigidbody>();

        bulletRb.AddForce(RandomTorque(), RandomTorque(), RandomTorque(), ForceMode.Impulse);
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
}
