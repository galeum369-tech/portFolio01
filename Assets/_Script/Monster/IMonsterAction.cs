using UnityEngine;

/// <summary>
/// 몬스터의 '특징적 행동'을 정의하는 인터페이스
/// 
/// - FSM의 기본 동작(이동/공격/피격)을 대체하지 않는다
/// - 존재하지 않아도 FSM은 정상 동작한다
/// - 몬스터 개성(포효, 돌진, 특수공격 등)을 추가하는 용도
/// 예)
/// - 전투 시작 시 포효
/// - 공격 전에 거리 벌리기
/// - 특정 조건에서만 사용하는 특수 행동
/// </summary>
public interface IMonsterAction
{
    /// <summary>
    /// 현재 상황에서 이 행동을 실행할 수 있는지 판단
    /// FSM, Core, 거리, AggressionLevel 등을 자유롭게 참조
    /// </summary>
    bool CanExecute(MonsterFSM fsm);

    /// <summary>
    /// 행동 실행
    /// 애니메이션 재생, NavMesh 제어, 상태 변경 등 수행
    /// </summary>
    void Execute(MonsterFSM fsm);
}

