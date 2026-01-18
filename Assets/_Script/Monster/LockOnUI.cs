using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// LockOnUI
/// 
/// [역할]
/// - 현재 락온된 몬스터를 화면 상에 표시
/// - 월드 → 스크린 좌표 변환
/// 
/// [소속]
/// - 메인 UI Canvas
/// </summary>
public class LockOnUI : MonoBehaviour
{
    [SerializeField] Image lockOnImage;
    [SerializeField] Vector3 worldOffset = new Vector3(0f, 1.5f, 0f);

    Transform target;
    Camera mainCam;

    void Awake()
    {
        mainCam = Camera.main;
        lockOnImage.enabled = false;
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 worldPos = target.position + worldOffset;
        Vector3 screenPos = mainCam.WorldToScreenPoint(worldPos);

        // 화면 뒤로 가면 숨김
        if (screenPos.z < 0f)
        {
            lockOnImage.enabled = false;
            return;
        }

        lockOnImage.enabled = true;
        lockOnImage.transform.position = screenPos;
    }

    /*───────────────────────────────*
     * 외부 연동
     *───────────────────────────────*/

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        lockOnImage.enabled = target != null;
    }

    public void ClearTarget()
    {
        target = null;
        lockOnImage.enabled = false;
    }
}
