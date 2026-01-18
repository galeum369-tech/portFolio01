using UnityEngine;

public class LockOnTarget : MonoBehaviour
{
    [SerializeField] GameObject worldIndicator;

    public void Show()
    {
        if (worldIndicator != null)
            worldIndicator.SetActive(true);
    }

    public void Hide()
    {
        if (worldIndicator != null)
            worldIndicator.SetActive(false);
    }
}


