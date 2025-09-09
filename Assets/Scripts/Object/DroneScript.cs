using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneScript : MonoBehaviour
{
    public static DroneScript Instance { get; private set; }
    public int attackDamage = 10;
    public float retargetInterval = 3.0f;
    public LayerMask enemyMask;
    readonly HashSet<IEnemyTarget> inRange = new();
    readonly HashSet<BossAI> inRangeBoss = new();
    [SerializeField] GameObject parentDrone;

    // 공격
    public Transform firePoint;          // 총구 위치(없으면 드론 위치 사용)
    public float laserThickness = 0.08f; // 큐브 두께
    public float laserLifetime = 0.05f;  // 시각효과 유지 시간
    private float lastFireTime = -999f;  // 내부 쿨다운 타임스탬프
    public IEnemyTarget current { get; private set; }   // EnemyAI 대신 인터페이스로 변경

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
        var target = other.GetComponentInParent<IEnemyTarget>();
        if (target != null) inRange.Add(target);
    }

    void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & enemyMask) == 0) return;
        var target = other.GetComponentInParent<IEnemyTarget>();
        if (target != null) inRange.Remove(target);
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
    public void UnregisterTarget(IEnemyTarget target)
    {
        inRange.Remove(target);
    }
    void Attack()
    {
        if (current == null) return;

        Vector3 start = firePoint ? firePoint.position : transform.position;

        var col = (current as MonoBehaviour).GetComponent<Collider>();
        Vector3 target = col ? col.bounds.center : (current as MonoBehaviour).transform.position;

        Vector3 dir = (target - start);
        float dist = dir.magnitude;
        if (dist < 0.01f) return;
        dir /= dist;

        RaycastHit hit;
        float maxDist = dist + 0.5f;
        if (Physics.Raycast(start, dir, out hit, maxDist, enemyMask, QueryTriggerInteraction.Ignore))
        {
            dist = hit.distance;
            Debug.Log("누구 때릴라 함");

            var targets = hit.collider.GetComponentInParent<IEnemyTarget>();
            if (targets != null)
            {
                targets.OnDamagedFromDrone(attackDamage); // EnemyAI든 BossAI든 알아서 호출됨
            }
        }

        SpawnLaserVisual(start, dir, dist);
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
        inRange.RemoveWhere(e => e == null || !(e as MonoBehaviour).gameObject.activeInHierarchy);

        float best = float.PositiveInfinity;
        IEnemyTarget bestTarget = null;
        var p = transform.position;

        foreach (var e in inRange)
        {
            float d = ((e as MonoBehaviour).transform.position - p).sqrMagnitude;
            if (d < best) { best = d; bestTarget = e; }
        }
        current = bestTarget;
    }

}
