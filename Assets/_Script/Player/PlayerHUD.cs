using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 플레이어 상태 UI
/// - 체력 / 스태미나 표시
/// </summary>
public class PlayerHUD : MonoBehaviour
{
    [Header("참조")]
    public PlayerCore core;

    [Header("UI")]
    public Image hpFill;
    public Image staminaFill;

    int maxHP;
    int maxStamina;

    void Start()
    {
        if (core == null) return;

        maxHP = core.GetComponent<PlayerCore>().CurrentHP;
        maxStamina = core.GetComponent<PlayerCore>().CurrentStamina;
    }

    void Update()
    {
        if (core == null) return;

        hpFill.fillAmount = (float)core.CurrentHP / maxHP;
        staminaFill.fillAmount = (float)core.CurrentStamina / maxStamina;
    }
}
