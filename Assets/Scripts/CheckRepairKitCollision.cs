using UnityEngine;

public class CheckRepairKitCollision : MonoBehaviour
{
	[SerializeField] private TimerScript timerScript;

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
            timerScript.StartCountdown();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("RepairKit"))
		{
			timerScript.ChanageCountdown(true);

        }
	}
}
