using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerScript : MonoBehaviour
{
    [SerializeField] private Slider slider;
    public float countdownTime = 30f;

    private float currentTime = 0;
    [SerializeField] private bool isCounting = false;

    private float curKitTimer = 0f;
    private float kitTimer = 3f; // 

    private void Start()
    {
        InGameManager.Instance.countdownText.gameObject.SetActive(false);
        slider.gameObject.SetActive(false);
    }

    public void StartCountdown()
    {
        if(slider.gameObject.activeSelf)
        {
            ChanageCountdown(true);
            return;
        }      

        currentTime = countdownTime;
        isCounting = true;

        //if()
        InGameManager.Instance.countdownText.gameObject.SetActive(true);
        slider.gameObject.SetActive(true);
    }

   public void ChanageCountdown(bool isKitRepair)
    {
        isCounting = isKitRepair;
    }

    public void CompleteCountdown()
    {
        isCounting = false;
        InGameManager.Instance.countdownText.gameObject.SetActive(false);
        slider.gameObject.SetActive(false);

        // 몬스터 전부 삭제, 방 열림
    }

    void Update()
    {
        if (isCounting)
        {
            currentTime -= Time.deltaTime;
            curKitTimer += Time.deltaTime;

            if (currentTime <= 0)
            {
                currentTime = 0;
                isCounting = false;
                InGameManager.Instance.countdownText.gameObject.SetActive(false);
                InGameManager.Instance.levelUp.gameObject.SetActive(true);
            }

            if(curKitTimer >= kitTimer)
            {
                Debug.Log("클리어 처리");
                CompleteCountdown();
            }
        }
        else
        {

            curKitTimer -= Time.deltaTime;
            currentTime += Time.deltaTime;

            if (currentTime > countdownTime)
            {
                currentTime = countdownTime;
            }

            if (curKitTimer < 0) curKitTimer = 0;
        }

        UpdateTimerUI();
    }


    void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        InGameManager.Instance.countdownText.text = string.Format("{0}:{1:00}", minutes, seconds);

        slider.value = curKitTimer / kitTimer;
    }
}
