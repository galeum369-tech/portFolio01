using UnityEngine;

public class WeaponSystem : MonoBehaviour
{
    [SerializeField] Animator animator;

    [Header("Weapon Prefabs")]
    public GameObject swordEquip;
    public GameObject swordUnequip;
    public GameObject axeEquip;
    public GameObject axeUnequip;

    WeaponType nextWeapon;

    public void RequestEquip(WeaponType type)
    {
        nextWeapon = type;
        animator.SetTrigger("WeaponSwap");
    }

    // 🔔 Animation Event (전환 애니 중간)
    public void OnWeaponSwapEvent()
    {
        // 전부 끄기
        swordEquip.SetActive(false);
        axeEquip.SetActive(false);
        swordUnequip.SetActive(true);
        axeUnequip.SetActive(true);

        switch (nextWeapon)
        {
            case WeaponType.Sword:
                swordEquip.SetActive(true);
                swordUnequip.SetActive(false);
                animator.SetLayerWeight(
                    animator.GetLayerIndex("Sword"), 1f);
                break;

            case WeaponType.Axe:
                axeEquip.SetActive(true);
                axeUnequip.SetActive(false);
                animator.SetLayerWeight(
                    animator.GetLayerIndex("Axe"), 1f);
                break;

            case WeaponType.None:
                // 맨손: Base만 사용
                break;
        }
    }
}

