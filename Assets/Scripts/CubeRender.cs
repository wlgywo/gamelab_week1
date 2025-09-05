using UnityEngine;

public class CubeRender : MonoBehaviour
{
    public Material[] materials;

    public Renderer[] cubes;
    public Light[] lights;

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
    }
}
