using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameManager : MonoBehaviour
{
    public static InGameManager Instance { get; private set; }

    [SerializeField] public TextMeshProUGUI countdownText;
    [SerializeField] public GameObject levelUp;
    [SerializeField] public Image levelUpImage;
    [SerializeField] public TextMeshProUGUI levelUpText1;   // 어떤 능력 업그레이드 할지 정하기.
    [SerializeField] public TextMeshProUGUI levelUpText2;   // 어떤 능력 업그레이드 할지 정하기.
    [SerializeField] public TextMeshProUGUI levelUpText3;   // 어떤 능력 업그레이드 할지 정하기.

    private void Awake()
    {
        if(Instance == null) Instance = this;
    }
}
