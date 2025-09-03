using UnityEngine;
using System.Collections;

public class DoorScript : MonoBehaviour
{

    public Transform leftDoor;      
    public Transform rightDoor;     
    public float openDistance = 1.5f; 
    public float openSpeed = 2.0f;

    private Vector3 leftDoorClosedPosition;
    private Vector3 rightDoorClosedPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        leftDoorClosedPosition = leftDoor.localPosition;
        rightDoorClosedPosition = rightDoor.localPosition;

        CallDoorOpen();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /*private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CallDoorOpen();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CallDoorClose();
        }
    }*/

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
