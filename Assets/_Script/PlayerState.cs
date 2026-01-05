using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerState : MonoBehaviour
{
    Animator anim;

    int axeLayer;
    int swordLayer;

    WeaponType currentWeapon;

    WeaponSystem ws;

    //무기변경의 무기프리팹들
    public GameObject equipmentSword;
    public GameObject unEquipmentSword;
    public GameObject equipmentAxe;
    public GameObject unEquipmentAxe;


    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        axeLayer = anim.GetLayerIndex("Axe");
        swordLayer = anim.GetLayerIndex("Sword");

        anim.SetLayerWeight(0, 1f);
        anim.SetLayerWeight(axeLayer, 0f);
        anim.SetLayerWeight(swordLayer, 0f);

        equipmentSword.SetActive(false);
        equipmentAxe.SetActive(false);
        unEquipmentSword.SetActive(true);
        unEquipmentAxe.SetActive(true);
    }

    void Update()
    {
        WeaponSwap();
    }

    void WeaponSwap()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            ws.RequestEquip(currentWeapon);
            /*anim.SetLayerWeight(0, 1f);
            anim.SetLayerWeight(axeLayer, 0f);
            anim.SetLayerWeight(swordLayer, 0f);

            equipmentSword.SetActive(false);
            equipmentAxe.SetActive(false);
            unEquipmentSword.SetActive(true);
            unEquipmentAxe.SetActive(true);
            */
        }
        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            /*anim.SetLayerWeight(0, 1f);
            anim.SetLayerWeight(axeLayer, 1f);
            anim.SetLayerWeight(swordLayer, 0f);

            equipmentSword.SetActive(false);
            equipmentAxe.SetActive(true);
            unEquipmentSword.SetActive(true);
            unEquipmentAxe.SetActive(false);
            */
        }
        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            /*anim.SetLayerWeight(0, 1f);
            anim.SetLayerWeight(axeLayer, 0f);
            anim.SetLayerWeight(swordLayer, 1f);

            equipmentSword.SetActive(true);
            equipmentAxe.SetActive(false);
            unEquipmentSword.SetActive(false);
            unEquipmentAxe.SetActive(true);
            */
        }
    }
}
