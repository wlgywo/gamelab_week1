using TMPro;
using UnityEngine;

public class InGameManager : MonoBehaviour
{
    public static InGameManager Instance { get; private set; }

    [SerializeField] public TextMeshProUGUI countdownText;

    private void Awake()
    {
        if(Instance == null) Instance = this;
    }
    // Awake -> Oneable > start >  > dis > destor

}
