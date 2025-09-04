using UnityEngine;

public class GravityManager : MonoBehaviour
{
    public static GravityManager Instance { get; private set; }

    private float gravityValue = -9.81f;
    private float gravityAngle = 0f;

    public bool isGravity { get; private set; } = false; // 현재 중력 진행중인지를 파악


    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void GravityChange(bool isLeft)
    {
        gravityAngle = isLeft ? -90f : 90f;
        SetGravity();
    }

    private void SetGravity()
    {
        Quaternion rot = Quaternion.AngleAxis(gravityAngle, PlayerController.Instance.transform.up); // 플레이어의 현재 윗방향 축을 기준으로 좌/우 회전
        Vector3 gravityDir = rot * PlayerController.Instance.transform.forward; // 플레이어의 정면 방향 축을 적용

        Physics.gravity = gravityDir.normalized * Mathf.Abs(gravityValue);
    }

    public void GravityCheck(bool isStart)
    {
        isGravity = isStart;
    }
}
