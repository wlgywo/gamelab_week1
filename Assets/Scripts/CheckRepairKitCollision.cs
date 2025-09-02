using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		
	}

	// Update is called once per frame
	void Update()
	{
		
	}

	void OnTriggerEnter(Collider other)
	{
        if (other.CompareTag("RepairKit"))
		{
			GetComponent<TimerScript>().StartCountdown();
		}
	}

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("RepairKit"))
        {
            GetComponent<TimerScript>().StopCountdown();
        }
    }

}
