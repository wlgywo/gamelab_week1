using UnityEngine;
using System.Collections;

public class BossDoorScript : MonoBehaviour
{
    public GameObject bossAi;

    public Transform leftDoor;
    public Transform rightDoor;
    public Light leftLight1;
    public Light leftLight2;
    public Light rightLight1;
    public Light rightLight2;
    public float openDistance = 1.5f;
    public float openSpeed = 2.0f;

    private Vector3 leftDoorClosedPosition;
    private Vector3 rightDoorClosedPosition;

    public bool isRightSideClear = false;
    public bool isLeftSideClear = false;
    public bool doorsAreOpen = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        leftDoorClosedPosition = leftDoor.localPosition;
        rightDoorClosedPosition = rightDoor.localPosition;
        rightLight1.enabled = false;
        leftLight1.enabled = false;
        rightLight2.enabled = false;
        leftLight2.enabled = false;
        CallDoorClose();
    }

    // Update is called once per frame
    void Update()
    {
        if (isRightSideClear)
        {
            rightLight1.enabled = true;
            rightLight2.enabled = true;
        }
        
        if (isLeftSideClear)
        {
            leftLight1.enabled = true;
            leftLight2.enabled = true;
        }

        if(isRightSideClear && isLeftSideClear)
        {
            doorsAreOpen = true;
        }
    }

    private IEnumerator OpenDoors()
    {
        Vector3 leftDoorOpenPosition = leftDoorClosedPosition + new Vector3(0, 0, openDistance);
        Vector3 rightDoorOpenPosition = rightDoorClosedPosition - new Vector3(0, 0, openDistance);

        while (Vector3.Distance(leftDoor.localPosition, leftDoorOpenPosition) > 0.01f)
        {
            leftDoor.localPosition = Vector3.Lerp(leftDoor.localPosition, leftDoorOpenPosition, Time.deltaTime * openSpeed);
            rightDoor.localPosition = Vector3.Lerp(rightDoor.localPosition, rightDoorOpenPosition, Time.deltaTime * openSpeed);
            yield return null;
        }

        leftDoor.localPosition = leftDoorOpenPosition;
        rightDoor.localPosition = rightDoorOpenPosition;
    }

    private IEnumerator CloseDoors()
    {
        while (Vector3.Distance(leftDoor.localPosition, leftDoorClosedPosition) > 0.01f)
        {
            leftDoor.localPosition = Vector3.Lerp(leftDoor.localPosition, leftDoorClosedPosition, Time.deltaTime * openSpeed);
            rightDoor.localPosition = Vector3.Lerp(rightDoor.localPosition, rightDoorClosedPosition, Time.deltaTime * openSpeed);
            yield return null;
        }

        leftDoor.localPosition = leftDoorClosedPosition;
        rightDoor.localPosition = rightDoorClosedPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && doorsAreOpen)
        {
            // 플레이어가 문에 닿았을 때의 동작

            if (bossAi != null)
            {
                bossAi.SetActive(true);
                //InGameManager.Instance.BossUI.SetActive(true);
            }
            CallDoorOpen();
        }
    }
    public void CallDoorOpen()
    {
        StopAllCoroutines();
        StartCoroutine(OpenDoors());
    }

    public void CallDoorClose()
    {
        StopAllCoroutines();
        StartCoroutine(CloseDoors());
    }
}
