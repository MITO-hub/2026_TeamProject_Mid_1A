using UnityEngine;
using UnityEngine.UI;

public class DogDexDebugUnlocker : MonoBehaviour
{
    [SerializeField] private DogDexManager dexManager;
    [SerializeField] private DogDexView dexView;
    [SerializeField] private int dogId = 1;
    [SerializeField] private bool clearOnClick;

    private void Awake()
    {
        if (dexManager == null)
        {
            dexManager = DogDexManager.Instance;
        }

        if (dexView == null)
        {
            dexView = FindAnyObjectByType<DogDexView>();
        }

        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveListener(HandleClick);
            button.onClick.AddListener(HandleClick);
        }
    }

    private void HandleClick()
    {
        if (clearOnClick)
        {
            ClearDex();
            return;
        }

        UnlockDog();
    }

    public void UnlockDog()
    {
        if (dexManager == null)
        {
            dexManager = DogDexManager.Instance;
        }

        if (dexManager != null)
        {
            bool unlocked = dexManager.UnlockDog(dogId);
            Debug.Log($"Dog dex test unlock {dogId}. changed={unlocked}, unlocked={dexManager.IsUnlocked(dogId)}");
        }

        RefreshView();
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

        RefreshView();
    }

    private void RefreshView()
    {
        if (dexView == null)
        {
            dexView = FindAnyObjectByType<DogDexView>();
        }

        if (dexView != null)
        {
            dexView.Refresh();
        }

        DogDexSlotView[] slots = FindObjectsByType<DogDexSlotView>(FindObjectsSortMode.None);
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].Refresh(dexManager);
        }
    }
}
