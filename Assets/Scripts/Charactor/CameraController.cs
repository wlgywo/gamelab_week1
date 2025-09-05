using UnityEngine;

public class CameraController : MonoBehaviour
{

    private void FixedUpdate()
    {
        Vector2 dir = InputManager.Instance.GetMoveDirNormalized();


    }
}
