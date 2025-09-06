using UnityEngine;

public class ShopTrigger: MonoBehaviour
{
    public GameObject promptUI;

    private bool isPlayerNear = false;

    void Start()
    {
        promptUI.SetActive(false);  // 시작할 땐 꺼두기
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            promptUI.SetActive(true);  // 플레이어 들어오면 UI 표시
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            promptUI.SetActive(false); // 플레이어 나가면 UI 끄기
        }
    }

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            // 여기서 상점 UI 띄우는 코드 실행
        }
    }
}
