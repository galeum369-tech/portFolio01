using UnityEngine;

public class CameraRootFollow : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] Vector3 offset = new Vector3(0f, 1.6f, 0f);

    void LateUpdate()
    {
        transform.position = player.position + offset;
    }
}
