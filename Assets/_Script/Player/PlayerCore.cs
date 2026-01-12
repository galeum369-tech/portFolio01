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

    float exhaustTimer; // 스태미나 회복 딜레이 타이머

    void Awake()
    {
        CurrentHP = data.maxHP;
        CurrentStamina = data.maxStamina;
        CurrentPoise = data.Poise;
    }

    void Update()
    {
        RecoverStamina(Time.deltaTime);
    }

    /*───────────────────────────────*
     * 스태미나 회복
     *───────────────────────────────*/
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
    public bool CanUseStamina(int amount)
    {
        return CurrentStamina >= amount;
    }

    public bool TryConsumeStamina(int amount)
    {
        if (CurrentStamina < amount)
            return false;

        CurrentStamina -= amount;
        exhaustTimer = data.exhaustDelay;
        return true;
    }

    /*───────────────────────────────*
     * 데미지 처리
     *───────────────────────────────*/

    public void TakeDamage(int damage)
    {
        // 방어력 단순 감산
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
    /// true 반환 시 경직 발생
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

    public int STR => data.STR;
    public int DEX => data.DEX;
}

