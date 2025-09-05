using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
	public GameObject enemyPrefab;

	public Transform[] pivots;
	
	public int spawnCount = 10;

	//public float firstDelay = 10f;	// 시작 지연

	private float curDelay = 0f;
	public float spawnInterval = 3f;    // 소환주기

	public bool isSpawn;

	List<GameObject> enemyList;

    private void Start()
    {
        enemyList = new List<GameObject>();
    }

    /*void OnEnable()
    {
        InvokeRepeating("SpawnEnemy", firstDelay, spawnInterval);
    }

    void OnDisable()
    {
		/CancelInvoke("SpawnEnemy");
    }*/

    private void Update()
    {
        if(isSpawn)
		{
			curDelay -= Time.deltaTime;
			if(curDelay <= 0)
			{
				curDelay = spawnInterval;
				SpawnEnemy();
            }
		}
    }

    void SpawnEnemy()
	{
		if (enemyPrefab == null) return;

		int num = Random.Range(0, pivots.Length);

        GameObject enemy = Instantiate(enemyPrefab, pivots[num].position, PostPlayerController.Instance.transform.rotation);
        enemyList.Add(enemy);
	}

	/*Vector3 GetRandomPos()
	{
		Vector3 playerPos = transform.position;

		float offsetX = Random.Range(-10f, 10f);
		float offsetZ = Random.Range(-10f, 10f);

		return new Vector3(playerPos.x + offsetX,
							playerPos.y,
							playerPos.z + offsetZ);
	}*/

	public void Complete()
	{
		isSpawn = false;

        foreach (GameObject enemy in enemyList)
        {
            if (enemy != null)
                Destroy(enemy);
        }
    }
}
