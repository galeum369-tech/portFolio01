using UnityEngine;

[CreateAssetMenu(
    fileName = "MonsterBaseData",
    menuName = "Monster/몬스터 베이스 데이터"
)]
public class MonsterBaseData : ScriptableObject
{
    /*───────────────────────────────*
     * 기본 스탯 (존재 자체의 전투 수치)
     *───────────────────────────────*/
    [Header("기본 스탯")]
    [Tooltip("몬스터의 최대 체력")]
    public float maxHP;

    [Tooltip("몬스터의 최대 스태미나")]
    public float maxStamina;

    [Tooltip("기본 공격력")]
    public float attackPower;

    [Tooltip("기본 방어력")]
    public float defense;

    [Tooltip("기본 이동 속도")]
    public float moveSpeed;

    [Tooltip("기본 강인도")]
    public float maxPoise;

    [Header("리소스 회복")]
    [Tooltip("초당 체력 회복량 (0이면 자동 회복 없음)")]
    public float hpRecoveryPerSecond = 0f;

    [Tooltip("초당 스태미나 회복량")]
    public float staminaRecoveryPerSecond = 10f;

    [Tooltip("초당 강인도 회복량 (0이면 즉시 회복 방식만 사용)")]
    public float poiseRecoveryPerSecond = 0f;

    [Header("스태미나 회복 지연")]
    [Tooltip("스태미나 사용 후 회복이 시작되기까지의 지연 시간")]
    public float staminaRecoveryDelay = 0.15f;

    /*───────────────────────────────*
     * 리소스 기반 행동 기준
     *───────────────────────────────*/
    [Header("리소스 기반 행동 기준 (적극성)")]

    [Tooltip("체력 비율이 이 값 이하일 경우 소극적으로 행동")]
    [Range(0f, 1f)]
    public float hpLowThreshold = 0.3f;

    [Tooltip("이 비율 이하부터 적극성이 눈에 띄게 감소 (소극적 행동)")]
    [Range(0f, 1f)]
    public float staminaLowThreshold = 0.2f;

    [Tooltip("이 비율 이상이면 공격 위주로 행동")]
    [Range(0f, 1f)]
    public float staminaHighThreshold = 0.6f;

    /*───────────────────────────────*
     * 전투 거리 기준
     *───────────────────────────────*/
    [Header("전투 거리 기준")]
    [Tooltip("몬스터가 유지하려는 이상적인 거리")]
    public float preferredRange = 2.0f;

    [Tooltip("이 거리 안에 들어오면 전투를 개시")]
    public float engageRange = 6.0f;

    [Tooltip("이 거리 이상이면 추격을 포기")]
    public float disengageRange = 8.0f;

    /*───────────────────────────────*
     * 반응 성향 및 적대 태도
     *───────────────────────────────*/
    [Header("반응 성향 및 적대 태도")]
    [Tooltip("몬스터의 기본 적대 성향")]
    public HostilityType baseHostility = HostilityType.Neutral;

    [Tooltip("피격 시 행동 변화(경직/반격)에 얼마나 민감한가")]
    [Range(0f, 1f)]
    public float hitReactionWeight = 0.5f;

    [Tooltip("플레이어의 근접 압박을 얼마나 버틸 수 있는가")]
    [Range(0f, 1f)]
    public float pressureTolerance = 0.5f;
}

public enum HostilityType
{
    Passive,    // 기본적으로 적대하지 않음 (도망/회피 성향)
    Neutral,    // 조건부 적대
    Aggressive  // 처음부터 공격적
}

