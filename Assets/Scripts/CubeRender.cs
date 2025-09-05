using System.Collections.Generic;
using UnityEngine;

public class CubeRender : MonoBehaviour
{
    public Material[] materials;
    public Material[] marbleMaterials;

    public Renderer[] cubes;
    public Light[] lights;
    public Renderer[] marbles;
    public Light[] marbleLights;

    public int[] count;
    public int[] indexs;

    public Color[] colors;

    private void Awake()
    {
        count = new int[materials.Length];
        for (int i = 0; i < materials.Length; i++) count[i] = 9;

        for (int i = 0; i < 54; i++)
        {
            int ran = Random.Range(0, 6);
            if (count[ran] == 0)
            {
                i--;
                continue;
            }
            else
            {
                bool check = false;
                for (int j = i % 9; j < i; j += 9)
                {
                    if (indexs[j] == ran)
                    {
                        check = true;
                        break;
                    }
                }

                if (check)
                {
                    i--;
                    continue;
                }


                indexs[i] = ran;
                cubes[i].material = materials[ran];
                lights[i].color = colors[ran];
                count[ran]--;
            }
        }

        MarbleShuffle();
    }

    public void MarbleShuffle()
    {
        List<int> a = new List<int> { 0, 1, 2, 3, 4, 5 };

        for(int i=0; i< marbles.Length; i ++)
        {
            int ran = Random.Range(0, marbles.Length);
            int temp = a[ran];
            a[ran] = a[i];
            a[i] = temp;
        }

        for(int i=0; i< marbles.Length; i++)
        {
            marbles[i].material = marbleMaterials[a[i]];
            marbleLights[i].color = colors[a[i]];
            //marbles[i].GetComponent<Marble>().SetIndex(a[i]);
            InGameManager.Instance.SetMarbleUI(colors[a[i]], i);
        }
    }
}
