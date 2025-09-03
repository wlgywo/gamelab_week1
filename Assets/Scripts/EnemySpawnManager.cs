using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
	public GameObject enemyPrefab;

	public int spawnCount = 10;

	public float firstDelay = 10f;	// 시작 지연
	public float spawnInterval = 3f;    // 소환주기


    void OnEnable()
    {
        InvokeRepeating("SpawnEnemy", firstDelay, spawnInterval);
    }

    void OnDisable()
    {
		CancelInvoke("SpawnEnemy");
    }

    void SpawnEnemy()
	{
		if (enemyPrefab == null) return;

		for (int i = 0; i < spawnCount; i++) {
			Instantiate(enemyPrefab,GetRandomPos() , enemyPrefab.transform.rotation);
		}
	}

	Vector3 GetRandomPos()
	{
		Vector3 playerPos = transform.position;

		float offsetX = Random.Range(-10f, 10f);
		float offsetZ = Random.Range(-10f, 10f);

		return new Vector3(playerPos.x + offsetX,
							playerPos.y,
							playerPos.z + offsetZ);
	}
}
