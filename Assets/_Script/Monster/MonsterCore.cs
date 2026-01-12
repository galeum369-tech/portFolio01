using UnityEngine;

/// <summary>
/// 몬스터의 모든 런타임 상태를 관리하는 코어
/// - 체력 / 스태미나 / 강인도
/// - 회복 처리
/// - 적극성 계산
/// FSM과 AI는 이 값을 '조회'만 한다.
/// </summary>
public class MonsterCore : MonoBehaviour
{
    [Header("베이스 데이터")]
    [SerializeField] private MonsterBaseData baseData;
    public MonsterBaseData BaseData => baseData;

    /*───────────────────────────────*
     * 런타임 상태
     *───────────────────────────────*/
    public float CurrentHP { get; private set; }
    public float CurrentStamina { get; private set; }
    public float CurrentPoise { get; private set; }

    // 마지막으로 맞은 공격 ID
    int lastHitAttackId = -1;


    // 마지막 스태미나 소비 시점 (회복 딜레이 계산용)
    float lastStaminaConsumeTime = -999f;

    public bool IsDead => CurrentHP <= 0f;

    void Awake()
    {
        CurrentHP = baseData.maxHP;
        CurrentStamina = baseData.maxStamina;
        CurrentPoise = baseData.maxPoise;
    }

    void Update()
    {
        if (IsDead) return;
        RecoverResources(Time.deltaTime);
    }

    /*───────────────────────────────*
     * 리소스 회복
     *───────────────────────────────*/
    void RecoverResources(float deltaTime)
    {
        // 체력 회복
        if (baseData.hpRecoveryPerSecond > 0f)
        {
            CurrentHP = Mathf.Min(
                baseData.maxHP,
                CurrentHP + baseData.hpRecoveryPerSecond * deltaTime
            );
        }

        // 스태미나 회복 (소비 후 딜레이 적용)
        if (baseData.staminaRecoveryPerSecond > 0f)
        {
            // 마지막 소비 이후 일정 시간이 지나야 회복 시작
            if (Time.time >= lastStaminaConsumeTime + baseData.staminaRecoveryDelay)
            {
                CurrentStamina = Mathf.Min(
                    baseData.maxStamina,
                    CurrentStamina + baseData.staminaRecoveryPerSecond * deltaTime
                );
            }
        }

        // 강인도 회복 (즉시 회복 방식과 병행 가능)
        if (baseData.poiseRecoveryPerSecond > 0f)
        {
            CurrentPoise = Mathf.Min(
                baseData.maxPoise,
                CurrentPoise + baseData.poiseRecoveryPerSecond * deltaTime
            );
        }
    }

    /// <summary>
    /// 해당 공격 ID로 피격 가능한지 여부
    /// 같은 공격(AttackId)에 대해서는 한 번만 맞게 한다.
    /// </summary>
    public bool CanBeHit(int attackId)
    {
        if (lastHitAttackId == attackId)
            return false;

        lastHitAttackId = attackId;
        return true;
    }


    /*───────────────────────────────*
     * 데미지 / 소비 처리
     *───────────────────────────────*/

    public void TakeDamage(float damage)
    {
        float finalDamage = Mathf.Max(0f, damage - baseData.defense);
        CurrentHP -= finalDamage;
    }

    /// <summary>
    /// 스태미나를 소비하고, 회복 딜레이 타이머를 갱신
    /// </summary>
    public void ConsumeStamina(float amount)
    {
        CurrentStamina = Mathf.Max(0f, CurrentStamina - amount);
        lastStaminaConsumeTime = Time.time;
    }

    /// <summary>
    /// 강인도 판정용 함수
    /// - 일반 몬스터: true → Hit 리액션 여부 판단
    /// - 보스 몬스터: true → 경직 / 스태거 트리거 판단
    /// 해석은 FSM에서 담당한다.
    /// </summary>
    public bool ApplyPoiseDamage(float poiseDamage)
    {
        CurrentPoise -= poiseDamage;

        if (CurrentPoise <= 0f)
        {
            ResetPoise();
            return true;
        }

        return false;
    }

    void ResetPoise()
    {
        CurrentPoise = baseData.maxPoise;
    }

    /*───────────────────────────────*
     * 적극성 계산
     *───────────────────────────────*/

    /// <summary>
    /// 현재 리소스를 기반으로 적극성 수준을 반환
    /// </summary>
    public AggressionLevel GetAggressionLevel()
    {
        float hpRatio = CurrentHP / baseData.maxHP;
        float staminaRatio = CurrentStamina / baseData.maxStamina;

        // 체력이 낮으면 무조건 소극적
        if (hpRatio <= baseData.hpLowThreshold)
            return AggressionLevel.Low;

        // 체력은 괜찮고 스태미나 기준 판단
        if (staminaRatio >= baseData.staminaHighThreshold)
            return AggressionLevel.High;

        if (staminaRatio >= baseData.staminaLowThreshold)
            return AggressionLevel.Balanced;

        return AggressionLevel.Low;
    }
}

public enum AggressionLevel
{
    Low,        // 소극적
    Balanced,   // 균형
    High        // 적극적
}
