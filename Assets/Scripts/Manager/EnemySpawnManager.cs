using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    // 레벨 관련
    private LevelScript levelScript;
    private int level;

    // 어떤 적 소환할건지. 필요하면 배열로 바꾸기
    public GameObject[] enemyPrefab;

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
                EnemyAI enemyScript;
                if (level <= 1)
                {
                    GameObject enemy = Instantiate(enemyPrefab[1], transform.position, transform.rotation);
                    enemyScript = enemy.GetComponent<EnemyAI>();
                }
                else if (level <= 3)
                {
                    GameObject enemy = Instantiate(enemyPrefab[2], transform.position, transform.rotation);
                    enemyScript = enemy.GetComponent<EnemyAI>();
                }
                else if (level <= 5)
                {
                    GameObject enemy = Instantiate(enemyPrefab[3], transform.position, transform.rotation);
                    enemyScript = enemy.GetComponent<EnemyAI>();
                }
                else if (level <= 7)
                {
                    GameObject enemy = Instantiate(enemyPrefab[4], transform.position, transform.rotation);
                    enemyScript = enemy.GetComponent<EnemyAI>();
                }
                else
                {
                    GameObject enemy = Instantiate(enemyPrefab[0], transform.position, transform.rotation);
                    enemyScript = enemy.GetComponent<EnemyAI>();
                }
                enemyScript.SetManager(this);
                if (enemyScript != null)
                {
                    enemyScript.level = level;
                    enemyScript.maxHp = level * 100;
                    enemyScript.hp = enemyScript.maxHp;
                    enemyScript.UpdateVisual();
                }
                yield break;
            }

            yield return new WaitForSeconds(3f);
        }
    }
    void SetRendererColor(Renderer r, Color color)
    {
        if (!r) return;
        var mat = r.material;                 // 이 인스턴스만 복제된 머티리얼 사용
        if (mat.HasProperty("_BaseColor"))    // URP/HDRP Lit
            mat.SetColor("_BaseColor", color);
        else if (mat.HasProperty("_Color"))   // Built-in Standard
            mat.SetColor("_Color", color);
    }

    public void ReSpawnEnemy()
    {
        if(!InGameManager.Instance.gameOver)
            StartCoroutine(SpawnEnemy());
    }
}
