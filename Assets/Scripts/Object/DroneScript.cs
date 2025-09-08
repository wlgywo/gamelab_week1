using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneScript : MonoBehaviour
{
    public static DroneScript Instance { get; private set; }
    public int attackDamage = 10;
    public float retargetInterval = 3.0f;
    public LayerMask enemyMask;
    readonly HashSet<EnemyAI> inRange = new();
    [SerializeField] GameObject parentDrone;

    // 공격
    public Transform firePoint;          // 총구 위치(없으면 드론 위치 사용)
    public float laserThickness = 0.08f; // 큐브 두께
    public float laserLifetime = 0.05f;  // 시각효과 유지 시간
    private float lastFireTime = -999f;  // 내부 쿨다운 타임스탬프
    public EnemyAI current { get; private set; }

    void Awake() {
        if (Instance == null)
        {
            Instance = this;
        }
        parentDrone.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & enemyMask) == 0) return;
        var e = other.GetComponentInParent<EnemyAI>();
        if (e) inRange.Add(e);
    }

    void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & enemyMask) == 0) return;
        var e = other.GetComponentInParent<EnemyAI>();
        if (e) inRange.Remove(e);
    }

    IEnumerator Start()
    {
        var wait = new WaitForSeconds(retargetInterval);
        while (true)
        {
            AcquireTarget();
            Attack();
            wait = new WaitForSeconds(retargetInterval);
            yield return wait;
        }
    }

    void Attack()
    {
        // 1) 쿨다운/타깃 체크
        if (current == null) return;

        // 2) 시작점/목표점/방향 계산
        Vector3 start = firePoint ? firePoint.position : transform.position;

        // 콜라이더가 있으면 그 중심을 노리면 더 안정적
        var col = current.GetComponent<Collider>();
        Vector3 target = col ? col.bounds.center : current.transform.position;

        Vector3 dir = (target - start);
        float dist = dir.magnitude;
        if (dist < 0.01f) return;
        dir /= dist;

        // 3) 레이캐스트로 실제 충돌 지점/피해 적용
        RaycastHit hit;
        float maxDist = dist + 0.5f; // 약간 여유
        if (Physics.Raycast(start, dir, out hit, maxDist, enemyMask, QueryTriggerInteraction.Ignore))
        {
            dist = hit.distance; // 시각효과 길이도 충돌 지점까지만

            // 맞은 대상에 데미지
            var hitEnemy = hit.collider.GetComponentInParent<EnemyAI>();
            if (hitEnemy != null)
            {
                // EnemyAI에 이 함수가 있다고 가정
                hitEnemy.GetDroneDamage(attackDamage);
            }
        }
        else
        {
            // 레이가 아무 것도 안 맞으면, 타깃까지 시각효과만 표시
            // (원한다면 여기서 return 해도 됨)
        }

        // 4) 시각효과: 얇은 빨간 직사각형 큐브 생성 → 짧게 표시 후 파괴
        SpawnLaserVisual(start, dir, dist);

        // 5) 쿨다운 갱신
        lastFireTime = Time.time;
    }

    void SpawnLaserVisual(Vector3 start, Vector3 dir, float length)
    {
        // 큐브 생성
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "LaserBeam";
        go.layer = LayerMask.NameToLayer("Ignore Raycast"); // 자기 자신 레이캐스트 방해 방지
        var col = go.GetComponent<Collider>();
        if (col) Destroy(col); // 시각효과만 쓸 거라면 콜라이더 제거

        // 위치/회전/스케일: 시작~끝의 중간에 길이만큼 늘린 직사각형
        go.transform.SetPositionAndRotation(start + dir * (length * 0.5f), Quaternion.LookRotation(dir));
        go.transform.localScale = new Vector3(laserThickness, laserThickness, length);

        // 색상(URP/HDRP/Built-in 호환)
        var r = go.GetComponent<Renderer>();
        var m = r.material; // 이 인스턴스만
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", Color.red);
        else if (m.HasProperty("_Color")) m.SetColor("_Color", Color.red);

        // 발광(선택)
        if (m.HasProperty("_EmissionColor"))
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", Color.red * 2f);
        }

        // 아주 짧게만 표시
        Destroy(go, laserLifetime);
    }
    void AcquireTarget()
    {
        inRange.RemoveWhere(e => e == null || !e.gameObject.activeInHierarchy);

        float best = float.PositiveInfinity;
        EnemyAI bestE = null;
        var p = transform.position;

        foreach (var e in inRange)
        {
            float d = (e.transform.position - p).sqrMagnitude;
            if (d < best) { best = d; bestE = e; }
        }
        current = bestE;
    }

}
