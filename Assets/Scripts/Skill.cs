using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Skill : MonoBehaviour
{
    private Button button;
    public TextMeshProUGUI title;
    public TextMeshProUGUI desc;

    private SkillType skillType;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    // 버튼 등록해서 레벨업 진행
    private void Start()
    {
        button.onClick.AddListener(() => {
            InGameManager.Instance.SkillUp(skillType);
            });
    }

    public void SetKill(LevelUpSO levelUpSO)
    {
        title.text = levelUpSO.name;
        desc.text = levelUpSO.desc;
        skillType = levelUpSO.skillType;
    }
}
