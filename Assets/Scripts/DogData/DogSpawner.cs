using UnityEngine;

public class DogSpawner : MonoBehaviour
{
    public DogDatabase database;
    public GameObject dogPrefab;
    public bool registerSpawnedDogsToDex = true;

    public void SpawnDog(int id)
    {
        DogData data = database.GetDogByID(id);

        if (data == null)
        {
            Debug.LogError("Dog data not found: " + id);
            return;
        }

        GameObject obj = Instantiate(dogPrefab);
        Dog dog = obj.GetComponent<Dog>();
        dog.SetData(data);

        if (registerSpawnedDogsToDex)
        {
            bool isFirstAcquisition = DogDexManager.RecordDogAcquired(data.ID);
            if (isFirstAcquisition)
            {
                Debug.Log("Dog registered in dex: " + data.ID);
            }
        }
    }
}
