using UnityEngine;

public class MineralHpBarBillboardScript : MonoBehaviour
{ 
    void LateUpdate()
    {
        // 카메라 바라보기
        transform.LookAt(Camera.main.transform);
        // 필요하다면 뒤집기
        transform.Rotate(0, 180, 0);
    }
}
