using UnityEngine;

public class BossRoomEnter : MonoBehaviour
{

    [SerializeField] private Transform bossDoor;   // Hierarchy에서 BossDoor 할당
    [SerializeField] private Transform bossDoor2;  // Hierarchy에서 BossDoor2 할당
    [SerializeField] private float moveDistance = 10f;
    [SerializeField] private float duration = 3f;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player")){
            InGameManager.Instance.BossUI.SetActive(true);
            InGameManager.Instance.Boss.SetActive(true);
        }

    }

    private System.Collections.IEnumerator OpenDoor(Transform door, Vector3 direction)
    {
        Vector3 startPos = door.position;
        Vector3 targetPos = startPos + direction * moveDistance;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            door.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
    }
}
