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
    public IReadOnlyList<int> AcquiredDogIds => saveData.unlockedDogIds;
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

    public bool HasAcquiredDog(int dogId)
    {
        return IsUnlocked(dogId);
    }

    public bool UnlockDog(int dogId)
    {
        return RegisterAcquiredDog(dogId);
    }

    public bool RegisterAcquiredDog(int dogId)
    {
        if (!IsValidDogId(dogId) || IsUnlocked(dogId))
        {
            return false;
        }

        saveData.unlockedDogIds.Add(dogId);
        Save();
        DogUnlocked?.Invoke(dogId);
        return true;
    }

    public static bool RecordDogAcquired(int dogId)
    {
        if (Instance != null)
        {
            return Instance.RegisterAcquiredDog(dogId);
        }

        if (dogId <= 0)
        {
            return false;
        }

        DogDexSaveData data = LoadSaveData();
        if (data.unlockedDogIds.Contains(dogId))
        {
            return false;
        }

        data.unlockedDogIds.Add(dogId);
        SaveData(data);
        return true;
    }

    public void ClearDex()
    {
        saveData.unlockedDogIds.Clear();
        Save();
    }

    private void Save()
    {
        SaveData(saveData);
    }

    private void Load()
    {
        saveData = LoadSaveData();
        RemoveInvalidSavedIds();
    }

    private static void SaveData(DogDexSaveData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    private static DogDexSaveData LoadSaveData()
    {
        string json = PlayerPrefs.GetString(SaveKey, string.Empty);
        if (string.IsNullOrEmpty(json))
        {
            return new DogDexSaveData();
        }

        return JsonUtility.FromJson<DogDexSaveData>(json) ?? new DogDexSaveData();
    }

    private bool IsValidDogId(int dogId)
    {
        if (dogId <= 0)
        {
            return false;
        }

        return database == null || database.GetEntry(dogId) != null;
    }

    private void RemoveInvalidSavedIds()
    {
        bool removedAny = false;
        for (int i = saveData.unlockedDogIds.Count - 1; i >= 0; i--)
        {
            if (!IsValidDogId(saveData.unlockedDogIds[i]))
            {
                saveData.unlockedDogIds.RemoveAt(i);
                removedAny = true;
            }
        }

        if (removedAny)
        {
            Save();
        }
    }

    [Serializable]
    private class DogDexSaveData
    {
        public List<int> unlockedDogIds = new List<int>();
    }
}
