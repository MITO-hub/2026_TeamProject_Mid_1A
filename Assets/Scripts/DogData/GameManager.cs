using UnityEngine;

public class GameManager : MonoBehaviour
{
    public DogSpawner spawner;

    void Start()
    {
        spawner.SpawnDog(1);
    }
}