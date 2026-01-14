using UnityEngine;

/// <summary>
/// PlayerCoreV2
/// 
/// [역할]
/// - 플레이어의 런타임 전투 수치 관리
/// - 체력 / 스태미나 / 강인도
/// - 스태미나 점진 회복 (딜레이 없음)
/// - 무적 상태에 따른 데미지 차단
/// 
/// ⚠ 행동 판단 ❌
/// ⚠ 입력 처리 ❌
/// Controller / Combat은 "요청"만 한다.
/// </summary>
public class PlayerCoreV2 : MonoBehaviour
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

    /*───────────────────────────────*
     * 상태 플래그
     *───────────────────────────────*/
    bool isInvincible;

    void Awake()
    {
        if (data == null)
        {
            Debug.LogError("[PlayerCore] PlayerBaseData가 설정되지 않았습니다.", this);
            enabled = false;
            return;
        }

        CurrentHP = data.maxHP;
        CurrentStamina = data.maxStamina;
        CurrentPoise = data.Poise;

        Debug.Log("[PlayerCore] 초기화 완료");
    }

    void Update()
    {
        if (IsDead) return;

        RecoverStamina(Time.deltaTime);
    }

    /*───────────────────────────────*
     * 스태미나 회복 (딜레이 없음)
     *───────────────────────────────*/

    void RecoverStamina(float deltaTime)
    {
        int before = CurrentStamina;

        CurrentStamina = Mathf.Min(
            data.maxStamina,
            CurrentStamina + Mathf.RoundToInt(data.staminaRegen * deltaTime)
        );

        if (before != CurrentStamina)
        {
            Debug.Log($"[PlayerCore] 스태미나 회복: {before} → {CurrentStamina}");
        }
    }

    /*───────────────────────────────*
     * 스태미나 소모
     *───────────────────────────────*/

    public bool TryConsumeStamina(int amount)
    {
        if (CurrentStamina < amount)
        {
            Debug.Log("[PlayerCore] 스태미나 부족");
            return false;
        }

        CurrentStamina -= amount;
        Debug.Log($"[PlayerCore] 스태미나 소모: -{amount}, 잔여 {CurrentStamina}");
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
     * 데미지 처리
     *───────────────────────────────*/

    public void TakeDamage(int damage)
    {
        if (isInvincible)
        {
            Debug.Log("[PlayerCore] 무적 상태 - 데미지 무시");
            return;
        }

        int finalDamage = Mathf.Max(1, damage - data.DEF);
        CurrentHP -= finalDamage;

        Debug.Log($"[PlayerCore] 데미지 {finalDamage} 적용, 남은 HP {CurrentHP}");

        if (CurrentHP < 0)
            CurrentHP = 0;
    }

    /*───────────────────────────────*
     * 무적 제어
     *───────────────────────────────*/

    public void SetInvincible(bool value)
    {
        isInvincible = value;
        Debug.Log(value
            ? "[PlayerCore] 무적 ON"
            : "[PlayerCore] 무적 OFF");
    }

    /*───────────────────────────────*
     * 강인도 (Poise)
     *───────────────────────────────*/

    public bool ApplyPoiseDamage(int poiseDamage)
    {
        CurrentPoise -= poiseDamage;

        Debug.Log($"[PlayerCore] 강인도 감소: -{poiseDamage}, 잔여 {CurrentPoise}");

        if (CurrentPoise <= 0)
        {
            ResetPoise();
            Debug.Log("[PlayerCore] 강인도 붕괴 → 리셋");
            return true;
        }

        return false;
    }

    void ResetPoise()
    {
        CurrentPoise = data.Poise;
    }

    /*───────────────────────────────*
     * 스탯 제공
     *───────────────────────────────*/

    public int STR => data.STR;
    public int DEX => data.DEX;

    public float WalkSpeed => data.moveSpeed;
    public float RunSpeed => data.runSpeed;
}
