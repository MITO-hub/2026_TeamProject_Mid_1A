using System.Collections.Generic;
using UnityEngine;

public class DogDexView : MonoBehaviour
{
    [SerializeField] private DogDexManager dexManager;
    [SerializeField] private DogDexSlotView slotPrefab;
    [SerializeField] private Transform slotParent;

    private readonly List<DogDexSlotView> slots = new List<DogDexSlotView>();

    private void Awake()
    {
        if (dexManager == null)
        {
            dexManager = DogDexManager.Instance;
        }
    }

    private void OnEnable()
    {
        if (dexManager == null)
        {
            dexManager = DogDexManager.Instance;
        }

        if (dexManager != null)
        {
            dexManager.DogUnlocked += HandleDogUnlocked;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (dexManager != null)
        {
            dexManager.DogUnlocked -= HandleDogUnlocked;
        }
    }

    public void Refresh()
    {
        if (dexManager == null || dexManager.Database == null || slotPrefab == null || slotParent == null)
        {
            return;
        }

        IReadOnlyList<DogDexEntry> entries = dexManager.Database.Entries;
        EnsureSlotCount(entries.Count);

        for (int i = 0; i < slots.Count; i++)
        {
            if (i >= entries.Count)
            {
                slots[i].gameObject.SetActive(false);
                continue;
            }

            DogDexEntry entry = entries[i];
            slots[i].SetEntry(entry, dexManager.IsUnlocked(entry.Id));
        }
    }

    private void EnsureSlotCount(int count)
    {
        while (slots.Count < count)
        {
            DogDexSlotView slot = Instantiate(slotPrefab, slotParent);
            slots.Add(slot);
        }
    }

    private void HandleDogUnlocked(int dogId)
    {
        Refresh();
    }
}
