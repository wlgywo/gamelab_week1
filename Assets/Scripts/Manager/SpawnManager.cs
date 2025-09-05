using UnityEngine;

public enum MapDirect
{ 
    up, down, left, right, forward, back,
}

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance {  get; private set; }

    private void Awake()
    {
        if(Instance == null) Instance = this;
    }
}
