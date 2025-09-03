using UnityEngine;

public class CheckRepairKitCollision : MonoBehaviour
{
	[SerializeField] private TimerScript timerScript;
	private GameObject curKit;

	void Start()
	{
		
	}

	// Update is called once per frame
	void Update()
	{
		if(curKit != null)
		{
			if(!curKit.activeSelf)
			{
				curKit = null;
                timerScript.ChanageCountdown(false);
            }
		}
	}

	private void OnTriggerEnter(Collider other)
	{
        if (other.CompareTag("RepairKit"))
		{
            timerScript.StartCountdown();
			curKit = other.gameObject;

        }
	}

	/*private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("RepairKit"))
		{
			timerScript.ChanageCountdown(false);

        }
	}*/
}
