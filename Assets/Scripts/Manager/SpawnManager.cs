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
    [SerializeField] private float spawnTimer = 15f; // 임의

    public bool[] checkMarble { get; private set; } = new bool[6];// false면 해당 마블 파괴된 상태

    private void Awake()
    {
        if(Instance == null) Instance = this;
    }

    private void Update()
    {
        curSpawnTimer -= Time.deltaTime;
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

    public void DestroyMarble(int index)
    {
        checkMarble[index] = true;
    }

    /*public void EraseEnemy(MapDirect dir)
    {
        spawners[(int)dir].EraseEnemy();
    }*/
}
