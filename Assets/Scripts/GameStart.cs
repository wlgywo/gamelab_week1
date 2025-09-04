using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStart : MonoBehaviour
{
    private void Awake()
    {
        Physics.gravity = new Vector3(0, -9.81f, 0);
    }


    public void Gamestart()
    {
        SceneManager.LoadScene(1);
    }
}
