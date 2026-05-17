using UnityEngine;

public class DogSpawner : MonoBehaviour
{
    public DogDatabase database;
    public GameObject dogPrefab;

    public void SpawnDog(int id)
    {
        DogData data = database.GetDogByID(id);

        if (data == null)
        {
            Debug.LogError("해당 ID의 강아지를 찾을 수 없습니다: " + id);
            return;
        }

        GameObject obj = Instantiate(dogPrefab);
        Dog dog = obj.GetComponent<Dog>();
        dog.SetData(data);
    }
}
