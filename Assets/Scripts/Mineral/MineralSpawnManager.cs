using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.Rendering;

public class MineralSpawnManager : MonoBehaviour
{
    private LevelScript levelScript;
    [SerializeField] private GameObject[] mineralPrefabs;
    private GameObject currentMineral;
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
            int rand = Random.Range(0, 50);
            
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
        currentMineral = Instantiate(mineralPrefabs[GetWeightedIndex(level)], transform.position,transform.rotation);
        MineralHpScript mineralHpScript = currentMineral.GetComponent<MineralHpScript>();
        mineralHpScript.SetManager(this);
    }

    private int GetWeightedIndex(int level)
    {
        // 레벨 기반 가중치 예시
        // 낮은 레벨일 때는 [70, 20, 8, 2]
        // 높은 레벨일 때는 [20, 30, 30, 20]
        int[] weights;

        if (level < 2) weights = new int[] { 70, 29, 1, 0 };
        else if (level < 4) weights = new int[] { 40, 40, 15, 5 };
        else if (level < 6) weights = new int[] { 10, 50, 30, 10 };
        else weights = new int[] { 0, 30, 40, 30 };

        // 총합
        int sum = 0;
        foreach (int w in weights) sum += w;

        int rand = Random.Range(0, sum);
        int cumulative = 0;

        for (int i = 0; i < weights.Length; i++)
        {
            cumulative += weights[i];
            if (rand <= cumulative) return i;
        }

        return 0;   // 오류로 못구했을때 제일 쓰레기 반환
    }

    public void ReSpawnMineral()
    {
        if (!InGameManager.Instance.gameOver)
            StartCoroutine(SpawnMineral());
    }
}
