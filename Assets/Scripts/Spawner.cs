using UnityEngine;

public class Spawner : MonoBehaviour
{
    public MapDirect mapDirect;
    public Marble marble { get; private set; }

    private float yOffset = 1.75f;

    [Header("Enemy Informations")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int enemyCount = 8; // 일단 max값 임의로
    private int curEnemyCount = 0;

    [Header("Spawn Settings")]
    private bool isSpawn = false;
    private float curTimer = 3f;
    private float cumulationTimer = 0f; // 누적 타이머
    [SerializeField] private float spawnDelay = 3f; // 스폰 타이머
    [SerializeField] private float spawnRange = 15f; // 스폰 반지름
    [SerializeField] private float spawnTimer = 15f; // 스폰 시간


    private void Awake()
    {
        marble = GetComponent<Marble>();
    }

    private void Update()
    {
        if(isSpawn)
        {
            if(cumulationTimer > spawnTimer)
            {
                isSpawn = false;
            }

            curTimer -= Time.deltaTime;
            cumulationTimer += Time.deltaTime;

            if (curTimer < 0 && curEnemyCount < enemyCount)
            {
                curTimer = spawnDelay;
                SpawnEnemy();
            }
        }
    }

    public void StartSpawn()
    {
        isSpawn = true;
        cumulationTimer = 0;
        curEnemyCount = 0;

        SpawnEnemy();
        curTimer = spawnDelay;
    }

    public void SpawnEnemy()
    {
        Debug.Log(mapDirect);
        Vector3 up = transform.up;

        Vector3 forward = Vector3.Cross(up, Vector3.right);
        if (forward.sqrMagnitude < 0.01f) // up이 Vector3.right와 거의 평행이면
            forward = Vector3.Cross(up, Vector3.forward);
        forward.Normalize();

        // 3. 임의의 각도
        float angle = Random.Range(0f, 360f);

        // 4. angle만큼 회전
        Quaternion rotation = Quaternion.AngleAxis(angle, up);
        Vector3 spawnDir = rotation * forward;

        // 5. 반지름 곱해서 위치 계산
        Vector3 spawnPos = transform.position - transform.up * yOffset + spawnDir * spawnRange;

        AI ai = Instantiate(enemyPrefab, spawnPos, transform.rotation).GetComponent<AI>();
        ai.SetMarble(marble.gameObject.transform, mapDirect);

        curEnemyCount++;
    }

    public void EraseEnemy()
    {
        enemyCount--;
    }
}
