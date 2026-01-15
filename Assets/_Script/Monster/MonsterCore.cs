using UnityEngine;

/// <summary>
/// 몬스터의 모든 런타임 상태를 관리하는 코어
/// - 체력 / 스태미나 / 강인도
/// - 회복 처리
/// - 적극성 계산
/// - (추가) Intent(의도) 계산
/// 
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

    /*───────────────────────────────*
     * Intent 디버그
     *───────────────────────────────*/
    [Header("디버그(선택)")]
    [Tooltip("체크하면 Intent 변화 시 로그를 출력합니다.")]
    [SerializeField] bool debugIntentLog = false;

    MonsterIntent lastIntent = MonsterIntent.None;

    void Awake()
    {
        // 방어 코드(데이터 누락)
        if (baseData == null)
        {
            Debug.LogError("[MonsterCore] MonsterBaseData가 연결되지 않았습니다.", this);
            enabled = false;
            return;
        }

        CurrentHP = baseData.maxHP;
        CurrentStamina = baseData.maxStamina;
        CurrentPoise = baseData.maxPoise;
    }

    void Update()
    {
        if (IsDead) return;

        RecoverResources(Time.deltaTime);

        // Intent 로그(선택)
        if (debugIntentLog)
        {
            var intent = GetIntent();
            if (intent != lastIntent)
            {
                Debug.Log($"[MonsterCore] Intent 변경: {lastIntent} → {intent}", this);
                lastIntent = intent;
            }
        }
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
            if (Time.time >= lastStaminaConsumeTime + baseData.staminaRecoveryDelay)
            {
                CurrentStamina = Mathf.Min(
                    baseData.maxStamina,
                    CurrentStamina + baseData.staminaRecoveryPerSecond * deltaTime
                );
            }
        }

        // 강인도 회복
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

        // 사망 처리(클램프)
        if (CurrentHP < 0f) CurrentHP = 0f;
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
    /// true → 경직/스태거 트리거 여부 (해석은 FSM에서)
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

    /*───────────────────────────────*
     * Intent(의도) 계산
     *───────────────────────────────*/

    /// <summary>
    /// 현재 리소스/상황을 기반으로 "지금 뭘 하고 싶은지"를 반환한다.
    /// 
    /// ✅ 설계 의도
    /// - FSM은 상태 전이를 담당(언제)
    /// - Intent는 행동 선택 기준(무엇을)
    /// - Action은 실제 실행(어떻게)
    /// 
    /// 🔸 지금은 단순 버전(V1)
    /// - AggressionLevel 기반으로만 매핑
    /// - 추후 거리/시야/쿨다운/패턴 등 추가 가능
    /// </summary>
    public MonsterIntent GetIntent()
    {
        if (IsDead)
            return MonsterIntent.None;

        AggressionLevel aggro = GetAggressionLevel();

        // (예시) 매우 단순 매핑
        // High: 압박(공격 성향)
        // Balanced: 전투 유지
        // Low: 후퇴/회복 성향
        switch (aggro)
        {
            case AggressionLevel.High:
                return MonsterIntent.Pressure;

            case AggressionLevel.Balanced:
                return MonsterIntent.Engage;

            case AggressionLevel.Low:
                // Low일 때는 Retreat/Recover 중 무엇을 선택할지 정책을 정할 수 있음
                // 지금은 "체력이 낮으면 Recover, 아니면 Retreat" 같은 규칙을 한 줄로 둠
                float hpRatio = CurrentHP / baseData.maxHP;
                if (hpRatio <= baseData.hpLowThreshold)
                    return MonsterIntent.Recover;

                return MonsterIntent.Retreat;
        }

        return MonsterIntent.None;
    }
}

public enum AggressionLevel
{
    Low,        // 소극적
    Balanced,   // 균형
    High        // 적극적
}
