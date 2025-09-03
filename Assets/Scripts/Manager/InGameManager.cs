using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameManager : MonoBehaviour
{
    public static InGameManager Instance { get; private set; }

    [SerializeField] public TextMeshProUGUI countdownText;

    [SerializeField] public GameObject levelUp;

    [field: SerializeField] public KitBox kitBox { get; private set; }

    private void Awake()
    {
        if(Instance == null) Instance = this;
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
        levelUp.SetActive(true);
    }
    public void RemoveLevelUpUI()
    {
        levelUp.SetActive(false);
    }
}
