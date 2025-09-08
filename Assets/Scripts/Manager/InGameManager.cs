using NUnit.Framework;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameManager : MonoBehaviour
{
    public static InGameManager Instance { get; private set; }

    [SerializeField] public TextMeshProUGUI countdownText;
    [SerializeField] public TextMeshProUGUI goldText;
    [SerializeField] public TextMeshProUGUI redMineralCountText;
    [SerializeField] public TextMeshProUGUI orangeMineralCountText;
    [SerializeField] public TextMeshProUGUI blueMineralCountText;
    [SerializeField] public TextMeshProUGUI purpleMineralCountText;
    [SerializeField] public Button redPotionBtn;
    [SerializeField] public Button droneBuyBtn;
    [SerializeField] public Button droneDamageBtn;
    [SerializeField] public Button droneAttackSpeedBtn;
    [SerializeField] public Button sellRedMineralBtn;
    [SerializeField] public Button sellOrangeMineralBtn;
    [SerializeField] public Button sellBlueMineralBtn;
    [SerializeField] public Button sellPurpleMineralBtn;

    [SerializeField] public GameObject levelUp;
    [SerializeField] public GameObject gameoverUI;
    [SerializeField] public GameObject gameClearUI;
    [SerializeField] public GameObject BossUI;
    [SerializeField] public GameObject playerUI;
    [SerializeField] public GameObject ShopUI;
    [SerializeField] public GameObject MineralUI;
    [SerializeField] public GameObject RecallUI;

    // 캐릭터 관련
    public bool isLevelUp = false;
    public bool gameOver { get; private set; } = false;

    // 소지품 관련
    private int gold = 0;
    private bool haveDrone = false;
    private int redMineralCount = 0;
    private int orangeMineralCount = 0;
    private int blueMineralCount = 0;
    private int purpleMineralCount = 0;

    // 상점 관련
    private int redPotionPrice = 30;
    private int dronePrice = 500;
    private int droneDamageUpPrice = 700;
    private int droneAttackSpeedUpPrice = 1000;
    private int redMineralPrice = 1;
    private int orangeMineralPrice = 10;
    private int blueMineralPrice = 50;
    private int purpleMineralPrice = 100;
    private int healRate = 10;

    [field: SerializeField] public KitBox kitBox { get; private set; }




    public float repairSpeed { get; private set; }
    private float upgradeRepairSpeed = 1.5f;

    private void Awake()
    {
        if(Instance == null) Instance = this;

        playerUI.SetActive(true);
        SetGoldText();
        
        redMineralCountText.text = "X " + redMineralCount;
        orangeMineralCountText.text = "X " + orangeMineralCount;
        blueMineralCountText.text = "X " + blueMineralCount;
        purpleMineralCountText.text = "X " + purpleMineralCount;
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
        StartCoroutine(LevelUpDelay());
    }

    IEnumerator LevelUpDelay()
    {
        yield return new WaitForSeconds(1.5f);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        PlayerController.Instance.isLevelUpOpen = false;
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
        SetShop();
        PlayerController.Instance.isShopOpen = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;


    }

    public void ShopClose()
    {
        if (gameOver) return;
        if (PlayerController.Instance.isLevelUpOpen) return;
        ShopUI.SetActive(false);
        PlayerController.Instance.isShopOpen = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SetGoldText()
    {
        goldText.text = "Gold : " + gold;
    }

    private void ChangeGold(int amount)
    {
        gold += amount;
        SetGoldText();
        SetShop();
    }

    public void SetShop()
    {
        // 빨간 물약
        if (gold < redPotionPrice) redPotionBtn.interactable = false;
        else redPotionBtn.interactable = true;

        // 드론
        if (gold < dronePrice || haveDrone) droneBuyBtn.interactable = false;
        else droneBuyBtn.interactable = true;

        // 드론 데미지
        if(gold < droneDamageUpPrice) droneDamageBtn.interactable = false;
        else droneDamageBtn.interactable = true;

        // 드론 공속
        if (gold < droneAttackSpeedUpPrice) droneAttackSpeedBtn.interactable = false;
        else droneAttackSpeedBtn.interactable = true;

        // 빨간 미네랄
        if (redMineralCount <= 0) sellRedMineralBtn.interactable = false;
        else sellRedMineralBtn.interactable = true;

        // 주황 미네랄
        if (orangeMineralCount <= 0) sellOrangeMineralBtn.interactable = false;
        else sellOrangeMineralBtn.interactable = true;

        // 파랑 미네랄
        if (blueMineralCount <= 0) sellBlueMineralBtn.interactable = false;
        else sellBlueMineralBtn.interactable = true;

        // 보라 미네랄
        if (purpleMineralCount <= 0) sellPurpleMineralBtn.interactable = false;
        else sellPurpleMineralBtn.interactable = true;
    }

    public void BuyRedPotion()
    {
        // 플레이어 체력 회복
        PlayerController.Instance.healHp(healRate);
        ChangeGold(-redPotionPrice);
    }
    public void BuyDrone()
    {
        // 드론 생성
        ChangeGold(-dronePrice);
    }
    public void BuyDroneDamageUp()
    {
        // 드론 데미지 업
        ChangeGold(-droneDamageUpPrice);
    }
    public void BuyDroneSpeedUp()
    {
        // 드론 공속 업
        ChangeGold(-droneAttackSpeedUpPrice);
    }
    public void SellRedMineral()
    {
        ChangeGold(redMineralPrice * redMineralCount);
        redMineralCount = 0;
        ChangeMineralText();
    }
    public void SellOrangeMineral()
    {
        ChangeGold(orangeMineralPrice * orangeMineralCount);
        orangeMineralCount = 0;
        ChangeMineralText();
    }
    public void SellBlueMineral()
    {
        ChangeGold(blueMineralPrice * blueMineralCount);
        blueMineralCount = 0;
        ChangeMineralText();
    }
    public void SellPurpleMineral()
    {
        ChangeGold(purpleMineralPrice * purpleMineralCount);
        purpleMineralCount = 0;
        ChangeMineralText();
    }
    public void ToggleMineralCount()
    {

    }
    public void AddRedMineral()
    {
        redMineralCount++;
        ChangeMineralText();
    }
    public void AddOrangeMineral()
    {
        orangeMineralCount++;
        ChangeMineralText();
    }
    public void AddBlueMineral()
    {
        blueMineralCount++;
        ChangeMineralText();
    }
    public void AddPurpleMineral()
    {
        purpleMineralCount++;
        ChangeMineralText();
    }

    private void ChangeMineralText()
    {
        redMineralCountText.text = "X " + redMineralCount;
        orangeMineralCountText.text = "X " + orangeMineralCount;
        blueMineralCountText.text = "X " + blueMineralCount;
        purpleMineralCountText.text = "X " + purpleMineralCount;
    }

    public void Recall()
    {
        RecallUI.SetActive(true);
    }

    public void RecallFinish()
    {
        RecallUI.SetActive(false);
    }

}
