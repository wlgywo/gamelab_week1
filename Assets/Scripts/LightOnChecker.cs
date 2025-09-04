using UnityEngine;
using System.Collections;

public class LightOnChecker : MonoBehaviour
{
    [SerializeField] private Light[] targetLight;
    [SerializeField] private float blinkInterval = 0.01f;

    private bool isLightOn = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (Light light in targetLight)
        {
            if (light != null)
            {
                light.enabled = false;
                isLightOn = false;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    private IEnumerator BlinkAndStayOn()
    {
         foreach (Light light in targetLight)
         {
                if (light != null)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        light.enabled = true;
                        yield return new WaitForSeconds(blinkInterval);

                        light.enabled = false;
                        yield return new WaitForSeconds(blinkInterval);
                    }

                    light.enabled = true;
                }
         }
    }

    public void CallLightOn()
    {
        if (!isLightOn)
        {
            isLightOn = true;
            StartCoroutine(BlinkAndStayOn());
        }
    }
}
