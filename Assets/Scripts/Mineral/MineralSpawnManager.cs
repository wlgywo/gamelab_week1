using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.Rendering;

public class MineralSpawnManager : MonoBehaviour
{
    private LevelScript levelScript;
    [SerializeField] private GameObject[] mineralPrefabs;
    private int level;
    void Start()
    {
        levelScript = GetComponentInParent<LevelScript>();
        level = levelScript.level;
        StartCoroutine(SpawnMineral());
    }

    private IEnumerator SpawnMineral()
    {
        while (true)
        {
            int rand = Random.Range(0, 10);
            
            if(rand <= 1)
            {
                ChooseMineral();
                yield break;
            }

            yield return new WaitForSeconds(3f); // 3초 대기
        }
    }

    void ChooseMineral()
    {
         Instantiate(mineralPrefabs[GetWeightedIndex(1)], transform.position,transform.rotation);
    }

    private int GetWeightedIndex(int level)
    {
        // 레벨 기반 가중치 예시
        // 낮은 레벨일 때는 [70, 20, 8, 2]
        // 높은 레벨일 때는 [20, 30, 30, 20]
        int[] weights;

        if (level < 3) weights = new int[] { 70, 20, 8, 2 };
        else if (level < 5) weights = new int[] { 40, 30, 20, 10 };
        else weights = new int[] { 10, 20, 30, 40 };

        // 총합
        int sum = 0;
        foreach (int w in weights) sum += w;

        int rand = Random.Range(0, sum);
        int cumulative = 0;

        for (int i = 0; i < weights.Length; i++)
        {
            cumulative += weights[i];
            if (rand < cumulative) return i;
        }

        return 0;   // 오류로 못구했을때 제일 쓰레기 반환
    }

    public void ReSpawnMineral()
    {
        StartCoroutine(SpawnMineral());
    }
}
