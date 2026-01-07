using UnityEngine;

public class PlayerWeaponState : MonoBehaviour
{
    [Header("애니메이터")]
    [SerializeField] Animator anim;

    int axeLayer;
    int swordLayer;
    int masicLayer;
    int weaponSwapHash;

    int nextWeapon; // 0:none, 1:axe, 2:sword
    bool isSwapping;

    [Header("무기 프리팹")]
    public GameObject axeEquip;
    public GameObject axeUnequip;
    public GameObject swordEquip;
    public GameObject swordUnequip;
    public GameObject masic;

    void Awake()
    {
        axeLayer = anim.GetLayerIndex("Axe");
        swordLayer = anim.GetLayerIndex("Sword");
        masicLayer = anim.GetLayerIndex("Masic");

        weaponSwapHash = Animator.StringToHash("WeaponSwap");

        axeEquip.SetActive(false);
        swordEquip.SetActive(false);
        masic.SetActive(false);
        axeUnequip.SetActive(true);
        swordUnequip.SetActive(true);
    }

    void OnEnable()
    {
        InputManager.OnWeaponNone += () => RequestEquip(0);
        InputManager.OnWeaponAxe += () => RequestEquip(1);
        InputManager.OnWeaponSword += () => RequestEquip(2);
        InputManager.OnMasic += () => RequestEquip(3);
    }

    void OnDisable()
    {
        InputManager.OnWeaponNone -= () => RequestEquip(0);
        InputManager.OnWeaponAxe -= () => RequestEquip(1);
        InputManager.OnWeaponSword -= () => RequestEquip(2);
        InputManager.OnMasic -= () => RequestEquip(3);
    }

    void RequestEquip(int weapon)
    {
        if (isSwapping) return;
        nextWeapon = weapon;
        isSwapping = true;
        anim.SetTrigger(weaponSwapHash);
    }

    // Animation Event
    public void OnWeaponSwapEvent()
    {
        // 전부 끄기
        axeEquip.SetActive(false);
        swordEquip.SetActive(false);
        masic.SetActive(false);
        axeUnequip.SetActive(true);
        swordUnequip.SetActive(true);

        anim.SetLayerWeight(axeLayer, 0f);
        anim.SetLayerWeight(swordLayer, 0f);
        anim.SetLayerWeight(masicLayer, 0f);

        switch (nextWeapon)
        {
            case 1: // Axe
                axeEquip.SetActive(true);
                axeUnequip.SetActive(false);
                anim.SetLayerWeight(axeLayer, 1f);
                break;

            case 2: // Sword
                swordEquip.SetActive(true);
                swordUnequip.SetActive(false);
                anim.SetLayerWeight(swordLayer, 1f);
                break;
            case 3: // Masic
                masic.SetActive(true);
                anim.SetLayerWeight(masicLayer, 1f);
                break;
        }
    }

    // Animation Event (끝)
    public void OnWeaponSwapEnd()
    {
        isSwapping = false;
    }
}

