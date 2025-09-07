using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody rb;
    public float power = 15f;
    private float deleteTiemr = 3;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Attack(Vector3 dir)
    {
        rb.AddForce(dir * power, ForceMode.Impulse);
    }

    private void Update()
    {
        deleteTiemr -= Time.deltaTime;

        if (deleteTiemr < 0) Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy")) Destroy(gameObject);
    }
}
