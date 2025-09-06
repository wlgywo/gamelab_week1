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

            int ran = Random.Range(0, spawners.Length);
            spawners[ran].StartSpawn();
        }
    }

    public void EraseEnemy(MapDirect dir)
    {
        spawners[(int)dir].EraseEnemy();
    }
}
