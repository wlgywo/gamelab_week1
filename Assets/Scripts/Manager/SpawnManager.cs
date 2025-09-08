using NUnit.Framework;
using System.Collections;
using UnityEngine;

public enum MapDirect
{
    down, up, left, right, back, forward
}

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance {  get; private set; }

    [SerializeField] private Spawner[] spawners;
    public Spawner[] Spawners => spawners;

    [SerializeField] private float curSpawnTimer = 0;
    [SerializeField] public float spawnTimer { get; private set; } = 15f; // 새로운 땅 스폰 지정 시간
    [SerializeField] public float spawnDelay { get; private set; } = 3f; // 몬스터 스폰 시간

    public bool[] checkMarble { get; private set; } = new bool[6];// false면 해당 마블 파괴된 상태
    public bool bossGenerate = false;

    public int bossMarbleIndex = 0;
    public int destroyMarbleCount { get; private set; } = 0;

    private float upgradeTimer = 30f;

    private void Awake()
    {
        if(Instance == null) Instance = this;
    }

    private void Update()
    {
        if (InGameManager.Instance.GameEnd) return;

        if (!bossGenerate)
        {
            upgradeTimer -= Time.deltaTime;
            if (upgradeTimer < 0)
            {
                upgradeTimer = 30f;
                UpgradeSpawn();
            }
        }

        if (InGameManager.Instance.quickMode) curSpawnTimer -= Time.deltaTime * 2;
        else curSpawnTimer -= Time.deltaTime;
        if(curSpawnTimer <0)
        {
            curSpawnTimer = spawnTimer;

            /*int ran = Random.Range(0, spawners.Length);
            spawners[ran].StartSpawn();*/

            StartCoroutine(SpawnStart());
        }
    }

    private IEnumerator SpawnStart()
    {
        int ran = -1;
        while(true)
        {
            ran = Random.Range(0, spawners.Length);
            yield return null;

            if (!checkMarble[ran]) break;
        }
        spawners[ran].StartSpawn();
    }

    public void BossGenerate()
    {
        if (bossGenerate) return;

        bossGenerate = true;

        BossSetMarblePos();

        spawners[bossMarbleIndex].BossGenerate();
    }


    public void BossSetMarblePos()
    {
        int num = 0;
        foreach(var c in checkMarble)
        {
            if (c) num++;
        }
        if (num == checkMarble.Length) return;

        while (true)
        {
            bossMarbleIndex = Random.Range(0, spawners.Length);
            //yield return null;

            if (!checkMarble[bossMarbleIndex]) break;
        }
    }

    public void DestroyMarble(int index)
    {
        destroyMarbleCount++;
        checkMarble[index] = true;
        spawners[index].DestroyAI();

        if (destroyMarbleCount == spawners.Length) InGameManager.Instance.GameOver();
    }

    public int CurMarbleCount()
    {
        int num = 0;

        foreach (var b in checkMarble)
            if (b) num++;

        return num;
    }

    public void UpgradeSpawn()
    {
        spawnDelay -= 0.5f;
        spawnTimer -= 1.5f;
    }
    
    /*public void EraseEnemy(MapDirect dir)
    {
        spawners[(int)dir].EraseEnemy();
    }*/
}
