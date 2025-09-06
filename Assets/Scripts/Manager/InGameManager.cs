using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum StatusType
{
    hp, exp, gravity,
    fallback // 모든 스킬 강화가 끝나면 나옴
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
    [SerializeField] private Image gravityUI;

    [Header("Marbles")]
    [SerializeField] public Transform marbleUITrans;
    [SerializeField] public GameObject marbleUIPrefabs;
    private List<Image> marbleImages = new List<Image>();
    public Marble[] marbles;
    private int marbleCnt = 0;

    [Header("Level Up")]
    [SerializeField] public LevelUpSO[] levelUpSO;
    [SerializeField] public LevelUpSO[] fallbackLevelUpSO; // 모든 강화가 완료되면 나오는 선택지
    [SerializeField] public Skill skillUIPrefabs;
    [SerializeField] public Transform skillUIPos;
    public bool isLevelUp = false;
    public int curLevel = 0;
    public int curExp = 0;
    private int[] expLevel = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
        //{ 2, 4, 6, 8, 10, 12, 14, 16, 18, 20 };
    private List<Skill> skillList = new List<Skill>();
    private int[] skillLevels;
    private int skillUICount = 3; // LevelUp UI에 표시할 스킬 갯수
    private int completeCount = 0;


    [Header("Skill Status")]
    [field: SerializeField] public float gravityTimer { get; private set; } = 5f;
    [field: SerializeField] public int power { get; private set; } = 10;

    public int healValue = 20;

    private const float UpgradeGravityTimer = 0.7f;
    private const int UpgradePlayerPower = 10;
    
    
    /*
    public bool gameOver { get; private set; } = false;

    public float repairSpeed { get; private set; }
    private float upgradeRepairSpeed = 5f;*/

    private void Awake()
    {
        if(Instance == null) Instance = this;

        playerUI.SetActive(true);
        skillLevels = new int[levelUpSO.Length];
    }

    private void Start()
    {
        UpdateVisual(StatusType.hp, 1);
        UpdateVisual(StatusType.exp, 0);
        UpdateVisual(StatusType.gravity, 1);
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
            curExp -= expLevel[curLevel];
            if(curLevel !=  expLevel.Length) curLevel++; // 만렙부턴 무한 반복

            LevelUp();
            Time.timeScale = 0;
        }

        UpdateVisual(StatusType.exp, (float)curExp / expLevel[curLevel]);
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

        int counting = levelUpSO.Length - completeCount;
        if (counting > skillUICount) counting = skillUICount; // 아직 완료해야할 갯수가 많다면 이렇게 진행

        Debug.Log("카운팅 값 : " + counting + " / 완료한 갯수 : " + completeCount);

        while (cnt < counting) // 현재 만렙이 아닌 구간만
        {
            num = Random.Range(0, levelUpSO.Length);

            // 이미 맥스 레벨이거나 리스트에 뽑혔다면 
            if (levelUpSO[num].maxlevel <= skillLevels[num] || list.Contains(num))
            {
                continue; // 다시 뽑기
            }

            Skill skill = Instantiate(skillUIPrefabs, skillUIPos);
            skill.SetKill(levelUpSO[num]);
            list.Add(num);
            skillList.Add(skill);

            cnt++;

            yield return null;
        }

        while (cnt == 0) // 모든 업글 완료
        {
            Skill skill = Instantiate(skillUIPrefabs, skillUIPos);
            skill.SetKill(fallbackLevelUpSO[0]);
            list.Add(num);
            skillList.Add(skill);
            break;
        }
    }

    public void SkillUp(SkillType skillType)
    {
        bool isFallback = false; // 기본 스킬인지 체크

        switch(skillType)
        {
            case SkillType.gravity:
                gravityTimer -= UpgradeGravityTimer;
                break;
            case SkillType.power:
                power += UpgradePlayerPower;
                break;
            case SkillType.fallback:
                PlayerController.Instance.Heal();
                isFallback = true;
                break;
        }

        foreach(var s in skillList)
        {
            if (s != null) Destroy(s.gameObject);
        }

        if (!isFallback)
        {
            skillLevels[(int)skillType]++;
            if (skillLevels[(int)skillType] == levelUpSO[(int)skillType].maxlevel) completeCount++;
        }

        skillList.Clear();

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

    public void UpdateVisual(StatusType type, float value = 0)
    {
        switch(type)
        {
            case StatusType.hp:
                hpSlider.value = value;
                break;
            case StatusType.exp:
                expSlider.value = value;
                break;
            case StatusType.gravity:
                gravityUI.fillAmount = 1 - value;
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
