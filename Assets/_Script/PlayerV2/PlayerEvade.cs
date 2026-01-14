using UnityEngine;

/// <summary>
/// PlayerEvade
/// 
/// [역할]
/// - 회피(구르기 / 백스텝) 관련 애니메이션 이벤트 수신
/// - 이벤트 타이밍을 PlayerControllerV2에 전달
/// 
/// ⚠ 로직 판단 ❌
/// ⚠ 상태 계산 ❌
/// → 오직 "언제 발생했는지"만 전달
/// </summary>
public class PlayerEvade : MonoBehaviour
{
    PlayerControllerV2 controller;

    void Awake()
    {
        controller = GetComponentInParent<PlayerControllerV2>();

        if (controller == null)
        {
            Debug.LogError("[PlayerEvade] PlayerControllerV2를 찾지 못했습니다.");
        }
    }

    /*───────────────────────────────*
     * 회피 애니메이션 이벤트
     *───────────────────────────────*/

    /// <summary>
    /// 회피 애니메이션 시작 지점
    /// </summary>
    public void EvadeStart()
    {
        Debug.Log("[PlayerEvade] 회피 시작 이벤트 수신");

        controller?.OnEvadeStart();
    }

    /// <summary>
    /// 회피 애니메이션 종료 지점
    /// </summary>
    public void EvadeEnd()
    {
        Debug.Log("[PlayerEvade] 회피 종료 이벤트 수신");

        controller?.OnEvadeEnd();
    }

    /*───────────────────────────────*
     * 무적 타이밍 이벤트
     *───────────────────────────────*/

    /// <summary>
    /// 무적 시작 (회피 중간 구간)
    /// </summary>
    public void InvincibleStart()
    {
        Debug.Log("[PlayerEvade] 무적 시작");

        controller?.OnInvincibleStart();
    }

    /// <summary>
    /// 무적 종료
    /// </summary>
    public void InvincibleEnd()
    {
        Debug.Log("[PlayerEvade] 무적 종료");

        controller?.OnInvincibleEnd();
    }
}

