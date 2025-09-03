using UnityEngine;

public class CheckRepairKitCollision : MonoBehaviour
{
	[SerializeField] private TimerScript timerScript;
	[SerializeField] private ParticleSystem[] particle;
	private GameObject curKit;
	private bool repairComplete;

	void Start()
	{
		
	}

	// Update is called once per frame
	void Update()
	{
		if(!repairComplete && curKit != null)
		{
			if(!curKit.activeSelf)
			{
				curKit = null;
                timerScript.ChanageCountdown(false);
            }
		}
	}

	public void RepairComplete()
	{
		repairComplete = true;
		foreach(var p in particle)
		{
			p.Stop();
		}
	}


	private void OnTriggerEnter(Collider other)
	{
        if (!repairComplete && other.CompareTag("RepairKit"))
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
