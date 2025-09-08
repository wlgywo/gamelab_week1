using UnityEngine;

public class Card : MonoBehaviour
{
    private float timer = 3f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Attack()
    {
        SpawnManager.Instance.Spawners[
            (int)Boss.Instance.mapDir].marble.Damage(
            Boss.Instance.damage);
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer < 0) Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Marble"))
        {
            Attack();
            Destroy(gameObject);
        }
    }
}
