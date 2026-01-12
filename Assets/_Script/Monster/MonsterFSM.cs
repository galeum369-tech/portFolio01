using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 공통 몬스터 FSM (일반 몬스터)
/// - 애니메이션 이벤트 미사용
/// - 애니메이션 시간 기반 상태 전이
/// - 행동 컴포넌트(IMonsterAction) 연동
/// </summary>
public class MonsterFSM : MonoBehaviour
{
    public enum State
    {
        Idle,
        Alert,
        Move,
        Attack,
        Hit,
        Dead
    }

    [Header("현재 상태")]
    public State CurrentState { get; private set; } = State.Idle;

    [Header("참조")]
    public MonsterCore core;
    public Animator anim;
    public NavMeshAgent agent;
    public Transform target;

    AnimatorStateInfo animInfo;

    IMonsterAction[] actions;

    [Header("시야 인식")]
    public float viewAngle = 120f;
    public float viewDistance = 8f;

    void Awake()
    {
        if (core == null) core = GetComponent<MonsterCore>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (anim == null) anim = GetComponentInChildren<Animator>();

        actions = GetComponents<IMonsterAction>();
    }

    void Update()
    {
        if (CurrentState == State.Dead)
            return;

        switch (CurrentState)
        {
            case State.Idle:
                UpdateIdle();
                break;
            case State.Alert:
                UpdateAlert();
                break;
            case State.Move:
                UpdateMove();
                break;
            case State.Attack:
                UpdateAttack();
                break;
            case State.Hit:
                UpdateHit();
                break;
        }
    }

    /*───────────────────────────────*
     * 상태별 업데이트
     *───────────────────────────────*/

    void UpdateIdle()
    {
        agent.isStopped = true;

        if (DetectTarget())
        {
            ChangeState(State.Alert);
        }
    }

    void UpdateAlert()
    {
        // Alert는 진입 연출용 상태
        // ★ 수정: 행동 실행은 EnterState에서 처리
        ChangeState(State.Move);
    }

    void UpdateMove()
    {
        if (target == null)
        {
            ChangeState(State.Idle);
            return;
        }

        float dist = Vector3.Distance(transform.position, target.position);

        // 추적 포기
        if (dist > core.BaseData.disengageRange)
        {
            ChangeState(State.Idle);
            return;
        }

        agent.isStopped = false;
        agent.SetDestination(target.position);

        // 공격 가능 거리
        if (dist <= core.BaseData.preferredRange)
        {
            ChangeState(State.Attack);
        }
    }

    void UpdateAttack()
    {
        agent.isStopped = true;

        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);

        // 공격 애니가 아직 끝나지 않았으면 대기
        if (info.normalizedTime < 0.95f)
            return;

        // 공격 종료
        ChangeState(State.Move);
    }

    void UpdateHit()
    {
        agent.isStopped = true;

        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);

        // 피격 애니 끝날 때까지 대기
        if (info.normalizedTime < 0.8f)
            return;

        ChangeState(State.Move);
    }

    /*───────────────────────────────*
     * 상태 전이
     *───────────────────────────────*/

    void ChangeState(State newState)
    {
        if (CurrentState == newState)
            return;

        ExitState(CurrentState);
        CurrentState = newState;
        EnterState(newState);
    }

    void EnterState(State state)
    {
        switch (state)
        {
            case State.Idle:
                anim.CrossFade("Idle", 0.1f);
                break;

            case State.Alert:
                // ★ 수정: 전투 개시 행동은 상태 진입 시 1회 실행
                TryExecuteActions();
                anim.CrossFade("Alert", 0.1f);
                break;

            case State.Move:
                anim.CrossFade("Move", 0.1f);
                break;

            case State.Attack:
                TryExecuteActions();   // 공격 관련 행동 실행
                anim.CrossFade("Attack", 0.05f);
                break;

            case State.Hit:
                anim.CrossFade("Hit", 0.05f);
                break;

            case State.Dead:
                anim.CrossFade("Die", 0.1f);
                agent.isStopped = true;
                break;
        }
    }

    void ExitState(State state)
    {
        // 현재는 별도 처리 없음
    }

    /*───────────────────────────────*
     * 외부 입력 (피격 / 사망)
     *───────────────────────────────*/

    /// <summary>
    /// 히트박스 / 투사체에서 호출
    /// </summary>
    public void OnHit(float poiseDamage)
    {
        if (CurrentState == State.Dead)
            return;

        // 일반 몬스터 기준:
        // true → 히트 리액션 발생
        if (core.ApplyPoiseDamage(poiseDamage))
        {
            ChangeState(State.Hit);
        }
    }

    public void OnDead()
    {
        ChangeState(State.Dead);
    }

    /*───────────────────────────────*
     * 행동 컴포넌트 처리
     *───────────────────────────────*/

    void TryExecuteActions()
    {
        // ★ 수정: Action이 없어도 안전하게 동작
        if (actions == null || actions.Length == 0)
            return;

        foreach (var action in actions)
        {
            if (action.CanExecute(this))
            {
                action.Execute(this);
                break;
            }
        }
    }

    /*───────────────────────────────*
     * 시야 인식
     *───────────────────────────────*/

    bool DetectTarget()
    {
        if (target == null)
            return false;

        Vector3 dir = target.position - transform.position;
        float dist = dir.magnitude;

        if (dist > viewDistance)
            return false;

        float angle = Vector3.Angle(transform.forward, dir.normalized);
        if (angle > viewAngle * 0.5f)
            return false;

        return true;
    }
}

