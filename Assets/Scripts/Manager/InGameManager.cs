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
    [SerializeField] public GameObject specialUI;
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
    [SerializeField] public SpecialSO[] specialSO;
    [SerializeField] public Skill skillUIPrefabs;
    [SerializeField] public Transform skillUIPos;
    [SerializeField] public Special specialUIPrefabs;
    [SerializeField] public Transform specialUIPos;
    //public bool isLevelUp = false;
    public int curLevel = 0;
    public int curExp = 0;
    private int[] expLevel = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
        //{ 2, 4, 6, 8, 10, 12, 14, 16, 18, 20 };
    private List<Skill> skillList = new List<Skill>();
    private int[] skillLevels;
    private int skillUICount = 3; // LevelUp UI에 표시할 스킬 갯수
    private int completeCount = 0;

    private List<Special> specialList = new List<Special>();
    private bool[] specialChecks;
    private int specialCount = 0; // 특수 스킬 완료 횟수

    [Header("Skill Status")]
    [field: SerializeField] public float gravityTimer { get; private set; } = 5f;
    [field: SerializeField] public int power { get; private set; } = 10;
    [field: SerializeField] public float moveSpeed { get; private set; } = 7;
    [field: SerializeField] public int hp { get; private set; } = 150;
    [field: SerializeField] public float attackSpeed { get; private set; } = 1f;
    [field: SerializeField] public int ciritical { get; private set; } = 10; // 10퍼

    public int healValue = 20;

    public int maxHp { get; private set; } = 150;
    public float curGravityTimer { get; private set; } = 0;
    public bool GravityCoolTime => curGravityTimer <= 0;

    private const float UpgradeGravityTimer = 0.5f;
    private const int UpgradePower = 5;
    private const float UpgradeMoveSpeed = 1.5f;
    private const int UpgradeHp = 25;
    private const float UpgradeAttackSpeed = -0.15f;
    private const int UpgradeCiritical = 5;

    [Header("Special Status")]
    [field: SerializeField] public bool expTwice { get; private set; }
    [field: SerializeField] public bool knockBack { get; private set; }
    [field: SerializeField] public bool blood { get; private set; }
    [field: SerializeField] public bool quickMode { get; private set; } // 이건 좀 달라질 가능성 농후
    /*
    public bool gameOver { get; private set; } = false;

    public float repairSpeed { get; private set; }
    private float upgradeRepairSpeed = 5f;*/

    private void Awake()
    {
        if(Instance == null) Instance = this;

        playerUI.SetActive(true);
        skillLevels = new int[levelUpSO.Length];
        specialChecks = new bool[specialSO.Length];
    }

    private void Start()
    {
        UpdateVisual(StatusType.hp);
        UpdateVisual(StatusType.exp);
        UpdateVisual(StatusType.gravity);

        InputManager.Instance.OnLeftGravity += (a,b) => curGravityTimer = gravityTimer;
        InputManager.Instance.OnRightGravity += (a, b) => curGravityTimer = gravityTimer;
    }

    private void Update()
    {
        curGravityTimer -= Time.deltaTime;
        UpdateVisual(StatusType.gravity);
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
            if(curLevel < expLevel.Length - 1) curLevel++; // 만렙부턴 무한 반복

            LevelUp();
            Time.timeScale = 0;
        }

        UpdateVisual(StatusType.exp);//, (float)curExp / expLevel[curLevel]);
    }

    public void LevelUp()
    {
        //if (gameOver) return;

        levelUp.SetActive(true);
        //isLevelUp = true;
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
            skill.SetSkill(levelUpSO[num]);
            list.Add(num);
            skillList.Add(skill);

            cnt++;

            yield return null;
        }

        while (cnt == 0) // 모든 업글 완료
        {
            Skill skill = Instantiate(skillUIPrefabs, skillUIPos);
            skill.SetSkill(fallbackLevelUpSO[0]);
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
                power += UpgradePower;
                break;
            case SkillType.speed:
                moveSpeed += UpgradeMoveSpeed;
                break;
            case SkillType.hp:
                maxHp += UpgradeHp;
                hp += UpgradeHp;
                UpdateVisual(StatusType.hp);
                break;
            case SkillType.attackSpeed:
                attackSpeed += UpgradeAttackSpeed;
                PlayerController.Instance.SetAttackAnim(skillLevels[(int)skillType] * UpgradeAttackSpeed);
                break;
            case SkillType.critical:
                ciritical += UpgradeCiritical;
                break;
            case SkillType.fallback:
                hp += healValue;
                if (hp > maxHp) hp = maxHp;
                UpdateVisual(StatusType.hp);
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

    
    public void Special()
    {
        //if (gameOver) return;

        specialUI.SetActive(true);

        StartCoroutine(SpecialCoroutine());

        StartCoroutine(OpenMouse());
    }

    private IEnumerator SpecialCoroutine()
    {
        int cnt = 0;
        List<int> list = new List<int>();
        int num = -1;

        int counting = specialSO.Length - specialCount;
        if (counting > skillUICount) counting = skillUICount; // 아직 완료해야할 갯수가 많다면 이렇게 진행

        while (cnt < counting) // 현재 만렙이 아닌 구간만
        {
            num = Random.Range(0, specialSO.Length);

            // 이미 활성화했거나, 리스트에 뽑혔다면 
            if (specialChecks[num] || list.Contains(num))
            {
                continue; // 다시 뽑기
            }

            Special special = Instantiate(specialUIPrefabs, specialUIPos);
            special.SetSpecial(specialSO[num]);
            list.Add(num);
            specialList.Add(special);

            cnt++;

            yield return null;
        }
    }

    public void SpecialComplete(SpecialType specialType)
    {
        switch (specialType)
        {
            case SpecialType.partner:
                PlayerController.Instance.ActivePartner();
                break;
            case SpecialType.expTwice:
                expTwice = true;
                break;
            case SpecialType.knockBack:
                knockBack = true;
                break;
            case SpecialType.blood:
                blood = true;
                break;
            case SpecialType.quickMode:
                quickMode = true; 
                break;
            
        }

        foreach (var s in specialList)
        {
            if (s != null) Destroy(s.gameObject);
        }

        skillList.Clear();

        specialChecks[(int)specialType] = true;

        Time.timeScale = 1;

        //RemoveLevelUpUI();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        specialUI.SetActive(false);
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
        //isLevelUp = false;
        levelUp.SetActive(false);
    }

    public void UpdateVisual(StatusType type)
    {
        switch(type)
        {
            case StatusType.hp:
                hpSlider.value = (float)hp/maxHp;
                break;
            case StatusType.exp:
                expSlider.value = (float)curExp/expLevel[curLevel];
                break;
            case StatusType.gravity:
                gravityUI.fillAmount = 1- curGravityTimer/gravityTimer;
                break;
            default:
                Debug.Log("치명적인 종류 오류");
                break;
        }
    }

    public void GetDamage(int damage)
    {
        Debug.Log("아얏");
        hp -= damage;
        UpdateVisual(StatusType.hp);

        if (hp < 0)
        {
            Debug.Log("게임 오버");
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
