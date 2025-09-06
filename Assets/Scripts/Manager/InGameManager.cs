using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum SliderType
{
    hp, exp
}

public class InGameManager : MonoBehaviour
{
    public static InGameManager Instance { get; private set; }

    /*[SerializeField] public TextMeshProUGUI countdownText;

    [SerializeField] public GameObject gameoverUI;
    [SerializeField] public GameObject gameClearUI;
    [SerializeField] public GameObject BossUI;
    */

    [Header("System UI")]
    [SerializeField] public GameObject playerUI;
    [SerializeField] public GameObject levelUp;
    [SerializeField] public Slider hpSlider;
    [SerializeField] public Slider expSlider;

    [Header("Marbles")]
    [SerializeField] public Transform marbleUITrans;
    [SerializeField] public GameObject marbleUIPrefabs;
    private List<Image> marbleImages = new List<Image>();
    public Marble[] marbles;
    private int marbleCnt = 0;

    [Header("Level Up")]
    [SerializeField] public LevelUpSO[] levelUpSO;
    [SerializeField] public Skill[] skillUIPrefabs;
    [SerializeField] public Transform skillUIPos;
    public bool isLevelUp = false;
    public int curLevel = 0;
    public int curExp = 0;
    public int[] expLevel = { 2, 4, 6, 8, 10, 12, 14, 16, 18, 20 };

    [Header("Skill Status")]
    [field: SerializeField] public float gravityTimer = 5f;

    private const float UpgradeGravityTimer = 0.5f;

    /*
    public bool gameOver { get; private set; } = false;

    public float repairSpeed { get; private set; }
    private float upgradeRepairSpeed = 5f;*/

    private void Awake()
    {
        if(Instance == null) Instance = this;

        playerUI.SetActive(true);
    }

    private void Start()
    {
        UpdateVisual(SliderType.hp, 1);
        UpdateVisual(SliderType.exp, 0);
    }

    public void SetMarbleUI(Color color, int index)
    {
        marbles[marbleCnt++].SetIndex(index);
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

    public void GetExp() // 몬스터마다 경험치가 달라도 재밌을듯
    {
        curExp++;
        if(curExp >= expLevel[curLevel])
        {
            curExp = 0;
            curLevel++;
            LevelUp();
            Time.timeScale = 0;
        }

        UpdateVisual(SliderType.exp, (float)curExp / expLevel[curLevel]);
    }

    public void LevelUp()
    {
        //if (gameOver) return;

        levelUp.SetActive(true);
        isLevelUp = true;
        StartCoroutine(LevelUpCoroutine());

        StartCoroutine(OpenMouse());
    }

    private IEnumerator LevelUpCoroutine()
    {
        int cnt = 0;
        List<int> list = new List<int>();
        int num = -1;

        while (cnt < 3) // 나중에 LevelUp 스크립트를 만들어 맥스 레벨을 찍으면 IngameManager에 카운팅해서 변경
        {
            num = Random.Range(0, levelUpSO.Length);

            // 이미 맥스 레벨이거나 리스트에 뽑혔다면 
            if (levelUpSO[num].maxlevel <= levelUpSO[num].curlevel || list.Contains(num))
            {
                continue; // 다시 뽑기
            }

            Skill skill = Instantiate(skillUIPrefabs[num], skillUIPos);
            skill.SetKill(levelUpSO[num]);
            list.Add(num);

            cnt++;

            yield return null;

        }
    }

    public void SkillUp(SkillType skillType)
    {
        switch(skillType)
        {
            case SkillType.gravity:
                gravityTimer -= UpgradeGravityTimer;
                break;
        }

        Time.timeScale = 1;

        RemoveLevelUpUI();
    }

    IEnumerator OpenMouse()
    {
        yield return new WaitForSecondsRealtime(1.25f);
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

    public void UpdateVisual(SliderType type, float value)
    {
        switch(type)
        {
            case SliderType.hp:
                hpSlider.value = value;
                break;
            case SliderType.exp:
                expSlider.value = value;
                break;
            default:
                Debug.Log("치명적인 종류 오류");
                break;
        }
    }

    /*
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
