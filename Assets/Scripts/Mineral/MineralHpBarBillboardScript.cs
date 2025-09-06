using UnityEngine;

public class MineralHpBarBillboardScript : MonoBehaviour
{
    public Transform target;          // Mineral Blue 오브젝트

    void LateUpdate()
    {
        if (target == null) return;

        // Mineral 머리 위 위치
        target = GetComponentInParent<MineralSpawnManager>().transform;

        Quaternion targetRotation = target.rotation;

        // 카메라 쪽으로 조금
        Quaternion rotation = Quaternion.AngleAxis(180, transform.forward);
        // 2. Rigidbody의 회전 적용
        transform.rotation = target.rotation * rotation;

        // UI(Canvas)는 항상 카메라 쪽을 바라봐야 함
        transform.forward = Camera.main.transform.forward;
    }
}
