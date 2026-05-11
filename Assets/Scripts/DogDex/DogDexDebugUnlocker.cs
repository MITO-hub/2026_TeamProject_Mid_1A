using UnityEngine;

public class DogDexDebugUnlocker : MonoBehaviour
{
    [SerializeField] private DogDexManager dexManager;
    [SerializeField] private int dogId = 1;

    private void Awake()
    {
        if (dexManager == null)
        {
            dexManager = DogDexManager.Instance;
        }
    }

    public void UnlockDog()
    {
        if (dexManager == null)
        {
            dexManager = DogDexManager.Instance;
        }

        if (dexManager != null)
        {
            dexManager.UnlockDog(dogId);
        }
    }

    public void ClearDex()
    {
        if (dexManager == null)
        {
            dexManager = DogDexManager.Instance;
        }

        if (dexManager != null)
        {
            dexManager.ClearDex();
        }
    }
}
