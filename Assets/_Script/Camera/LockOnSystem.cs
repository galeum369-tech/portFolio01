using UnityEngine;
using UnityEngine.InputSystem;

public class LockOnSystem : MonoBehaviour
{
    public Transform lockOnTarget;   // Dummy/Target
    public bool IsLockOn { get; private set; }

    PlayerController player;

    [SerializeField] float lockOnRange = 15f;
    [SerializeField] float lockOnViewAngle = 120f; // 정면 기준
    [SerializeField] LayerMask monsterLayer;




    void Awake()
    {
        player = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (!IsLockOn || lockOnTarget == null)
            return;

        // 사망 시 해제
        MonsterCore monster = lockOnTarget.GetComponentInParent<MonsterCore>();
        if (monster == null || monster.IsDead)
        {
            ReleaseLockOn();
            return;
        }

        // 거리 초과 시 해제
        float dist = Vector3.Distance(
            transform.position,
            lockOnTarget.position
        );

        if (dist > lockOnRange)
        {
            ReleaseLockOn();
            return;
        }

        // 기존 회전 로직
        RotateToTarget();
    }

    void RotateToTarget()
    {
        Vector3 dir = lockOnTarget.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f)
            return;

        Quaternion rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            rot,
            Time.deltaTime * 10f
        );
    }

    void ReleaseLockOn()
    {
        if (lockOnTarget != null)
            lockOnTarget.GetComponent<LockOnTarget>()?.Hide();

        lockOnTarget = null;
        IsLockOn = false;
        player.IsLockOn = false;
    }


    void ToggleLockOn()
    {
        if (!IsLockOn)
        {
            lockOnTarget = FindNearestTarget();
            if (lockOnTarget == null)
                return;

            IsLockOn = true;
            player.IsLockOn = true;

            // 표시 ON
            lockOnTarget.GetComponent<LockOnTarget>()?.Show();
        }
        else
        {
            // 표시 OFF
            if (lockOnTarget != null)
                lockOnTarget.GetComponent<LockOnTarget>()?.Hide();

            lockOnTarget = null;
            IsLockOn = false;
            player.IsLockOn = false;
        }
    }


    void OnEnable()
    {
        InputManager.OnLockOn += ToggleLockOn;
    }

    void OnDisable()
    {
        InputManager.OnLockOn -= ToggleLockOn;
    }

    Transform FindNearestTarget()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            lockOnRange,
            monsterLayer
        );

        float minDist = float.MaxValue;
        Transform nearest = null;

        foreach (var hit in hits)
        {
            MonsterCore monster = hit.GetComponentInParent<MonsterCore>();
            if (monster == null || monster.IsDead)
                continue;

            LockOnTarget target = monster.GetComponentInChildren<LockOnTarget>();
            if (target == null)
                continue;

            Vector3 dir = target.transform.position - transform.position;
            float dist = dir.magnitude;

            // 시야각 체크
            float angle = Vector3.Angle(transform.forward, dir.normalized);
            if (angle > lockOnViewAngle * 0.5f)
                continue;

            if (dist < minDist)
            {
                minDist = dist;
                nearest = target.transform;
            }
        }

        return nearest;
    }


}

