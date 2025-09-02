using TMPro;
using UnityEngine;

public class TimerScript : MonoBehaviour
{
    //[SerializeField] private TextMeshProUGUI countdownText;
    public float countdownTime = 10f;

    private float currentTime = 0;
    private bool isCounting = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //countdownText.gameObject.SetActive(false);

        InGameManager.Instance.countdownText.gameObject.SetActive(false);
    }

    // Update is called once per frame
  
    public void StartCountdown()
    {
        currentTime = countdownTime;
        isCounting = true;
        InGameManager.Instance.countdownText.gameObject.SetActive(true);
    }

    void Update()
    {
        if (isCounting)
        {
            currentTime -= Time.deltaTime;

            if(currentTime <= 0)
            {
                currentTime = 0;
                isCounting = false;
                InGameManager.Instance.countdownText.gameObject.SetActive(false);
                // 타이머 끝난 이후 처리
            }

            UpdateTimerUI();
        }
    }


    void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        InGameManager.Instance.countdownText.text = string.Format("{0}:{1:00}", minutes, seconds);
    }
}
