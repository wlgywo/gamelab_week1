using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameManager : MonoBehaviour
{
    public static InGameManager Instance { get; private set; }

    [SerializeField] public TextMeshProUGUI countdownText;
    [SerializeField] public TextMeshProUGUI goldText;
    [SerializeField] public Button dronBuyBtn;

    [SerializeField] public GameObject levelUp;
    [SerializeField] public GameObject gameoverUI;
    [SerializeField] public GameObject gameClearUI;
    [SerializeField] public GameObject BossUI;
    [SerializeField] public GameObject playerUI;
    [SerializeField] public GameObject ShopUI;


    // 캐릭터 관련
    private int gold = 0;
    public bool isLevelUp = false;
    public bool gameOver { get; private set; } = false;

    [field: SerializeField] public KitBox kitBox { get; private set; }




    public float repairSpeed { get; private set; }
    private float upgradeRepairSpeed = 1.5f;

    private void Awake()
    {
        if(Instance == null) Instance = this;

        playerUI.SetActive(true);
        SetGoldText();
    }


    public void DropKitBox()
    {
        kitBox.transform.position = PlayerController.Instance.kitBoxPos.position;
        kitBox.transform.rotation = PlayerController.Instance.transform.rotation;
        kitBox.gameObject.SetActive(true);
    }
    public void GetKitBox()
    {
        kitBox.gameObject.SetActive(false);
    }
    public void LevelUp()
    {
        if (gameOver) return;

        levelUp.SetActive(true);
        isLevelUp = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void RemoveLevelUpUI()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isLevelUp = false;
        levelUp.SetActive(false);
    }

    public void UpgradeRepairSpeed()
    {
        repairSpeed += upgradeRepairSpeed;
    }

    public void GameOver()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        gameOver = true;
        gameoverUI.SetActive(true);
        BossUI.SetActive(false);
        playerUI.SetActive(false);
    }
    public void GameClear()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        gameOver = true;
        gameClearUI.SetActive(true);
        BossUI.SetActive(false);
        playerUI.SetActive(false);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void ShopOpen()
    {
        if (gameOver) return;
        ShopUI.SetActive(true);
        PlayerController.Instance.isShopOpen = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void ShopClose()
    {
        if (gameOver) return;
        ShopUI.SetActive(false);
        PlayerController.Instance.isShopOpen = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void SetGoldText()
    {
        goldText.text = "Gold : " + gold;
    }

    public void ChangeGold(int amount)
    {
        gold += amount;
        SetGoldText();
    }
}
