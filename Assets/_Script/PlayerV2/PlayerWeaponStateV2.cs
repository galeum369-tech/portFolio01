using UnityEngine;

/// <summary>
/// PlayerWeaponStateV2
/// 
/// [역할]
/// - 현재 무기 상태 관리 (근접 / 마법 / 비무장)
/// - 콤보 가능 여부 제공
/// - WeaponBase 제공 (있을 경우)
/// - 애니메이터 레이어 제어
/// 
/// ⚠ 공격 실행 ❌
/// ⚠ 입력 처리 ❌
/// </summary>
public class PlayerWeaponStateV2 : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] Animator anim;

    /*───────────────────────────────*
     * Animator Layer
     *───────────────────────────────*/
    int axeLayer;
    int swordLayer;
    int magicLayer;

    int weaponSwapHash;

    /*───────────────────────────────*
     * 상태
     *───────────────────────────────*/
    public WeaponBase CurrentWeapon { get; private set; }

    /// <summary>
    /// 현재 무기가 콤보 공격 가능한가?
    /// (마법 = false)
    /// </summary>
    public bool IsComboWeapon { get; private set; }

    bool isSwapping;
    int nextWeapon; // 0:none, 1:axe, 2:sword, 3:magic

    /*───────────────────────────────*
     * 무기 프리팹
     *───────────────────────────────*/
    [Header("Weapon Objects")]
    [SerializeField] GameObject axeEquip;
    [SerializeField] GameObject axeUnequip;
    [SerializeField] GameObject swordEquip;
    [SerializeField] GameObject swordUnequip;
    [SerializeField] GameObject magicObject;

    /*───────────────────────────────*
     * 초기화
     *───────────────────────────────*/
    void Awake()
    {
        axeLayer = anim.GetLayerIndex("Axe");
        swordLayer = anim.GetLayerIndex("Sword");
        magicLayer = anim.GetLayerIndex("Masic");

        weaponSwapHash = Animator.StringToHash("WeaponSwap");

        // 초기 상태
        DisableAllWeapons();
        axeUnequip.SetActive(true);
        swordUnequip.SetActive(true);

        IsComboWeapon = false;
        CurrentWeapon = null;
    }

    void OnEnable()
    {
        InputManager.OnWeaponNone += OnWeaponNone;
        InputManager.OnWeaponAxe += OnWeaponAxe;
        InputManager.OnWeaponSword += OnWeaponSword;
        InputManager.OnMasic += OnWeaponMagic;
    }

    void OnDisable()
    {
        InputManager.OnWeaponNone -= OnWeaponNone;
        InputManager.OnWeaponAxe -= OnWeaponAxe;
        InputManager.OnWeaponSword -= OnWeaponSword;
        InputManager.OnMasic -= OnWeaponMagic;
    }

    /*───────────────────────────────*
     * 입력 수신
     *───────────────────────────────*/
    void OnWeaponNone() => RequestEquip(0);
    void OnWeaponAxe() => RequestEquip(1);
    void OnWeaponSword() => RequestEquip(2);
    void OnWeaponMagic() => RequestEquip(3);

    void RequestEquip(int weapon)
    {
        if (isSwapping)
        {
            Debug.Log("[WeaponStateV2] 무기 교체 중 → 입력 무시");
            return;
        }

        nextWeapon = weapon;
        isSwapping = true;

        Debug.Log($"[WeaponStateV2] 무기 교체 요청: {weapon}");
        anim.SetTrigger(weaponSwapHash);
    }

    /*───────────────────────────────*
     * Animation Event
     *───────────────────────────────*/

    /// <summary>
    /// 무기 교체 실제 반영 시점
    /// </summary>
    public void OnWeaponSwapEvent()
    {
        Debug.Log("[WeaponStateV2] 무기 교체 적용");

        DisableAllWeapons();
        ResetAnimatorLayers();

        IsComboWeapon = true;
        CurrentWeapon = null;

        switch (nextWeapon)
        {
            case 0: // 비무장
                IsComboWeapon = true;
                CurrentWeapon = GetComponentInChildren<WeaponBase>();
                break;

            case 1: // 도끼
                axeEquip.SetActive(true);
                axeUnequip.SetActive(false);
                anim.SetLayerWeight(axeLayer, 1f);
                CurrentWeapon = axeEquip.GetComponent<WeaponBase>();
                break;

            case 2: // 검
                swordEquip.SetActive(true);
                swordUnequip.SetActive(false);
                anim.SetLayerWeight(swordLayer, 1f);
                CurrentWeapon = swordEquip.GetComponent<WeaponBase>();
                break;

            case 3: // 마법
                magicObject.SetActive(true);
                anim.SetLayerWeight(magicLayer, 1f);

                IsComboWeapon = false;
                CurrentWeapon = null;
                break;
        }

        Debug.Log($"[WeaponStateV2] 콤보 가능: {IsComboWeapon}");
    }

    /// <summary>
    /// 무기 교체 종료
    /// </summary>
    public void OnWeaponSwapEnd()
    {
        isSwapping = false;
        Debug.Log("[WeaponStateV2] 무기 교체 종료");
    }

    /*───────────────────────────────*
     * 내부 유틸
     *───────────────────────────────*/
    void DisableAllWeapons()
    {
        axeEquip.SetActive(false);
        swordEquip.SetActive(false);
        magicObject.SetActive(false);
    }

    void ResetAnimatorLayers()
    {
        anim.SetLayerWeight(axeLayer, 0f);
        anim.SetLayerWeight(swordLayer, 0f);
        anim.SetLayerWeight(magicLayer, 0f);
    }
}

