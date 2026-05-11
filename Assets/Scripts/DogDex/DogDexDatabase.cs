using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DogDexDatabase", menuName = "Dog Dex/Database")]
public class DogDexDatabase : ScriptableObject
{
    [SerializeField] private List<DogDexEntry> entries = new List<DogDexEntry>();

    public IReadOnlyList<DogDexEntry> Entries => entries;

    public DogDexEntry GetEntry(int dogId)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].Id == dogId)
            {
                return entries[i];
            }
        }

        return null;
    }
}
