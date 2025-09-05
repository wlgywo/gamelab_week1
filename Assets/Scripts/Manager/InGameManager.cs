using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameManager : MonoBehaviour
{
    public static InGameManager Instance { get; private set; }

    [SerializeField] public TextMeshProUGUI countdownText;

    [SerializeField] public GameObject levelUp;
    [SerializeField] public GameObject gameoverUI;
    [SerializeField] public GameObject gameClearUI;
    [SerializeField] public GameObject BossUI;
    [SerializeField] public GameObject playerUI;
    

    [field: SerializeField] public KitBox kitBox { get; private set; }

    public bool isLevelUp = false;

    public bool gameOver { get; private set; } = false;


    public float repairSpeed { get; private set; }
    private float upgradeRepairSpeed = 5f;

    private void Awake()
    {
        if(Instance == null) Instance = this;

        playerUI.SetActive(true);
    }


    public void DropKitBox()
    {
        kitBox.transform.position = PostPlayerController.Instance.kitBoxPos.position;
        kitBox.transform.rotation = PostPlayerController.Instance.transform.rotation;
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
        StartCoroutine(OpenMouse());
    }

    IEnumerator OpenMouse()
    {
        yield return new WaitForSeconds(1.25f);
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
}
