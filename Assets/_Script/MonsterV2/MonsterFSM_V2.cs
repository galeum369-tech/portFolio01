using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// MonsterFSM_V2
/// - 일반 몬스터 전용 FSM (Intent 미사용)
/// - 쿨타임 기반 단발 공격
/// - 애니메이터 파라미터 방식
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class MonsterFSM_V2 : MonoBehaviour
{
    public enum State { Idle, Scream, Chase, Attack, Hit, Dead }
    public State CurrentState { get; private set; } = State.Idle;

    /*───────────────────────────────*
     * 참조
     *───────────────────────────────*/
    [Header("참조")]
    [SerializeField] MonsterBaseData baseData;
    [SerializeField] Animator anim;
    [SerializeField] NavMeshAgent agent;

    [Header("공격 히트박스")]
    [SerializeField] GameObject attackHitBox; // 팔/무기 히트박스

    Transform target;

    /*───────────────────────────────*
     * 타겟 탐색
     *───────────────────────────────*/
    [Header("타겟 탐색")]
    [SerializeField] LayerMask targetLayer;
    [SerializeField] float viewDistance = 8f;
    [SerializeField] float viewAngle = 120f;
    [SerializeField] float searchInterval = 0.25f;

    /*───────────────────────────────*
     * 공격 설정
     *───────────────────────────────*/
    [Header("공격 설정")]
    [SerializeField] float attackCooldown = 2f;
    [SerializeField] float minAttackRange = 1.2f;

    bool isAttackCooldown;
    float attackCooldownTimer;
    bool hasScreamed;

    /*───────────────────────────────*
     * 런타임 스탯
     *───────────────────────────────*/
    float currentHP;
    float currentPoise;

    /*───────────────────────────────*
     * Animator 파라미터
     *───────────────────────────────*/
    readonly int hashWalk = Animator.StringToHash("Walk");
    readonly int hashAttack = Animator.StringToHash("Attack");
    readonly int hashHit = Animator.StringToHash("Hit");
    readonly int hashScream = Animator.StringToHash("Scream");
    readonly int hashDeath = Animator.StringToHash("Death");

    /*───────────────────────────────*
     * 초기화
     *───────────────────────────────*/
    void Awake()
    {
        if (!baseData)
        {
            Debug.LogError("[MonsterFSM_V2] MonsterBaseData 미설정", this);
            enabled = false;
            return;
        }

        if (!anim) anim = GetComponentInChildren<Animator>();
        if (!agent) agent = GetComponent<NavMeshAgent>();

        currentHP = baseData.maxHP;
        currentPoise = baseData.maxPoise;

        agent.speed = baseData.moveSpeed;
        agent.isStopped = true;

        if (attackHitBox)
            attackHitBox.SetActive(false);
    }

    void Start()
    {
        StartCoroutine(TargetSearchRoutine());
    }

    void Update()
    {
        if (CurrentState == State.Dead)
            return;

        UpdateAttackCooldown();

        switch (CurrentState)
        {
            case State.Idle: UpdateIdle(); break;
            case State.Scream: UpdateScream(); break;
            case State.Chase: UpdateChase(); break;
            case State.Attack: UpdateAttack(); break;
            case State.Hit: UpdateHit(); break;
        }
    }

    /*───────────────────────────────*
     * 타겟 탐색 (최적화)
     *───────────────────────────────*/
    IEnumerator TargetSearchRoutine()
    {
        while (CurrentState != State.Dead)
        {
            if (CurrentState == State.Idle)
                TryFindTarget();

            yield return new WaitForSeconds(searchInterval);
        }
    }

    /*───────────────────────────────*
     * 상태별 로직
     *───────────────────────────────*/
    void UpdateIdle()
    {
        agent.isStopped = true;
        anim.SetBool(hashWalk, false);

        if (target != null)
            ChangeState(hasScreamed ? State.Chase : State.Scream);
    }

    void UpdateScream()
    {
        agent.isStopped = true;

        if (IsAnimFinished(0.95f))
        {
            hasScreamed = true;
            ChangeState(State.Chase);
        }
    }

    void UpdateChase()
    {
        if (!IsTargetValid())
        {
            target = null;
            ChangeState(State.Idle);
            return;
        }

        float dist = Vector3.Distance(transform.position, target.position);

        if (dist > baseData.disengageRange)
        {
            target = null;
            ChangeState(State.Idle);
            return;
        }

        // 너무 가까우면 정지 + 회전
        if (dist <= minAttackRange)
        {
            agent.isStopped = true;
            anim.SetBool(hashWalk, false);
            FaceTarget(8f);
            return;
        }

        // 공격 진입 (쿨 OK일 때만)
        if (dist <= baseData.preferredRange && !isAttackCooldown)
        {
            ChangeState(State.Attack);
            return;
        }

        agent.isStopped = false;
        anim.SetBool(hashWalk, true);
        agent.SetDestination(target.position);
    }

    void UpdateAttack()
    {
        agent.isStopped = true;
        FaceTarget(12f);

        if (IsAnimFinished(0.9f))
        {
            ChangeState(State.Chase);
        }
    }

    void UpdateHit()
    {
        agent.isStopped = true;

        if (IsAnimFinished(0.8f))
            ChangeState(State.Chase);
    }

    /*───────────────────────────────*
     * 상태 전이
     *───────────────────────────────*/
    void ChangeState(State next)
    {
        if (CurrentState == next)
            return;

        // Attack 종료 시 쿨 시작
        if (CurrentState == State.Attack)
        {
            isAttackCooldown = true;
            attackCooldownTimer = attackCooldown;
            anim.SetBool(hashAttack, false);
            DisableHitBox();
        }

        CurrentState = next;

        switch (next)
        {
            case State.Idle:
                anim.SetBool(hashWalk, false);
                break;

            case State.Scream:
                anim.SetTrigger(hashScream);
                break;

            case State.Chase:
                anim.SetBool(hashWalk, true);
                break;

            case State.Attack:
                anim.SetBool(hashAttack, true);
                break;

            case State.Hit:
                anim.SetTrigger(hashHit);
                DisableHitBox();
                break;

            case State.Dead:
                anim.SetTrigger(hashDeath);
                agent.isStopped = true;
                DisableHitBox();
                GetComponent<Collider>().enabled = false;
                break;
        }
    }

    /*───────────────────────────────*
     * 쿨타임
     *───────────────────────────────*/
    void UpdateAttackCooldown()
    {
        if (!isAttackCooldown || CurrentState == State.Attack)
            return;

        attackCooldownTimer -= Time.deltaTime;
        if (attackCooldownTimer <= 0f)
            isAttackCooldown = false;
    }

    /*───────────────────────────────*
     * 데미지 처리
     *───────────────────────────────*/
    public void TakeDamage(float damage, float poiseDamage)
    {
        if (CurrentState == State.Dead)
            return;

        currentHP -= Mathf.Max(1f, damage - baseData.defense);
        currentPoise -= poiseDamage;

        if (currentHP <= 0f)
        {
            ChangeState(State.Dead);
            return;
        }

        if (currentPoise <= 0f)
        {
            currentPoise = baseData.maxPoise;
            ChangeState(State.Hit);
        }
    }

    /*───────────────────────────────*
     * 히트박스 제어 (Animation Event)
     *───────────────────────────────*/
    public void EnableHitBox()
    {
        if (attackHitBox)
            attackHitBox.SetActive(true);
    }

    public void DisableHitBox()
    {
        if (attackHitBox)
            attackHitBox.SetActive(false);
    }

    /*───────────────────────────────*
     * 유틸
     *───────────────────────────────*/
    void FaceTarget(float speed)
    {
        if (!target) return;

        Vector3 dir = target.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f) return;

        Quaternion rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * speed);
    }

    bool IsAnimFinished(float threshold)
    {
        if (anim.IsInTransition(0)) return false;
        return anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= threshold;
    }

    void TryFindTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, viewDistance, targetLayer);
        float minDist = float.MaxValue;
        Transform nearest = null;

        foreach (var hit in hits)
        {
            Vector3 dir = hit.transform.position - transform.position;
            float angle = Vector3.Angle(transform.forward, dir.normalized);

            if (angle > viewAngle * 0.5f)
                continue;

            float dist = dir.magnitude;
            if (dist < minDist)
            {
                minDist = dist;
                nearest = hit.transform;
            }
        }

        target = nearest;
    }

    bool IsTargetValid()
    {
        if (!target) return false;

        PlayerCoreV2 player = target.GetComponentInParent<PlayerCoreV2>();
        return player && !player.IsDead;
    }
}
