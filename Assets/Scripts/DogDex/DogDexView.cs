using System.Collections.Generic;
using UnityEngine;

public class DogDexView : MonoBehaviour
{
    [SerializeField] private DogDexManager dexManager;
    [SerializeField] private DogDexSlotView slotPrefab;
    [SerializeField] private Transform slotParent;
    [SerializeField] private bool showAllEntries;
    [SerializeField] private bool forceLockedSlotDisplay;

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

        CacheExistingSlots();

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
            bool isUnlocked = showAllEntries || dexManager.IsUnlocked(entry.Id);
            slots[i].SetAlwaysShowLockedView(forceLockedSlotDisplay);
            slots[i].SetEntry(entry, isUnlocked);
        }
    }

    private void EnsureSlotCount(int count)
    {
        while (slots.Count < count)
        {
            DogDexSlotView slot = Instantiate(slotPrefab, slotParent);
            slot.SetAlwaysShowLockedView(forceLockedSlotDisplay);
            slots.Add(slot);
        }
    }

    private void CacheExistingSlots()
    {
        if (slots.Count > 0 || slotParent == null)
        {
            return;
        }

        for (int i = 0; i < slotParent.childCount; i++)
        {
            DogDexSlotView slot = slotParent.GetChild(i).GetComponent<DogDexSlotView>();
            if (slot != null)
            {
                slot.SetAlwaysShowLockedView(forceLockedSlotDisplay);
                slots.Add(slot);
            }
        }
    }

    private void HandleDogUnlocked(int dogId)
    {
        Refresh();
    }
}
