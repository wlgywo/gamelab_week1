using UnityEngine;
using UnityEngine.UI;

public class MineralHpScript : MonoBehaviour
{
    [SerializeField] private Image hpFill;

    public void SetHealth(float current, float max)
    {
        hpFill.fillAmount = current / max;
    }

}
