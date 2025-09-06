using UnityEngine;

public class ShopTrigger: MonoBehaviour
{
    public GameObject promptUI;

    public bool isPlayerNear = false;

    void Start()
    {
        promptUI.SetActive(false);  // 시작할 땐 꺼두기
        InputManager.Instance.OnShopOpen += InputManager_OnShopOpen; // 상점 여는 함수 등록
        InputManager.Instance.OnShopClose += InputManager_OnShopClose; // 상점 닫는 함수 등록
    }

    private void InputManager_OnShopOpen(object sender, System.EventArgs e)
    {
        if (isPlayerNear)
        {
            InGameManager.Instance.ShopOpen();
        }
    }

    private void InputManager_OnShopClose(object sender, System.EventArgs e)
    {
        InGameManager.Instance.ShopClose();
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
