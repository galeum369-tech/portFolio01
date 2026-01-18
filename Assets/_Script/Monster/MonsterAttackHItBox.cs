using UnityEngine;

public class MonsterAttackHitBox : MonoBehaviour
{
    [SerializeField] float damage = 10f;
    [SerializeField] float poiseDamage = 10f;

    void OnTriggerEnter(Collider other)
    {
        PlayerCoreV2 player = other.GetComponentInParent<PlayerCoreV2>();
        if (player == null)
            return;

        player.TakeDamage((int)damage);
        // 필요 시 poise 처리
    }
}