using UnityEngine;

/// <summary>
/// 플레이어의 런타임 전투 상태를 관리하는 코어
/// - 체력 / 스태미나 / 강인도
/// - 스태미나 회복 및 소모 딜레이
/// - 무기 공격력 계산용 스탯 제공 (STR / DEX)
/// 
/// Controller / Combat은 이 값을 조회·요청만 한다.
/// </summary>
public class PlayerCore : MonoBehaviour
{
    [Header("플레이어 데이터")]
    [SerializeField] private PlayerBaseData data;

    /*───────────────────────────────*
     * 런타임 상태
     *───────────────────────────────*/
    public int CurrentHP { get; private set; }
    public int CurrentStamina { get; private set; }
    public int CurrentPoise { get; private set; }

    public bool IsDead => CurrentHP <= 0;

    // 스태미나 회복 딜레이 타이머
    float exhaustTimer;

    void Awake()
    {
        // 데이터 누락 방어
        if (data == null)
        {
            Debug.LogError("PlayerBaseData가 설정되지 않았습니다.", this);
            enabled = false;
            return;
        }

        CurrentHP = data.maxHP;
        CurrentStamina = data.maxStamina;
        CurrentPoise = data.Poise;
    }

    void Update()
    {
        // 사망 시 리소스 처리 중단
        if (IsDead) return;

        RecoverStamina(Time.deltaTime);
    }

    /*───────────────────────────────*
     * 스태미나 회복
     *───────────────────────────────*/

    /// <summary>
    /// 스태미나 자동 회복 처리
    /// - 스태미나 소비 후 딜레이(exhaustDelay) 동안 회복 정지
    /// </summary>
    void RecoverStamina(float deltaTime)
    {
        if (exhaustTimer > 0f)
        {
            exhaustTimer -= deltaTime;
            return;
        }

        CurrentStamina = Mathf.Min(
            data.maxStamina,
            CurrentStamina + Mathf.RoundToInt(data.staminaRegen * deltaTime)
        );
    }

    /*───────────────────────────────*
     * 스태미나 소모
     *───────────────────────────────*/

    /// <summary>
    /// 스태미나 사용 가능 여부 확인
    /// </summary>
    public bool CanUseStamina(int amount)
    {
        return CurrentStamina >= amount;
    }

    /// <summary>
    /// 스태미나 소모 시도
    /// - 성공 시 회복 딜레이 시작
    /// </summary>
    public bool TryConsumeStamina(int amount)
    {
        if (CurrentStamina < amount)
            return false;

        CurrentStamina -= amount;
        exhaustTimer = data.exhaustDelay;
        return true;
    }

    /// <summary>
    /// 달리기용 스태미나 소모
    /// (초당 소모량 기반)
    /// </summary>
    public bool TryConsumeRunStamina(float deltaTime)
    {
        int cost = Mathf.RoundToInt(data.runCostPerSecond * deltaTime);
        return TryConsumeStamina(cost);
    }

    /// <summary>
    /// 구르기용 스태미나 소모
    /// </summary>
    public bool TryConsumeRollStamina()
    {
        return TryConsumeStamina(data.rollingCost);
    }

    /*───────────────────────────────*
     * 데미지 처리
     *───────────────────────────────*/

    /// <summary>
    /// 체력 데미지 처리
    /// - 방어력 단순 감산
    /// </summary>
    public void TakeDamage(int damage)
    {
        int finalDamage = Mathf.Max(1, damage - data.DEF);
        CurrentHP -= finalDamage;

        if (CurrentHP < 0)
            CurrentHP = 0;
    }

    /*───────────────────────────────*
     * 강인도 (Poise)
     *───────────────────────────────*/

    /// <summary>
    /// 강인도 데미지 적용
    /// true 반환 시 경직/히트 리액션 발생
    /// </summary>
    public bool ApplyPoiseDamage(int poiseDamage)
    {
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
     * 공격 스탯 제공 (무기 계산용)
     *───────────────────────────────*/

    /// <summary>
    /// 근력 스탯 (STR 계수 계산용)
    /// </summary>
    public int STR => data.STR;

    /// <summary>
    /// 기량 스탯 (DEX 계수 계산용)
    /// </summary>
    public int DEX => data.DEX;

    /*───────────────────────────────*
     * 이동 관련 수치 제공
     *───────────────────────────────*/

    /// <summary>
    /// 걷기 이동 속도
    /// </summary>
    public float WalkSpeed => data.moveSpeed;

    /// <summary>
    /// 달리기 이동 속도
    /// </summary>
    public float RunSpeed => data.runSpeed;
}

