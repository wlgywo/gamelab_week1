using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerScript : MonoBehaviour
{
    [SerializeField] private CheckRepairKitCollision repairKit;
    [SerializeField] private Slider slider;
    private float countdownTime = 10f;

    private float currentTime = 0;
    [SerializeField] private bool isCounting = false;

    //private float curKitTimer = 0f;
    //private float kitTimer = 30f; //  30초로 수정

    private bool isRun = false;

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

        isRun = true;
        currentTime = countdownTime - InGameManager.Instance.repairSpeed;
        isCounting = true;
        //curKitTimer = 0;

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
        isRun = false;
        isCounting = false;
        repairKit.RepairComplete();
        InGameManager.Instance.countdownText.gameObject.SetActive(false);
        slider.gameObject.SetActive(false);
        InGameManager.Instance.LevelUp();

        // 몬스터 전부 삭제, 방 열림
    }

    void Update()
    {
        if (!isRun) return;

        if (isCounting)
        {
            currentTime -= Time.deltaTime;
            //curKitTimer += Time.deltaTime;

            if (currentTime <= 0)
            {
                currentTime = 0;
                isCounting = false;
                InGameManager.Instance.countdownText.gameObject.SetActive(false);
                CompleteCountdown();
                //InGameManager.Instance.levelUp.gameObject.SetActive(true);
            }

            /*if(curKitTimer >= (kitTimer- InGameManager.Instance.repairSpeed))
            {
                Debug.Log("클리어 처리");
                CompleteCountdown();
            }*/
        }
        else
        {

            currentTime += Time.deltaTime;
            //curKitTimer -= Time.deltaTime;

            if (currentTime > countdownTime - InGameManager.Instance.repairSpeed)
            {
                currentTime = countdownTime - InGameManager.Instance.repairSpeed;
            }

            //if (curKitTimer < 0) curKitTimer = 0;
        }

        UpdateTimerUI();
    }


    void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);

        Debug.Log("초 : " + seconds);

        InGameManager.Instance.countdownText.text = string.Format("{0}:{1:00}", minutes, seconds);

        //slider.value = curKitTimer / (kitTimer- InGameManager.Instance.repairSpeed);
        float maxCount = countdownTime - InGameManager.Instance.repairSpeed;

        slider.value = (maxCount - currentTime) / maxCount;
    }
}
