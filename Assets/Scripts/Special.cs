using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Special : MonoBehaviour
{
    private Button button;
    public TextMeshProUGUI title;
    public TextMeshProUGUI desc;

    private SpecialType specialType;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    // 버튼 등록해서 레벨업 진행
    private void Start()
    {
        button.onClick.AddListener(() => {
            InGameManager.Instance.SpecialComplete(specialType);
        });
    }

    public void SetSpecial(SpecialSO specialSO)
    {
        title.text = specialSO.name;
        desc.text = specialSO.desc;
        specialType = specialSO.specialType;
    }
}
