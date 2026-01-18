using UnityEngine;

/// <summary>
/// PlayerCoreV2
/// 
/// [역할]
/// - 플레이어의 런타임 전투 수치 관리
/// - 체력 / 스태미나 / 강인도
/// - 스태미나 점진 회복
/// - 무적 / 가드 상태에 따른 데미지 처리
/// - 히트 판정 및 히트 리액션 처리
/// 
/// ⚠ 행동 판단 ❌
/// ⚠ 입력 처리 ❌
/// → Controller / Combat / HitBox는 "요청"만 한다.
/// </summary>
public class PlayerCoreV2 : MonoBehaviour
{
    /*───────────────────────────────*
     * 데이터
     *───────────────────────────────*/
    [Header("플레이어 데이터")]
    [SerializeField] private PlayerBaseData data;

    /*───────────────────────────────*
     * 런타임 상태
     *───────────────────────────────*/
    public int CurrentHP { get; private set; }
    public float CurrentStamina { get; private set; }
    public int CurrentPoise { get; private set; }

    public bool IsDead => CurrentHP <= 0;

    /*───────────────────────────────*
     * 상태 플래그
     *───────────────────────────────*/
    bool isInvincible;
    bool isGuarding;
    bool isHitStopping;

    /*───────────────────────────────*
     * 가드 설정
     *───────────────────────────────*/
    [Header("가드")]
    [Tooltip("가드 시 데미지 경감 비율 (0.3 = 30% 감소)")]
    [Range(0f, 1f)]
    [SerializeField] float guardDamageReduction = 0.3f;

    /*───────────────────────────────*
     * 히트 연출
     *───────────────────────────────*/
    [Header("히트 연출")]
    [SerializeField] Animator anim;
    [Tooltip("일반 히트 시 멈춤 시간")]
    [SerializeField] float hitStopTime = 0.25f;
    [Tooltip("강인도 붕괴 시 멈춤 시간")]
    [SerializeField] float staggerStopTime = 0.4f;

    /*───────────────────────────────*
     * 초기화
     *───────────────────────────────*/
    void Awake()
    {
        if (data == null)
        {
            Debug.LogError("[PlayerCoreV2] PlayerBaseData가 설정되지 않았습니다.", this);
            enabled = false;
            return;
        }

        if (anim == null)
            anim = GetComponentInChildren<Animator>();

        CurrentHP = data.maxHP;
        CurrentStamina = data.maxStamina;
        CurrentPoise = data.Poise;
    }

    void Update()
    {
        if (IsDead) return;
        RecoverStamina(Time.deltaTime);
    }

    /*───────────────────────────────*
     * 스태미나
     *───────────────────────────────*/
    void RecoverStamina(float deltaTime)
    {
        if (CurrentStamina >= data.maxStamina)
            return;

        CurrentStamina += data.staminaRegen * deltaTime;

        if (CurrentStamina > data.maxStamina)
            CurrentStamina = data.maxStamina;
    }

    public bool TryConsumeStamina(float amount)
    {
        if (CurrentStamina < amount)
            return false;

        CurrentStamina -= amount;
        return true;
    }

    public bool TryConsumeRunStamina(float deltaTime)
    {
        int cost = Mathf.RoundToInt(data.runCostPerSecond * deltaTime);
        return TryConsumeStamina(cost);
    }

    public bool TryConsumeRollStamina()
    {
        return TryConsumeStamina(data.rollingCost);
    }

    /*───────────────────────────────*
     * 외부 공격 통합 진입점
     *───────────────────────────────*/

    /// <summary>
    /// 몬스터 / 투사체 / 트랩 등
    /// 외부 공격의 단일 진입점
    /// </summary>
    public void ReceiveAttack(int damage, int poiseDamage)
    {
        if (isInvincible || IsDead)
            return;

        int finalDamage = Mathf.Max(1, damage - data.DEF);

        if (isGuarding)
            finalDamage = Mathf.RoundToInt(finalDamage * (1f - guardDamageReduction));

        ApplyHPDamage(finalDamage);

        bool poiseBroken = ApplyPoiseDamage(poiseDamage);

        PlayHitReaction(poiseBroken);
    }

    /*───────────────────────────────*
     * 기존 코드 호환용 (중요)
     *───────────────────────────────*/

    /// <summary>
    /// 기존 몬스터 히트박스 호환용
    /// </summary>
    public void TakeDamage(int damage)
    {
        ReceiveAttack(damage, 0);
    }

    /*───────────────────────────────*
     * 내부 처리
     *───────────────────────────────*/

    void ApplyHPDamage(int amount)
    {
        CurrentHP -= amount;

        if (CurrentHP < 0)
            CurrentHP = 0;
    }

    bool ApplyPoiseDamage(int poiseDamage)
    {
        if (poiseDamage <= 0)
            return false;

        CurrentPoise -= poiseDamage;

        if (CurrentPoise <= 0)
        {
            ResetPoise();
            return true;
        }

        return false;
    }

    void ResetPoise()
    {
        CurrentPoise = data.Poise;
    }

    /*───────────────────────────────*
     * 히트 리액션
     *───────────────────────────────*/

    void PlayHitReaction(bool stagger)
    {
        if (isHitStopping)
            return;

        isHitStopping = true;

        if (anim != null)
            anim.SetTrigger("Hit");

        float stopTime = stagger ? staggerStopTime : hitStopTime;
        Invoke(nameof(ReleaseHitStop), stopTime);
    }

    void ReleaseHitStop()
    {
        isHitStopping = false;
    }

    /*───────────────────────────────*
     * 상태 제어
     *───────────────────────────────*/

    public void SetGuard(bool value)
    {
        isGuarding = value;
    }

    public bool IsGuarding => isGuarding;

    public void SetInvincible(bool value)
    {
        isInvincible = value;
    }

    /*───────────────────────────────*
     * 스탯 제공 (Controller 전용)
     *───────────────────────────────*/

    public int STR => data.STR;
    public int DEX => data.DEX;

    public float WalkSpeed => data.moveSpeed;
    public float RunSpeed => data.runSpeed;
}


