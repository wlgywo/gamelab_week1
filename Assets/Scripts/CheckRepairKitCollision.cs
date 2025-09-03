using UnityEngine;

public class CheckRepairKitCollision : MonoBehaviour
{
	[SerializeField] private TimerScript timerScript;
	[SerializeField] private ParticleSystem[] particle;
	[SerializeField] private DoorScript[] doors;
	[SerializeField] private LightOnChecker lightOnChecker;
	private GameObject curKit;
	private bool repairComplete;
	private bool playerOn;

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

		foreach(var d in doors)
		{
			d.CallDoorOpen();
        }
	}


	private void OnTriggerEnter(Collider other)
	{
        if (!repairComplete && other.CompareTag("RepairKit"))
		{
            if (!playerOn)
			{
                foreach (var d in doors)
                {
                    d.CallDoorClose();
                    playerOn = true;
                }
                lightOnChecker.CallLightOn();
            }

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
