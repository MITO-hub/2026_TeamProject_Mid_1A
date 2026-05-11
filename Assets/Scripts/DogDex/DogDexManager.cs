using System;
using System.Collections.Generic;
using UnityEngine;

public class DogDexManager : MonoBehaviour
{
    private const string SaveKey = "DogDexSaveData";

    public static DogDexManager Instance { get; private set; }

    [SerializeField] private DogDexDatabase database;

    private DogDexSaveData saveData = new DogDexSaveData();

    public event Action<int> DogUnlocked;

    public DogDexDatabase Database => database;
    public IReadOnlyList<int> UnlockedDogIds => saveData.unlockedDogIds;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    public bool IsUnlocked(int dogId)
    {
        return saveData.unlockedDogIds.Contains(dogId);
    }

    public bool UnlockDog(int dogId)
    {
        if (dogId <= 0 || IsUnlocked(dogId))
        {
            return false;
        }

        saveData.unlockedDogIds.Add(dogId);
        Save();
        DogUnlocked?.Invoke(dogId);
        return true;
    }

    public void ClearDex()
    {
        saveData.unlockedDogIds.Clear();
        Save();
    }

    private void Save()
    {
        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    private void Load()
    {
        string json = PlayerPrefs.GetString(SaveKey, string.Empty);
        if (string.IsNullOrEmpty(json))
        {
            saveData = new DogDexSaveData();
            return;
        }

        saveData = JsonUtility.FromJson<DogDexSaveData>(json) ?? new DogDexSaveData();
    }

    [Serializable]
    private class DogDexSaveData
    {
        public List<int> unlockedDogIds = new List<int>();
    }
}
