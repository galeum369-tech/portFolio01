using UnityEngine;

/// <summary>
/// 몬스터의 "의도(Intent)" 정의
/// 
/// ✅ 목적
/// - FSM(Idle/Move/Attack...)은 그대로 유지한다.
/// - Intent는 "지금 뭘 하고 싶은지"를 표현한다.
/// - 실제 행동(포효/돌진/거리벌리기/특수공격)은 IMonsterAction이 수행한다.
/// 
/// ✅ 핵심
/// - FSM = 언제(타이밍/상태)
/// - Intent = 무엇을(의도/목표)
/// - Action = 어떻게(구체 행동)
/// </summary>
public enum MonsterIntent
{
    None = 0,

    /// <summary>
    /// 전투 유지 / 기본 추적 및 안전한 압박
    /// - 거리 맞추고 기본 공격 각 보는 단계
    /// </summary>
    Engage = 10,

    /// <summary>
    /// 적극 압박 / 공격적으로 밀어붙임
    /// - 돌진, 강공격, 연계 등 공격 행동 선호
    /// </summary>
    Pressure = 20,

    /// <summary>
    /// 후퇴 / 거리 벌리기
    /// - 체력이 낮거나 스태미나가 부족할 때 회복 시간을 벌고 싶음
    /// </summary>
    Retreat = 30,

    /// <summary>
    /// 회복 / 숨 고르기
    /// - Retreat보다 더 명확히 "회복 행동"을 우선
    /// - (추후: 가드/방어/포지션 유지 등으로 확장 가능)
    /// </summary>
    Recover = 40
}

