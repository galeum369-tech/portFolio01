using UnityEngine;

/// <summary>
/// PlayerLockOnV2
/// 
/// [역할]
/// - 락온 대상 탐색 / 유지 / 해제
/// - 거리 / 시야각 / 사망 조건 관리
/// - PlayerController / Camera / UI에 "타겟 정보만" 제공
/// 
/// ⚠ 카메라 직접 제어 ❌
/// ⚠ 회전 직접 제어 ❌
/// ⚠ UI 직접 제어 ❌
/// </summary>
public class PlayerLockOnV2 : MonoBehaviour
{
    [Header("LockOn 설정")]
    [SerializeField] float lockOnRange = 15f;
    [SerializeField] float lockOnViewAngle = 120f;

    [Tooltip("LockOnTarget 전용 레이어")]
    [SerializeField] LayerMask lockOnTargetLayer;

    PlayerControllerV2 controller;

    public bool IsLockOn { get; private set; }
    public Transform CurrentTarget { get; private set; }

    void Awake()
    {
        controller = GetComponent<PlayerControllerV2>();

        if (controller == null)
            Debug.LogError("[LockOnV2] PlayerControllerV2를 찾지 못했습니다.", this);
    }

    void OnEnable()
    {
        InputManager.OnLockOn += ToggleLockOn;
    }

    void OnDisable()
    {
        InputManager.OnLockOn -= ToggleLockOn;
    }

    void Update()
    {
        if (!IsLockOn || CurrentTarget == null)
            return;

        // 1️⃣ 사망 체크
        MonsterCore monster = CurrentTarget.GetComponentInParent<MonsterCore>();
        if (monster == null || monster.IsDead)
        {
            Debug.Log("[LockOnV2] 타겟 사망 → 락온 해제");
            ReleaseLockOn();
            return;
        }

        // 2️⃣ 거리 체크
        float dist = Vector3.Distance(transform.position, CurrentTarget.position);
        if (dist > lockOnRange)
        {
            Debug.Log("[LockOnV2] 거리 초과 → 락온 해제");
            ReleaseLockOn();
        }
    }

    /*───────────────────────────────*
     * 락온 토글
     *───────────────────────────────*/
    void ToggleLockOn()
    {
        if (IsLockOn)
        {
            ReleaseLockOn();
            return;
        }

        Transform target = FindNearestTarget();
        if (target == null)
        {
            Debug.Log("[LockOnV2] 락온 대상 없음");
            return;
        }

        CurrentTarget = target;
        IsLockOn = true;

        controller?.SetLockOn(true, CurrentTarget);

        // 월드용 타겟 표시 (UI 아님)
        target.GetComponent<LockOnTarget>()?.Show();

        Debug.Log($"[LockOnV2] 락온 ON : {target.name}");
    }

    void ReleaseLockOn()
    {
        if (CurrentTarget != null)
            CurrentTarget.GetComponent<LockOnTarget>()?.Hide();

        CurrentTarget = null;
        IsLockOn = false;

        controller?.SetLockOn(false, null);

        Debug.Log("[LockOnV2] 락온 OFF");
    }

    /*───────────────────────────────*
     * 타겟 탐색
     *───────────────────────────────*/
    Transform FindNearestTarget()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            lockOnRange,
            lockOnTargetLayer
        );

        float minDist = float.MaxValue;
        Transform nearest = null;

        foreach (var hit in hits)
        {
            // LockOnTarget은 "락온 기준점" 역할
            LockOnTarget target = hit.GetComponent<LockOnTarget>();
            if (target == null)
                continue;

            MonsterCore monster = target.GetComponentInParent<MonsterCore>();
            if (monster == null || monster.IsDead)
                continue;

            Vector3 dir = target.transform.position - transform.position;
            float angle = Vector3.Angle(transform.forward, dir.normalized);

            if (angle > lockOnViewAngle * 0.5f)
                continue;

            float dist = dir.magnitude;
            if (dist < minDist)
            {
                minDist = dist;
                nearest = target.transform;
            }
        }

        return nearest;
    }

    /*───────────────────────────────*
     * Gizmo
     *───────────────────────────────*/
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lockOnRange);

        Vector3 left = Quaternion.Euler(0, -lockOnViewAngle * 0.5f, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, lockOnViewAngle * 0.5f, 0) * transform.forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + left * lockOnRange);
        Gizmos.DrawLine(transform.position, transform.position + right * lockOnRange);
    }
}



