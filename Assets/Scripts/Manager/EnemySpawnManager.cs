using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    // 레벨 관련
    private LevelScript levelScript;
    int level;

    // 어떤 적 소환할건지. 필요하면 배열로 바꾸기
    public GameObject enemyPrefab;

	private float curDelay = 0f;

	public bool isSpawn;

    private void Start()
    {
        levelScript = GetComponentInParent<LevelScript>();
        level = levelScript.level;
        StartCoroutine(SpawnEnemy());
    }
    private IEnumerator SpawnEnemy()
    {
        while (true)
        {
            int rand = Random.Range(0, 50);
            if (rand <= 1)
            {
                GameObject enemy = Instantiate(enemyPrefab, transform.position, transform.rotation);
                EnemyAI enemyScript = enemy.GetComponent<EnemyAI>();
                if (enemyScript != null)
                {
                    enemyScript.level = level;
                }
                yield break;
            }

            yield return new WaitForSeconds(3f);
        }
    }

    public void ReSpawnEnemy()
    {
        StartCoroutine(SpawnEnemy());
    }
}
