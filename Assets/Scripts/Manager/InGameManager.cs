using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameManager : MonoBehaviour
{
    public static InGameManager Instance { get; private set; }

    /*[SerializeField] public TextMeshProUGUI countdownText;

   [SerializeField] public GameObject levelUp;
    [SerializeField] public GameObject gameoverUI;
    [SerializeField] public GameObject gameClearUI;
    [SerializeField] public GameObject BossUI;
    */
    [SerializeField] public GameObject playerUI;

    [SerializeField] public Transform marbleUITrans;
    [SerializeField] public GameObject marbleUIPrefabs;

    private List<Image> marbleImages = new List<Image>();

    public Marble[] marbles;
    int cnt = 0;
    /*public bool isLevelUp = false;

    public bool gameOver { get; private set; } = false;


    public float repairSpeed { get; private set; }
    private float upgradeRepairSpeed = 5f;*/

    private void Awake()
    {
        if(Instance == null) Instance = this;

        playerUI.SetActive(true);
    }

    public void SetMarbleUI(Color color, int index)
    {
        marbles[cnt++].SetIndex(index);
        Image marble = Instantiate(marbleUIPrefabs, marbleUITrans).GetComponent<Image>();
        marble.color = color;
        marbleImages.Add(marble);
    }

    public void MarbleHit(int damage, int index)
    {
        marbles[index].Damage(damage);
    }

    public void SetMarbleUIHp(float hp, int index)
    {
        marbleImages[index].fillAmount = hp;
    }

    /*public void LevelUp()
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
    }*/
}
