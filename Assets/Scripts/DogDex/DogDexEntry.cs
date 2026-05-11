using System;
using UnityEngine;

[Serializable]
public class DogDexEntry
{
    [SerializeField] private int id;
    [SerializeField] private string displayName;
    [SerializeField] private int level;
    [SerializeField] private Sprite icon;

    public int Id => id;
    public string DisplayName => displayName;
    public int Level => level;
    public Sprite Icon => icon;
}
