using UnityEngine;
using System.Collections;

public class LightOnChecker : MonoBehaviour
{
    [SerializeField] private Light[] targetLight;
    [SerializeField] private float blinkInterval = 0.01f;

    private bool isBlinking = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (Light light in targetLight)
        {
            if (light != null)
            {
                light.enabled = false;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            foreach (Light light in targetLight)
            {
                if (light != null)
                {
                    StartCoroutine(BlinkAndStayOn());
                }
            }
        }
    }

    private IEnumerator BlinkAndStayOn()
    {
        
        isBlinking = true;

        foreach (Light light in targetLight)
        {
            if (light != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    light.enabled = true;
                    yield return new WaitForSeconds(blinkInterval);

                    light.enabled = false;
                    yield return new WaitForSeconds(blinkInterval);
                }

                light.enabled = true;
                isBlinking = false;
            }
        }
        
    }
}
